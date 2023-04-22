using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEngine;
using System;
using System.Collections.Generic;
namespace Kurisu.AkiBT.Compiler
{
    internal interface IReference
    {
        public int Rid{set;}
    }
    [System.Serializable]
    public struct Reference
    {
        public readonly int rid;
        public Reference(int rid)
        {
            this.rid=rid;
        }
    }
    [System.Serializable]
    internal class Node:IReference
    {
        /// <summary>
        /// ReferenceID由Compiler生成
        /// </summary>
        public int rid;
        public int Rid{set=>rid=value;}
        // <summary>
        // Type根据NodeType查找获得
        // </summary>
        public Dictionary<string,string> type;
        public Dictionary<string,object> data;
    }
    [System.Serializable]
    internal class ReferencedVariable:IReference
    {
        /// <summary>
        /// ReferenceID由Compiler生成
        /// </summary>
        public int rid;
        public int Rid{set=>rid=value;}
        // <summary>
        // Type根据NodeType查找获得
        // </summary>
        public Dictionary<string,string> type;
        public Dictionary<string,object> data=new Dictionary<string, object>();
    }
    [System.Serializable]
    internal class Variable
    {
        public bool isShared;
        public string mName=string.Empty;
        public object value;
    }
    public class AkiBTCompiler
    {
        public AkiBTCompiler(string noteTypeFactoryName)
        {
            factory=new NodeTypeFactory(noteTypeFactoryName);
        }
        public AkiBTCompiler()
        {
            factory=new NodeTypeFactory("AkiBTTypeDictionary");
        }
        private int currentID=1000;
        private Reference root;
        private List<Reference> variableReferences=new List<Reference>();
        private List<object> referencesCache=new List<object>();
        private const string Pattern= @"(\(|\)|\[|\,|\:|\]| |\n|\r|=>|\t)";
        private readonly NodeTypeFactory factory;
        private readonly Dictionary<string,Queue<Processor>>processorPool=new Dictionary<string, Queue<Processor>>();
        private readonly Dictionary<string,Queue<Processor>>recyclePool=new Dictionary<string, Queue<Processor>>();
        private Scanner scanner=new Scanner();
        /// <summary>
        /// Get AkiBTIL
        /// </summary>
        /// <param name="code">Input AkiBTCode</param>
        /// <returns>AkiBTIL</returns>
        public string Compile(string code)
        {
            Init();
            scanner.Init(this,Regex.Split(code,Pattern));
            using(GetProcessor<AutoProcessor>(this,scanner)){};
            var IL=new AkiBTIL(root,referencesCache,variableReferences);
            RecycleProcessor();
            return JsonConvert.SerializeObject(IL);
        }
        private void Init()
        {
            root=default;
            currentID=1000;
            variableReferences.Clear();
            referencesCache.Clear();
        }
        internal void RegisterRoot(Node node)
        {
            factory.GenerateType("Root",node);
            this.root=Register(node);
        }
        internal Reference RegisterNode(string nodeType,Node node)
        {
            factory.GenerateType(nodeType,node);
            return Register(node);
        }
        internal Reference RegisterReferencedVariable(string variableType,ReferencedVariable variable)
        {
            factory.GenerateType("Shared"+variableType,variable);
            var reference=Register(variable);
            variableReferences.Add(reference);
            return reference;
        }
        private Reference Register<T>(T data)where T:IReference
        {
            referencesCache.Add(data);
            var reference=new Reference(currentID);
            data.Rid=reference.rid;
            currentID++;
            return reference;
        }
        internal bool IsNode(string nodeType)
        {
            return factory.IsNode(nodeType);
        }
        internal T GetProcessor<T>(AkiBTCompiler compiler,Scanner scanner)where T : Processor, new()
        {
            var processor=GetProcessor<T>();
            processor.Init(compiler,scanner);
            return processor;
        }
        private T GetProcessor<T>() where T : Processor, new()
        {
            T obj;
            if (CheckCache<T>())
            {
                string name = typeof(T).FullName;
                obj = (T)recyclePool[name].Dequeue();
                return obj;
            }
            else
            {
                return new T();
            }
        }
        private bool CheckCache<T>()where T : Processor
        {
            string name = typeof(T).FullName;
            return recyclePool.ContainsKey(name) && recyclePool[name].Count > 0;
        }
        private void RecycleProcessor()
        {
            foreach(var pair in processorPool)
            {
                if(!recyclePool.ContainsKey(pair.Key))recyclePool.Add(pair.Key,new Queue<Processor>());
                recyclePool[pair.Key].Clear();
                int count=pair.Value.Count;
                while(count>0)
                {
                    recyclePool[pair.Key].Enqueue(pair.Value.Dequeue());
                    count--;
                }
            }
        }
        internal void PushProcessor(Processor processor)
        {
            string name = processor.GetType().FullName;
            if (!processorPool.ContainsKey(name))
            {
                processorPool.Add(name, new Queue<Processor>());
            }
            processorPool[name].Enqueue(processor);
        }
    }
}