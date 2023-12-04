using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Reflection;
using System.Linq;
namespace Kurisu.AkiBT.DSL.Editor
{
    /// <summary>
    /// Tools to split fields and function, header file can be provided for UGC side.
    /// User do not need to know implementation detail.
    /// </summary>
    public class CodeGenerator
    {
        [MenuItem("Tools/AkiBT/DSL/Code Generator")]
        public static void GenerateCode()
        {
            var scriptFolderPath = EditorUtility.OpenFolderPanel("Select Script Folder", "", "");
            if (string.IsNullOrEmpty(scriptFolderPath))
            {
                Debug.LogError("Script folder path is not selected!");
                return;
            }
            string[] scriptFiles = Directory.GetFiles(scriptFolderPath, "*.cs", SearchOption.AllDirectories);
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes());
            foreach (string scriptFile in scriptFiles)
            {
                string scriptContent = File.ReadAllText(scriptFile);
                //ScriptName should be class name
                string className = Path.GetFileNameWithoutExtension(scriptFile);
                //Find last field index
                Type scriptType = types.First(x => x.Name == className);
                FieldInfo[] fields = scriptType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                //Suppose class is public
                int classIndex = scriptContent.IndexOf("public class ");
                int namespaceEndIndex = scriptContent.IndexOf('{');
                int endIndex = GetLastFieldIndex(fields, scriptContent);
                string fieldContent = scriptContent[..endIndex];
                string methodContent = scriptContent[(endIndex + 1)..];
                //Replace to partial class
                string headerScriptContent = fieldContent.Replace("public class", "public partial class") + "\n}\n}";
                int indent = scriptContent.LastIndexOf('}', scriptContent.Length - 2) - scriptContent.LastIndexOf('\n', scriptContent.Length - 3) - 1;
                string bodyScriptContent = scriptContent[..(namespaceEndIndex + 1)] + "\n" + new string(' ', indent) + "public partial class " + className + "\n" + new string(' ', indent) + "{\n" + methodContent;
                string headerScriptPath = Path.GetDirectoryName(scriptFile) + "/" + Path.GetFileNameWithoutExtension(scriptFile) + "_Header.cs";
                string bodyScriptPath = Path.GetDirectoryName(scriptFile) + "/" + Path.GetFileNameWithoutExtension(scriptFile) + "_Body.cs";
                string backUpPath = Path.GetDirectoryName(scriptFile) + "/" + Path.GetFileNameWithoutExtension(scriptFile) + ".txt";
                //Add backup for original script
                File.WriteAllText(backUpPath, scriptContent);
                File.WriteAllText(headerScriptPath, headerScriptContent);
                File.WriteAllText(bodyScriptPath, bodyScriptContent);
                File.Delete(scriptFile);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Code generation complete!");
        }
        private static int GetLastFieldIndex(FieldInfo[] fieldInfos, string scriptContent)
        {
            int endIndex = -1;
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                int index = scriptContent.IndexOf(" " + fieldInfo.Name + ";");
                if (index != -1)
                {
                    index += fieldInfo.Name.Length + 2;
                    if (index > endIndex)
                    {
                        endIndex = index;
                    }
                }
            }
            return endIndex;
        }

    }
}