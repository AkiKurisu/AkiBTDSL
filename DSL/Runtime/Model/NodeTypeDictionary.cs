using System.Collections.Generic;
using Newtonsoft.Json;
namespace Kurisu.AkiBT.DSL
{
    public class NodeTypeDictionary
    {
        public Dictionary<string, NodeTypeInfo> internalDictionary = new();
        public List<EnumInfo> enumInfos = new();

        public NodeTypeInfo this[string key] => internalDictionary[key];
        public bool ContainsKey(string key)
        {
            return internalDictionary.ContainsKey(key);
        }

        public bool TryGetValue(string key, out NodeTypeInfo value)
        {
            return internalDictionary.TryGetValue(key, out value);
        }
    }
    public static class CompileType
    {
        public const uint Property = 0;
        public const uint Variable = 1;
        public const uint Enum = 2;
    }
    public class NodeTypeInfo
    {
        public string className;
        public string ns;
        public string asm;
        [JsonIgnore]
        public bool IsVariable => compileType == CompileType.Variable;
        public uint compileType;
        public List<PropertyTypeInfo> properties;
    }
    public class EnumInfo
    {
        public string[] options;
        public int IndexOf(string option)
        {
            for (int i = 0; i < options.Length; i++)
            {
                if (options[i] == option) return i;
            }
            return -1;
        }
    }
    public class PropertyTypeInfo
    {
        public string label;
        public string name;
        public uint compileType;
        [JsonIgnore]
        public bool IsVariable => compileType == CompileType.Variable;
        [JsonIgnore]
        public bool IsEnum => compileType == CompileType.Enum;
        public int enumIndex;
    }
}
