using System.Text.RegularExpressions;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
namespace Kurisu.AkiBT.Compiler
{
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
    internal class Node
    {
        /// <summary>
        /// ReferenceID由Compiler生成
        /// </summary>
        public int rid;
        // <summary>
        // Type根据NodeType查找获得
        // </summary>
        public Dictionary<string,string> type;
        public Dictionary<string,object> data;
    }
    [System.Serializable]
    internal class ReferencedVariable
    {
        /// <summary>
        /// ReferenceID由Compiler生成
        /// </summary>
        public int rid;
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
        public AkiBTCompiler(string noteTypeFactoryPath)
        {
            factory=new NodeTypeFactory(noteTypeFactoryPath);
        }
        public AkiBTCompiler()
        {
            string path=Application.streamingAssetsPath+"/AkiBTTypeDictionary.json";
            factory=new NodeTypeFactory(path);
        }
        private int currentID=1000;
        private Reference root;
        private List<Reference> variableReferences=new List<Reference>();
        private List<object> referencesCache=new List<object>();
        private const string Pattern= @"(\(|\)|\[|\,|\:|\]| |\n|\r|=>|\t)";
        private readonly NodeTypeFactory factory;
        /// <summary>
        /// 输出AkiBTIL
        /// </summary>
        /// <param name="code">输入AkiBTCode</param>
        /// <returns>AkiBTIL</returns>
        public string Compile(string code)
        {
            Init();
            var tokens=Regex.Split(code,Pattern);
            for(int i=0;i<tokens.Length;i++)
            {
                Debug.Log($"{i}:{tokens[i]}");
            }
            new AutoProcessor(this,tokens,-1);
            var IL=new AkiBTIL(root,referencesCache,variableReferences);
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
            var reference=Register(node);
            node.rid=reference.rid;
            //Debug.Log($"获得Root,ReferenceID:{reference.rid}");
            this.root=reference;
        }
        internal Reference RegisterNode(string nodeType,Node node)
        {
            factory.GenerateType(nodeType,node);
            var reference=Register(node);
            node.rid=reference.rid;
            //Debug.Log($"获得Node,ReferenceID:{reference.rid}");
            return reference;
        }
        internal Reference RegisterReferencedVariable(string variableType,ReferencedVariable variable)
        {
            factory.GenerateType("Shared"+variableType,variable);
            var reference=Register(variable);
            variable.rid=reference.rid;
            variableReferences.Add(reference);
            //Debug.Log($"获得Variable,ReferenceID:{reference.rid}");
            return reference;
        }
        private Reference Register(object data)
        {
            referencesCache.Add(data);
            var reference=new Reference(currentID);
            currentID++;
            return reference;
        }
    }
}