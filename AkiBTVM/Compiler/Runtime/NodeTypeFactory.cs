using System.Collections.Generic;
using System.IO;
using System;
using Newtonsoft.Json;
namespace Kurisu.AkiBT.Compiler
{
    public class NodeTypeInfo:Dictionary<string,string>
    {
        
    }
    
    internal class NodeTypeFactory
    {
        private class NodeTypeDictionary:Dictionary<string,NodeTypeInfo>
        {
            
        }
        private readonly NodeTypeDictionary nodeTypeDict;
        internal NodeTypeFactory(string path)
        {
            if(!File.Exists(path))
            {
                throw new ArgumentNullException(path);

            }
            nodeTypeDict=JsonConvert.DeserializeObject<NodeTypeDictionary>(File.ReadAllText(path));
        }
        internal void GenerateType(string nodeType,Node node)
        {
            if(nodeTypeDict.ContainsKey(nodeType))
            {
                node.type=nodeTypeDict[nodeType];
                return;
            }
            throw new Exception($"Can't find NodeType:{nodeType} in the NodeTypeDictionary!");
        }
        internal void GenerateType(string variableType,ReferencedVariable variable)
        {
            if(nodeTypeDict.ContainsKey(variableType))
            {
                variable.type=nodeTypeDict[variableType];
                return;
            }
            throw new Exception($"Can't find VariableType:{variableType} in the NodeTypeDictionary!");
        }
    }
}
