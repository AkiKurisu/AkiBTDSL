using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using System.IO;
using UnityEngine;
namespace Kurisu.AkiBT.DSL
{
    public interface ITypeContract
    {
        bool CanConvert(Type inputType, Type expectType);
        object Convert(in object value, Type inputType, Type expectType);
    }
    public class Vector3IntContract : ITypeContract
    {
        public bool CanConvert(Type inputType, Type expectType)
        {
            return (inputType == typeof(Vector3Int)) && expectType == typeof(Vector3);
        }

        public object Convert(in object value, Type inputType, Type expectType)
        {
            return (Vector3)(Vector3Int)value;
        }
    }
    public class Vector2IntContract : ITypeContract
    {
        public bool CanConvert(Type inputType, Type expectType)
        {
            return (inputType == typeof(Vector2Int)) && expectType == typeof(Vector2);
        }

        public object Convert(in object value, Type inputType, Type expectType)
        {
            return (Vector2)(Vector2Int)value;
        }
    }
    public class NodeTypeRegistry
    {
        [JsonProperty]
        private Dictionary<string, NodeInfo> NodeInfos { get; set; } = new();
        public static readonly HashSet<ITypeContract> contracts = new();
        static NodeTypeRegistry()
        {
            contracts.Add(new Vector2IntContract());
            contracts.Add(new Vector3IntContract());
        }
        public static NodeTypeRegistry FromPath(string path)
        {
            return JsonConvert.DeserializeObject<NodeTypeRegistry>(File.ReadAllText(path));
        }
        /// <summary>
        /// Get node meta data from node path
        /// </summary>
        /// <param name="nodePath"></param>
        /// <param name="metaData"></param>
        /// <returns></returns>
        public bool TryGetNode(string nodePath, out NodeInfo metaData)
        {
            if (NodeInfos.TryGetValue(nodePath, out metaData))
            {
                // Lazy call to cache all fields
                metaData.GetNodeType();
                return !metaData.isVariable;
            }
            return false;
        }
        /// <summary>
        /// Register a new node
        /// </summary>
        /// <param name="nodePath"></param>
        /// <param name="metaData"></param>
        public void SetNode(string nodePath, NodeInfo metaData)
        {
            NodeInfos[nodePath] = metaData;
        }
        public static object Cast(in object value, Type inputType, Type expectType)
        {
            foreach (var contract in contracts)
            {
                if (contract.CanConvert(inputType, expectType))
                {
                    return contract.Convert(value, inputType, expectType);
                }
            }
            return value;
        }
    }
    public class NodeInfo
    {
        public string className;
        public string ns;
        public string asm;
        public bool isVariable;
        public List<PropertyInfo> properties;
        private List<FieldInfo> fieldInfos;
        private Type type;
        public PropertyInfo GetProperty(string label)
        {
            return properties?.FirstOrDefault(x => x.label == label || x.name == label);
        }
        public Type GetNodeType()
        {
            type ??= Type.GetType(Assembly.CreateQualifiedName(asm, $"{ns}.{className}"));
            if (fieldInfos == null)
            {
                fieldInfos = GetAllFields(type).ToList();
                foreach (var property in properties)
                {
                    var field = fieldInfos.FirstOrDefault(x => x.Name == property.name);
                    if (field != null)
                        property.FieldInfo = field;
                }
            }
            return type;
        }
        private static IEnumerable<FieldInfo> GetAllFields(Type t)
        {
            if (t == null)
                return Enumerable.Empty<FieldInfo>();

            return t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                .Concat(GetAllFields(t.BaseType));
        }
    }
    public class PropertyInfo
    {
        public string label;
        public string name;
        public FieldType fieldType;
        [JsonIgnore]
        public bool IsVariable => fieldType == FieldType.Variable;
        [JsonIgnore]
        public bool IsEnum => fieldType == FieldType.Enum;
        [JsonIgnore]
        public FieldInfo FieldInfo { get; internal set; }
    }
}
