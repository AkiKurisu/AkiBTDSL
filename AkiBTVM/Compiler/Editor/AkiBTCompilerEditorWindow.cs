using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kurisu.AkiBT.Editor;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
namespace Kurisu.AkiBT.Compiler.Editor
{
    public class AkiBTCompilerEditorWindow : EditorWindow
    {   
        private class CompilerSetting
        {
            public bool usingBehaviorTreeSettingMask;
            public string editorName="AkiBT";
            public string inputCode=string.Empty;
            public string dictionaryName="AkiBTTypeDictionary";
        }
        private string inputCode{get=>setting.inputCode;set=>setting.inputCode=value;}
        private string outPutCode;
        private Vector2 m_ScrollPosition;
        private bool usingBehaviorTreeSettingMask{get=>setting.usingBehaviorTreeSettingMask;set=>setting.usingBehaviorTreeSettingMask=value;}
        private string editorName{get=>setting.editorName;set=>setting.editorName=value;}
        private string dictionaryName{get=>setting.dictionaryName;set=>setting.dictionaryName=value;}
        private AkiBTCompiler compiler;
        private const string KeyName="AkiBTCompilerSetting";
        private  CompilerSetting setting;
        [MenuItem("Tools/AkiBTVM/Compiler Editor")]
        public static void OpenEditor()
        {
            GetWindow<AkiBTCompilerEditorWindow>("AkiBT Compiler Editor");
        }
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
        private int state=2;
        private void OnEnable() {
            var data=EditorPrefs.GetString(KeyName);
            setting=JsonConvert.DeserializeObject<CompilerSetting>(data);
            if(setting==null)setting=new CompilerSetting();
        }
        private void OnDisable() {
            EditorPrefs.SetString(KeyName,JsonConvert.SerializeObject(setting));
        }
        private void GatherStyle()
        {
            textAreaStyle=new GUIStyle(EditorStyles.textArea){wordWrap=true};
            labelStyle=new GUIStyle(GUI.skin.label){richText=true};
        }
        private void OnGUI()
        {
            GatherStyle();
            m_ScrollPosition = BeginVerticalScrollView(m_ScrollPosition, false, GUI.skin.verticalScrollbar, "OL Box");
            EditorGUILayout.BeginVertical();
                GUILayout.Label("Input Code");
                inputCode=EditorGUILayout.TextArea(inputCode,textAreaStyle,GUILayout.MinHeight(200));
                GUILayout.Label("Output Data");
                outPutCode=EditorGUILayout.TextArea(outPutCode,textAreaStyle,GUILayout.MinHeight(100));
                var orgColor=GUI.backgroundColor;
                GUI.backgroundColor=new Color(140/255f, 160/255f, 250/255f);
                if(GUILayout.Button("Compile",GUILayout.MinHeight(25)))
                {
                    state=0;
                    state=Compile();
                }
                GUI.backgroundColor=orgColor;
                DrawResult(state);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Dictionary Name",labelStyle);
                dictionaryName=GUILayout.TextField(dictionaryName,GUILayout.MinWidth(100));
                GUILayout.Label("Using Editor Mask",labelStyle);
                GUI.enabled=usingBehaviorTreeSettingMask;
                editorName=GUILayout.TextField(editorName,GUILayout.MinWidth(50));
                GUI.enabled=true;
                usingBehaviorTreeSettingMask=EditorGUILayout.ToggleLeft(string.Empty,usingBehaviorTreeSettingMask,GUILayout.Width(20));
                GUI.backgroundColor=new Color(253/255f, 163/255f, 255/255f);
                if(GUILayout.Button("Create Type Dictionary"))
                {
                    EditorApplication.delayCall += GetTypeDict;
                }
                GUI.backgroundColor=orgColor;
                GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        private void DrawResult(int state)
        {
            if(state==1)
            {
                GUILayout.Label("<color=#3aff48>AkiBTCompiler</color> : Compile Success!",labelStyle);
            }
            if(state==0)
            {
                GUILayout.Label("<color=#ff2f2f>AkiBTCompiler</color> : Compile Fail!",labelStyle);
            }
            if(state==-1)
            {
                GUILayout.Label("<color=#ff2f2f>AkiBTCompiler</color> : Input Code Is Empty!",labelStyle);
            } 
            if(state==-2)
            {
                GUILayout.Label("<color=#ff2f2f>AkiBTCompiler</color> : Type Dictionary has not generated!",labelStyle);
            } 
        }
        private int Compile()
        {
            string fileInStreaming = $"{Application.streamingAssetsPath}/{dictionaryName}.json";
            if(!File.Exists(fileInStreaming))
            {
                return -2;
            }
            compiler=new AkiBTCompiler(dictionaryName);
            if(string.IsNullOrEmpty(inputCode))
            {
                return -1;
            }
            outPutCode=compiler.Compile(inputCode);
            return 1;
        }
        private async void GetTypeDict()
        {
            IEnumerable<Type> list=SearchUtility.FindSubClassTypes(typeof(NodeBehavior));
            string[] showGroups=null;
            string[] notShowGroups=null;
            if(usingBehaviorTreeSettingMask)(showGroups,notShowGroups)=BehaviorTreeSetting.GetMask(editorName);
            if(showGroups!=null)
            {
                for(int i=0;i<showGroups.Length;i++)Debug.Log($"Showing Group:{showGroups[i]}");
            }
            if(notShowGroups!=null)
            {
                for(int i=0;i<notShowGroups.Length;i++)Debug.Log($"Not Showing Group:{notShowGroups[i]}");
            }
            var groups=list.GroupsByAkiGroup();
            list=list.Except(groups.SelectMany(x=>x)).ToList();
            groups=groups.SelectGroup(showGroups).ExceptGroup(notShowGroups);
            list=list.Concat(groups.SelectMany(x=>x).Distinct()).Concat(SearchUtility.FindSubClassTypes(typeof(SharedVariable)));;
            var nodeDict=new Dictionary<string,NodeTypeInfo>();
            foreach(var type in list)
            {
                AddTypeInfo(nodeDict,type);
            }
            string path=$"{Application.streamingAssetsPath}/{dictionaryName}.json";
            if(!File.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }
            Debug.Log("<color=#3aff48>AkiBTCompiler</color> : Creating AkiBT Type Dictionary...");
            //Write to file
            await File.WriteAllTextAsync(path,JsonConvert.SerializeObject(nodeDict,Formatting.Indented),System.Text.Encoding.UTF8);
            Debug.Log($"<color=#3aff48>AkiBTCompiler</color> : Create Success, file path:{path}");
        }
        private static void AddTypeInfo(Dictionary<string,NodeTypeInfo> dict,Type type)
        {
            if(dict.ContainsKey(type.Name))
            {
                dict[$"{type.Namespace}.{type.Name}"]=GenerateTypeInfo(type);
            }
            else
            {
                dict[type.Name]=GenerateTypeInfo(type);
            }
        }
        /// <summary>
        /// You need to generate TypeDictionary before using AkiBTVM
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static NodeTypeInfo GenerateTypeInfo(Type type)
        {
            var info=new NodeTypeInfo();
            info["class"]=type.Name;
            info["ns"]=type.Namespace;
            info["asm"]=type.Assembly.GetName().Name;
            if(type.IsSubclassOf(typeof(SharedVariable)))
            {
                info["compileType"]="Variable";
                return info;
            }
            info["compileType"]="Node";
            type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(field => field.GetCustomAttribute<HideInEditorWindow>() == null)//根据Atrribute判断是否需要隐藏
                .Concat(GetAllFields(type))//Concat合并列表
                .Where(field => field.IsInitOnly == false)
                .ToList().ForEach((p) =>
                {
                    var label=p.GetCustomAttribute(typeof(AkiLabelAttribute), false) as AkiLabelAttribute;
                    info[label?.Title??p.Name]=p.Name;
                });
            return info;
        }
        private static IEnumerable<FieldInfo> GetAllFields(Type t)
        {
            if (t == null)
                return Enumerable.Empty<FieldInfo>();

            return t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(field => field.GetCustomAttribute<SerializeField>() != null||field.GetCustomAttribute<SerializeReference>() != null)
                .Where(field => field.GetCustomAttribute<HideInEditorWindow>() == null).Concat(GetAllFields(t.BaseType));//Concat合并列表
        }
    }
}