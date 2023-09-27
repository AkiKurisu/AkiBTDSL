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
    internal class NodeTypeFactory
    {
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
            if (!nodeTypeDict.TryGetValue(nodeType, out NodeTypeInfo nodeTypeInfo))
            {
                throw new CompileException($"Can't find node type: {nodeType} in the Type Dictionary!");
            }
            node.type[ClassKey] = nodeTypeInfo.className;
            node.type[NameSpaceKey] = nodeTypeInfo.ns;
            node.type[AssemblyKey] = nodeTypeInfo.asm;
            var list = node.data.Keys.ToArray();
            foreach (var key in list)
            {
                if (key == ClassKey || key == NameSpaceKey || key == AssemblyKey) continue;
                var property = nodeTypeInfo.properties?.FirstOrDefault(x => x.label == key || x.name == key);
                if (property != null)
                {
                    var data = node.data[key];
                    if (property.IsEnum && data is string value) data = nodeTypeDict.enumInfos[property.enumIndex].IndexOf(value);
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
            if (nodeTypeDict.TryGetValue(nodeType, out NodeTypeInfo nodeTypeInfo))
            {
                if (nodeTypeInfo.properties == null) return false;
                return nodeTypeInfo.properties.Any(x => x.label == fieldLabel && x.IsVariable);
            }
            return false;
        }
        internal bool IsNode(string nodeType)
        {
            if (nodeTypeDict.TryGetValue(nodeType, out NodeTypeInfo nodeTypeInfo))
            {
                return !nodeTypeInfo.IsVariable;
            }
            return false;
        }
        internal void GenerateType(string variableType, ReferencedVariable variable)
        {
            if (nodeTypeDict.TryGetValue(variableType, out NodeTypeInfo nodeTypeInfo))
            {
                variable.type[ClassKey] = nodeTypeInfo.className;
                variable.type[NameSpaceKey] = nodeTypeInfo.ns;
                variable.type[AssemblyKey] = nodeTypeInfo.asm;
                return;
            }
            throw new CompileException($"Can't find variable type: {variableType} in the type dictionary!");
        }
    }
}
