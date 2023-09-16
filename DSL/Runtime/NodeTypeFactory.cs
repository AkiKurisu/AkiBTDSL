using System.Collections.Generic;
using System.IO;
using System;
using Newtonsoft.Json;
using UnityEngine;
#if !UNITY_EDITOR && UNITY_ANDROID
using UnityEngine.Networking;
using System.Threading.Tasks;
#endif
using System.Linq;
namespace Kurisu.AkiBT.DSL
{
    public class NodeTypeInfo
    {
        public string className;
        public string ns;
        public string asm;
        public bool isVariable;
        public List<PropertyTypeInfo> properties;
    }
    public class PropertyTypeInfo
    {
        public string label;
        public string name;
        public bool isVariable;
    }
    internal class NodeTypeFactory
    {
        private class NodeTypeDictionary : Dictionary<string, NodeTypeInfo> { }
        private NodeTypeDictionary nodeTypeDict;
        private const string ClassKey = "class";
        private const string NameSpaceKey = "ns";
        private const string AssemblyKey = "asm";
        internal NodeTypeFactory(string dictionaryName)
        {
            string fileInStreaming = $"{Application.streamingAssetsPath}/{dictionaryName}.json";
#if !UNITY_EDITOR && UNITY_ANDROID
            string fileInPersistent = $"{Application.persistentDataPath}/{dictionaryName}.json";
            StreamingToPersistent(fileInStreaming,fileInPersistent);
#else
            if (!File.Exists(fileInStreaming))
            {
                throw new ArgumentNullException(fileInStreaming);
            }
            Deserialize(fileInStreaming);
#endif
        }
#if !UNITY_EDITOR && UNITY_ANDROID
        /// <summary>
        /// Since we can't read streaming assets folder directly in android, we need to copy it to persistent folder.
        /// </summary>
        /// <param name="fileInStreaming"></param>
        /// <param name="fileInPersistent"></param>
        /// <returns></returns>
        private async void StreamingToPersistent(string fileInStreaming, string fileInPersistent)
        {
            if (File.Exists(fileInPersistent))
            {
                Deserialize(fileInPersistent);
                return;
            }
            var request = UnityWebRequest.Get(fileInStreaming);
            var download = new DownloadHandlerFile(fileInPersistent);
            request.downloadHandler = download;
            await WaitRequestTask(request.SendWebRequest());
            Deserialize(fileInPersistent);
        }
        private static async Task WaitRequestTask(UnityWebRequestAsyncOperation request)
        {
            while (!request.isDone)
            {
                await Task.Yield();
            }
        }
#endif
        private void Deserialize(string path)
        {
            nodeTypeDict = JsonConvert.DeserializeObject<NodeTypeDictionary>(File.ReadAllText(path));
        }

        internal void GenerateType(string nodeType, Node node)
        {
            if (!nodeTypeDict.ContainsKey(nodeType))
            {
                throw new Exception($"<color=#ff2f2f>AkiBTCompiler</color> : Can't find node type: {nodeType} in the Type Dictionary!");
            }
            node.type[ClassKey] = nodeTypeDict[nodeType].className;
            node.type[NameSpaceKey] = nodeTypeDict[nodeType].ns;
            node.type[AssemblyKey] = nodeTypeDict[nodeType].asm;
            var list = node.data.Keys.ToArray();
            foreach (var key in list)
            {
                if (key == ClassKey || key == NameSpaceKey || key == AssemblyKey) continue;
                var property = nodeTypeDict[nodeType].properties?.FirstOrDefault(x => x.label == key || x.name == key);
                if (property != null)
                {
                    var data = node.data[key];
                    node.data.Remove(key);
                    node.data[property.name] = data;
                }
                else
                {
                    Debug.LogWarning($"<color=#ff2f2f>AkiBTCompiler</color> : Can't find property: {key} of node type: {nodeType} in type dictionary, value will be discarded!");
                    node.data.Remove(key);
                }
            }
        }
        internal bool IsVariable(string nodeType, string fieldLabel)
        {
            if (nodeTypeDict.ContainsKey(nodeType))
            {
                if (nodeTypeDict[nodeType].properties == null) return false;
                return nodeTypeDict[nodeType].properties.Any(x => x.label == fieldLabel && x.isVariable);
            }
            return false;
        }
        internal bool IsNode(string nodeType)
        {
            if (nodeTypeDict.ContainsKey(nodeType))
            {
                return !nodeTypeDict[nodeType].isVariable;
            }
            return false;
        }
        internal void GenerateType(string variableType, ReferencedVariable variable)
        {
            if (nodeTypeDict.ContainsKey(variableType))
            {
                variable.type[ClassKey] = nodeTypeDict[variableType].className;
                variable.type[NameSpaceKey] = nodeTypeDict[variableType].ns;
                variable.type[AssemblyKey] = nodeTypeDict[variableType].asm;
                return;
            }
            throw new Exception($"<color=#ff2f2f>AkiBTCompiler</color> : Can't find variable type: {variableType} in the type dictionary!");
        }
    }
}
