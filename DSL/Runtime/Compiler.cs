using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Collections.Generic;
namespace Kurisu.AkiBT.DSL
{
    /// <summary>
    /// Simple compiler using recursive descent subroutine method
    /// </summary>
    public class Compiler
    {
        public Compiler(string noteTypeFactoryName)
        {
            factory = new NodeTypeFactory(noteTypeFactoryName);
        }
        public Compiler()
        {
            factory = new NodeTypeFactory("TypeDictionary");
        }
        private int currentID = 1000;
        private Reference root;
        private readonly List<Reference> variableReferences = new();
        private readonly List<object> referencesCache = new();
        private const string Pattern = @"(\(|\)|\[|\,|\:|\]| |\n|\r|=>|\t)";
        private readonly NodeTypeFactory factory;
        private readonly Dictionary<string, Queue<Processor>> processorPool = new();
        private readonly Dictionary<string, Queue<Processor>> recyclePool = new();
        private readonly Scanner scanner = new();
        /// <summary>
        /// Get BehaviorTreeSerializeReferenceData
        /// </summary>
        /// <param name="code">Input AkiBTCode</param>
        /// <returns>Output BehaviorTreeSerializeReferenceData</returns>
        public string Compile(string code)
        {
            Init();
            scanner.Init(Tokenize(code));
            Process<MainProcessor>(scanner).Dispose();
            RecycleProcessor();
            return JsonConvert.SerializeObject(new SerializeReferenceData(root, referencesCache, variableReferences));
        }
        private static string[] Tokenize(string code)
        {
            int start = 0;
            int flag = 0;
            var tokens = new List<string>();
            for (int i = 0; i < code.Length; ++i)
            {
                if (code[i] == '\"')
                {
                    if (flag == 0)
                    {
                        tokens.AddRange(Regex.Split(code[start..i], Pattern));
                        start = i + 1;
                        flag = 1;
                    }
                    else
                    {
                        tokens.Add(code[start..i]);
                        start = i + 1;
                        flag = 0;
                    }
                }
            }
            if (start < code.Length)
                tokens.AddRange(Regex.Split(code[(start - flag)..], Pattern));
            return tokens.ToArray();
        }
        private void Init()
        {
            root = default;
            currentID = 1000;
            variableReferences.Clear();
            referencesCache.Clear();
        }
        internal void RegisterRoot(Node node)
        {
            factory.GenerateType("Root", node);
            root = Register(node);
        }
        internal Reference RegisterNode(string nodeType, Node node)
        {
            factory.GenerateType(nodeType, node);
            return Register(node);
        }
        internal Reference RegisterReferencedVariable(string variableType, ReferencedVariable variable)
        {
            factory.GenerateType("Shared" + variableType, variable);
            var reference = Register(variable);
            variableReferences.Add(reference);
            return reference;
        }
        private Reference Register<T>(T data) where T : IReference
        {
            referencesCache.Add(data);
            var reference = new Reference(currentID);
            data.Rid = reference.rid;
            currentID++;
            return reference;
        }
        internal bool IsNode(string nodeType)
        {
            return factory.IsNode(nodeType);
        }
        internal bool IsVariable(string nodeType, string fieldLabel)
        {
            return factory.IsVariable(nodeType, fieldLabel);
        }
        private T Process<T>(Scanner scanner) where T : Processor, new()
        {
            var processor = GetProcessor<T>();
            processor.Process(this, scanner);
            return processor;
        }
        internal T Process<T>(Processor parentProcessor) where T : Processor, new()
        {
            var processor = GetProcessor<T>();
            processor.Process(parentProcessor);
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
        private bool CheckCache<T>() where T : Processor
        {
            string name = typeof(T).FullName;
            return recyclePool.ContainsKey(name) && recyclePool[name].Count > 0;
        }
        private void RecycleProcessor()
        {
            foreach (var pair in processorPool)
            {
                if (!recyclePool.ContainsKey(pair.Key)) recyclePool.Add(pair.Key, new Queue<Processor>());
                recyclePool[pair.Key].Clear();
                int count = pair.Value.Count;
                while (count > 0)
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