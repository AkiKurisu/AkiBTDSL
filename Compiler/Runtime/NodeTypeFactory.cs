using System.Collections.Generic;
using System.IO;
using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System.Threading.Tasks;
namespace Kurisu.AkiBT.Compiler
{
    public class NodeTypeInfo:Dictionary<string,string>{}
    internal class NodeTypeFactory
    {
        private class NodeTypeDictionary:Dictionary<string,NodeTypeInfo>{}
        private NodeTypeDictionary nodeTypeDict;
        private const string TypeKey="compileType";
        private const string Node="Node";
        private const string Variable="Variable";
        private const string ClassKey="class";
        private const string NameSpaceKey="ns";
        private const string AssemblyKey="as";
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
            if(!nodeTypeDict.ContainsKey(nodeType))
            {
                throw new Exception($"<color=#ff2f2f>AkiBTCompiler</color> : Can't find NodeType:{nodeType} in the Type Dictionary!");
            }
            node.type=new Dictionary<string, string>();
            node.type["class"]=nodeTypeDict[nodeType]["class"];
            node.type["ns"]=nodeTypeDict[nodeType]["ns"];
            node.type["asm"]=nodeTypeDict[nodeType]["asm"];
            var list=node.data.Keys.ToArray();
            foreach(var key in list)
            {
                if(key==ClassKey||key==NameSpaceKey||key==AssemblyKey)continue;
                if(nodeTypeDict[nodeType].ContainsKey(key))
                {
                    var data=node.data[key];
                    node.data.Remove(key);
                    node.data[nodeTypeDict[nodeType][key]]=data;
                }
                else
                {
                    Debug.LogWarning($"<color=#ff2f2f>AkiBTCompiler</color> : Can't find Property:{key} in Type Dictionary:{nodeType}, value will be discarded. ");
                    node.data.Remove(key);
                }
            }
        }
        internal bool IsNode(string nodeType)
        {
            if(nodeTypeDict.ContainsKey(nodeType))
            {
                return nodeTypeDict[nodeType][TypeKey]==Node;
            }
            return false;
        }
        internal void GenerateType(string variableType,ReferencedVariable variable)
        {
            if(nodeTypeDict.ContainsKey(variableType))
            {
                variable.type=nodeTypeDict[variableType];
                return;
            }
            throw new Exception($"<color=#ff2f2f>AkiBTCompiler</color> : Can't find VariableType:{variableType} in the Type Dictionary!");
        }
    }
}
