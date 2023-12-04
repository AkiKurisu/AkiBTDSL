using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kurisu.AkiBT.Editor;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
namespace Kurisu.AkiBT.DSL.Editor
{
    public class CompilerEditorWindow : EditorWindow
    {
        private class CompilerSetting
        {
            public bool usingBehaviorTreeSettingMask;
            public string editorName = "AkiBT";
            public string inputCode = string.Empty;
            public string dictionaryName = "TypeDictionary";
        }
        private string InputCode { get => setting.inputCode; set => setting.inputCode = value; }
        private string outPutCode;
        private Vector2 m_ScrollPosition;
        private Vector2 inputPosition;
        private bool UsingBehaviorTreeSettingMask { get => setting.usingBehaviorTreeSettingMask; set => setting.usingBehaviorTreeSettingMask = value; }
        private string EditorName { get => setting.editorName; set => setting.editorName = value; }
        private string DictionaryName { get => setting.dictionaryName; set => setting.dictionaryName = value; }
        private static string KeyName => Application.productName + "_AkiBT_DSL_CompilerSetting";
        private CompilerSetting setting;
        public delegate Vector2 BeginVerticalScrollViewFunc(Vector2 scrollPosition, bool alwaysShowVertical, GUIStyle verticalScrollbar, GUIStyle background, params GUILayoutOption[] options);
        static BeginVerticalScrollViewFunc s_func;
        static BeginVerticalScrollViewFunc BeginVerticalScrollView
        {
            get
            {
                if (s_func == null)
                {
                    var methods = typeof(EditorGUILayout).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Where(x => x.Name == "BeginVerticalScrollView").ToArray();
                    var method = methods.First(x => x.GetParameters()[1].ParameterType == typeof(bool));
                    s_func = (BeginVerticalScrollViewFunc)method.CreateDelegate(typeof(BeginVerticalScrollViewFunc));
                }
                return s_func;
            }
        }
        private GUIStyle textAreaStyle;
        private GUIStyle labelStyle;
        private int state = 2;
        private int mTab;
        [MenuItem("Tools/AkiBT/DSL/Compiler")]
        public static void OpenEditor()
        {
            GetWindow<CompilerEditorWindow>("AkiBT Compiler");
        }
        private void OnEnable()
        {
            var data = EditorPrefs.GetString(KeyName);
            setting = JsonConvert.DeserializeObject<CompilerSetting>(data);
            setting ??= new CompilerSetting();
        }
        private void OnDisable()
        {
            EditorPrefs.SetString(KeyName, JsonConvert.SerializeObject(setting));
        }
        private void GatherStyle()
        {
            textAreaStyle = new GUIStyle(GUI.skin.textArea) { wordWrap = true };
            labelStyle = new GUIStyle(GUI.skin.label) { richText = true };
        }
        private void OnGUI()
        {
            GatherStyle();
            int newTab = GUILayout.Toolbar(mTab, new string[] { "Compile", "Decompile" });
            if (newTab != mTab)
            {
                mTab = newTab;
                state = 2;
            }
            m_ScrollPosition = BeginVerticalScrollView(m_ScrollPosition, false, GUI.skin.verticalScrollbar, "OL Box");
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Input Code");
            inputPosition = BeginVerticalScrollView(inputPosition, false, GUI.skin.verticalScrollbar, "OL Box", GUILayout.Height(400));
            InputCode = EditorGUILayout.TextArea(InputCode, textAreaStyle);
            EditorGUILayout.EndScrollView();
            switch (mTab)
            {
                case 0:
                    DrawCompile();
                    break;
                case 1:
                    DrawDecompile();
                    break;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            DrawToolbar();
        }
        private void DrawCompile()
        {
            GUILayout.Label("Output Data");
            EditorGUILayout.TextArea(outPutCode, textAreaStyle, GUILayout.MinHeight(100));
            var orgColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(140 / 255f, 160 / 255f, 250 / 255f);
            if (GUILayout.Button("Compile", GUILayout.MinHeight(25)))
            {
                state = 0;
                state = Compile();
            }
            GUI.backgroundColor = new Color(253 / 255f, 163 / 255f, 255 / 255f);
            if (state == 1 && GUILayout.Button("Convert To BehaviorTreeSO", GUILayout.MinHeight(25)))
            {
                string path = EditorUtility.OpenFolderPanel("Select saving path", Application.dataPath, "");
                if (string.IsNullOrEmpty(path)) return;
                var behaviorTreeSO = CreateInstance<BehaviorTreeSO>();
                behaviorTreeSO.Deserialize(outPutCode);
                var savePath = path.Replace(Application.dataPath, string.Empty);
                string outPutPath = $"Assets/{savePath}/ConvertTreeSO.asset";
                AssetDatabase.CreateAsset(behaviorTreeSO, outPutPath);
                AssetDatabase.SaveAssets();
                Log($"BehaviorTreeSO saved succeed! File Path:{outPutPath}");
                GUIUtility.ExitGUI();
            }
            GUI.backgroundColor = orgColor;
            DrawCompileResult(state);
        }
        private void DrawDecompile()
        {
            DrawDragAndDrop();
            DrawDecompileResult(state);
            if (GUILayout.Button("Decompile All", GUILayout.MinHeight(25)))
            {
                string path = EditorUtility.OpenFolderPanel("Choose decompile files saving path", Application.dataPath, "");
                if (string.IsNullOrEmpty(path)) return;
                //Using AkiBT Service
                var serviceData = BehaviorTreeSetting.GetOrCreateSettings().ServiceData;
                serviceData.ForceSetUp();
                var decompiler = new Decompiler();
                foreach (var pair in serviceData.serializationCollection.serializationPairs)
                {
                    if (pair.behaviorTreeSO != null)
                    {
                        string data;
                        try
                        {
                            data = decompiler.Decompile(pair.behaviorTreeSO);
                        }
                        catch
                        {
                            Error($"Decompile failed with {pair.behaviorTreeSO.name}");
                            continue;
                        }
                        string folderPath = path + $"/{pair.behaviorTreeSO.GetType().Name}";
                        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                        string savePath = $"{folderPath}/{pair.behaviorTreeSO.name}_DSL.json";
                        File.WriteAllText(savePath, data);
                    }
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                GUIUtility.ExitGUI();
            }
        }
        private void DrawDragAndDrop()
        {

            GUIStyle StyleBox = new(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Italic,
                fontSize = 12
            };
            GUI.skin.box = StyleBox;
            Rect myRect = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
            GUI.Box(myRect, "Drag and Drop BehaviorTree to this Box!", StyleBox);
            {
                if (Event.current.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    Event.current.Use();

                }
                if (Event.current.type == EventType.DragPerform)
                {
                    if (DragAndDrop.objectReferences.Length > 0)
                    {
                        state = 0;
                        if (DragAndDrop.objectReferences[0] is GameObject gameObject)
                            state = Decompile(gameObject.GetComponent<IBehaviorTree>());
                        else if (DragAndDrop.objectReferences[0] is IBehaviorTree behaviorTree)
                            state = Decompile(behaviorTree);
                        else
                            state = -1;
                    }
                }
            }
        }
        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Dictionary Name", labelStyle);
            DictionaryName = GUILayout.TextField(DictionaryName, GUILayout.MinWidth(100));
            GUILayout.Label("Using Editor Mask", labelStyle);
            GUI.enabled = UsingBehaviorTreeSettingMask;
            EditorName = GUILayout.TextField(EditorName, GUILayout.MinWidth(50));
            GUI.enabled = true;
            UsingBehaviorTreeSettingMask = EditorGUILayout.ToggleLeft(string.Empty, UsingBehaviorTreeSettingMask, GUILayout.Width(20));
            var orgColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(253 / 255f, 163 / 255f, 255 / 255f);
            if (GUILayout.Button("Create Type Dictionary"))
            {
                EditorApplication.delayCall += GetTypeDict;
            }
            GUI.backgroundColor = orgColor;
            GUILayout.EndHorizontal();
        }
        private void DrawCompileResult(int state)
        {
            if (state == 1)
            {
                GUILayout.Label("<color=#3aff48>AkiBTCompiler</color> : Compile Succeed!", labelStyle);
            }
            if (state == 0)
            {
                GUILayout.Label("<color=#ff2f2f>AkiBTCompiler</color> : Compile Failed!", labelStyle);
            }
            if (state == -1)
            {
                GUILayout.Label("<color=#ff2f2f>AkiBTCompiler</color> : Input Code Is Empty!", labelStyle);
            }
            if (state == -2)
            {
                GUILayout.Label("<color=#ff2f2f>AkiBTCompiler</color> : Type Dictionary has not generated!", labelStyle);
            }
        }
        private void DrawDecompileResult(int state)
        {
            if (state == 1)
            {
                GUILayout.Label("<color=#3aff48>AkiBTDecompiler</color> : Decompile Succeed!", labelStyle);
            }
            if (state == 0)
            {
                GUILayout.Label("<color=#ff2f2f>AkiBTDecompiler</color> : Decompile Failed!", labelStyle);
            }
            if (state == -1)
            {
                GUILayout.Label("<color=#ff2f2f>AkiBTDecompiler</color> : Input Object's type is not supported!", labelStyle);
            }
            if (state == -2)
            {
                GUILayout.Label("<color=#ff2f2f>AkiBTDecompiler</color> : Type Dictionary has not generated!", labelStyle);
            }
        }
        private int Compile()
        {
            string fileInStreaming = $"{Application.streamingAssetsPath}/{DictionaryName}.json";
            if (!File.Exists(fileInStreaming))
            {
                return -2;
            }
            if (string.IsNullOrEmpty(InputCode))
            {
                return -1;
            }
            outPutCode = new Compiler(DictionaryName).Compile(InputCode);
            return 1;
        }
        private int Decompile(IBehaviorTree behaviorTree)
        {
            string fileInStreaming = $"{Application.streamingAssetsPath}/{DictionaryName}.json";
            if (!File.Exists(fileInStreaming))
            {
                return -2;
            }
            if (behaviorTree == null) return -1;
            InputCode = new Decompiler().Decompile(behaviorTree);
            return 1;
        }
        private void GetTypeDict()
        {
            IEnumerable<Type> list = SubclassSearchUtility.FindSubClassTypes(typeof(NodeBehavior));
            string[] showGroups = null;
            string[] notShowGroups = null;
            if (UsingBehaviorTreeSettingMask) (showGroups, notShowGroups) = BehaviorTreeSetting.GetMask(EditorName);
            if (showGroups != null)
            {
                for (int i = 0; i < showGroups.Length; i++) Log($"Showing Group:{showGroups[i]}");
            }
            if (notShowGroups != null)
            {
                for (int i = 0; i < notShowGroups.Length; i++) Log($"Not Showing Group:{notShowGroups[i]}");
            }
            var groups = list.GroupsByAkiGroup();
            list = list.Except(groups.SelectMany(x => x)).ToList();
            groups = groups.SelectGroup(showGroups).ExceptGroup(notShowGroups);
            list = list.Concat(groups.SelectMany(x => x).Distinct()).Concat(SubclassSearchUtility.FindSubClassTypes(typeof(SharedVariable))); ;
            var nodeDict = new NodeTypeDictionary();
            foreach (var type in list)
            {
                AddTypeInfo(nodeDict, type);
            }
            string path = $"{Application.streamingAssetsPath}/{DictionaryName}.json";
            if (!File.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }
            //Write to file
            File.WriteAllText(path, JsonConvert.SerializeObject(nodeDict, Formatting.Indented), System.Text.Encoding.UTF8);
            Log($"Create succeed, file saving path:{path}");
        }
        private static void AddTypeInfo(NodeTypeDictionary dict, Type type)
        {
            if (dict.ContainsKey(type.Name))
            {
                Log($"{type.Name} already exits, label will be replaced with {type.Namespace}.{type.Name}");
                dict.internalDictionary[$"{type.Namespace}.{type.Name}"] = GenerateTypeInfo(dict, type);
            }
            else
            {
                dict.internalDictionary[type.Name] = GenerateTypeInfo(dict, type);
            }
        }
        /// <summary>
        /// You need to generate TypeDictionary before using compiler
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static NodeTypeInfo GenerateTypeInfo(NodeTypeDictionary dict, Type type)
        {
            var enumDict = new Dictionary<Type, int>();
            var info = new NodeTypeInfo
            {
                className = type.Name,
                ns = type.Namespace,
                asm = type.Assembly.GetName().Name
            };
            if (type.IsSubclassOf(typeof(SharedVariable)))
            {
                info.compileType = CompileType.Variable;
                return info;
            }
            info.compileType = CompileType.Property;
            var properties = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                  .Concat(GetAllFields(type))
                  .Where(field => field.IsInitOnly == false && field.GetCustomAttribute<HideInEditorWindow>() == null)
                  .ToList();
            if (properties.Count == 0) return info;
            info.properties = new();
            properties.ForEach((p) =>
                {
                    var label = p.GetCustomAttribute(typeof(AkiLabelAttribute), false) as AkiLabelAttribute;
                    PropertyTypeInfo propertyInfo;
                    info.properties.Add(propertyInfo = new PropertyTypeInfo()
                    {
                        name = p.Name,
                        label = label?.Title ?? p.Name,
                        compileType = GetCompileType(p.FieldType)
                    });
                    if (propertyInfo.IsEnum)
                    {
                        if (!enumDict.ContainsKey(p.FieldType))
                        {
                            enumDict.Add(p.FieldType, dict.enumInfos.Count);
                            dict.enumInfos.Add(new EnumInfo()
                            {
                                options = Enum.GetNames(p.FieldType)
                            });
                        }
                        propertyInfo.enumIndex = enumDict[p.FieldType];
                    }
                });
            return info;
        }
        private static uint GetCompileType(Type type)
        {
            if (type.IsSubclassOf(typeof(SharedVariable))) return CompileType.Variable;
            if (type.IsEnum) return CompileType.Enum;
            return CompileType.Property;
        }
        private static IEnumerable<FieldInfo> GetAllFields(Type t)
        {
            if (t == null)
                return Enumerable.Empty<FieldInfo>();

            return t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(field => field.GetCustomAttribute<SerializeField>() != null || field.GetCustomAttribute<SerializeReference>() != null)
                .Concat(GetAllFields(t.BaseType));
        }
        private static void Log(string message)
        {
            Debug.Log($"<color=#3aff48>AkiBTCompiler</color> : {message}");
        }
        private static void Error(string message)
        {
            Debug.Log($"<color=#ff2f2f>AkiBTCompiler</color> : {message}");
        }
    }
}