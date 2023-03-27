using System.Collections.Generic;
using System.IO;
using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
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
        private NodeTypeDictionary nodeTypeDict;
        internal NodeTypeFactory(string dictionaryName)
        {
            string fileInStreaming = $"{Application.streamingAssetsPath}/{dictionaryName}.json";
            #if !UNITY_EDITOR&&UNITY_ANDROID
                string fileInPersistent = $"{Application.persistentDataPath}/{dictionaryName}.json";
                StreamingToPersistant(fileInStreaming,fileInPersistent);
            #else
                if(!File.Exists(fileInStreaming))
                {
                    throw new ArgumentNullException(fileInStreaming);
                }
                Deserialize(fileInStreaming);
            #endif
        }
        /// <summary>
        /// Since we can't read streaming assets folder directly in android, we need to copy it to persistent folder.
        /// </summary>
        /// <param name="fileInStreaming"></param>
        /// <param name="fileInPersistent"></param>
        /// <returns></returns>
        private async void StreamingToPersistant(string fileInStreaming,string fileInPersistent)
        {
            if(File.Exists(fileInPersistent))
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
        private void Deserialize(string path)
        {
            nodeTypeDict=JsonConvert.DeserializeObject<NodeTypeDictionary>(File.ReadAllText(path));
        }
        private static async Task WaitRequestTask(UnityWebRequestAsyncOperation request)
        {
            while(!request.isDone)
            {
                await Task.Yield();
            }
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
