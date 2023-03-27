using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Kurisu.AkiBT.Editor;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.UIElements;

namespace Kurisu.AkiBT.Compiler.Editor
{
internal class AkiBTCompilerEditorWindow : EditorWindow
{
    private TextField inputField;
    private TextField outputField;
    private Button compileButton;
    private AkiBTCompiler compiler;
    /// <summary>
    /// You can change the file name for using differnt compile usage
    /// </summary>
    const string ReadFileName="AkiBTTypeDictionary";
    private Toggle usingBehaviorTreeSettingMask;
    private TextField editorName;
    private TextField dictionaryName;
    
    [MenuItem("Tools/AkiBTVM/Compiler Editor")]
    public static void OpenEditor()
    {
        GetWindow<AkiBTCompilerEditorWindow>("AkiBT Compiler Editor");
    }
    internal AkiBTCompilerEditorWindow()
    {
        try
        {
            compiler=new AkiBTCompiler($"{Application.streamingAssetsPath}/{ReadFileName}.json");
        }
        catch(ArgumentNullException)
        {
            AsyncInitCompiler();
        }
    }
    private async void AsyncInitCompiler()
    {
        await GetTypeDict();
        compiler=new AkiBTCompiler($"{Application.streamingAssetsPath}/{ReadFileName}.json");
        Debug.Log("AkiBT Compiler Inited Already!");
    }
    public void CreateGUI()
    {
        rootVisualElement.Add(GetTitleLabel("AkiBT Compiler"));
        //Input
        var scrollInput=new ScrollView();
        scrollInput.style.minHeight=200;
        inputField=new TextField();
        inputField.multiline=true;
        inputField.style.whiteSpace=WhiteSpace.Normal;
        inputField.style.minHeight=200;
        //Output
        var scrollOutput=new ScrollView();
        scrollOutput.style.minHeight=100;
        outputField=new TextField();
        outputField.style.minHeight=100;
        outputField.multiline=true;
        outputField.style.whiteSpace=WhiteSpace.Normal;
        //Add
        rootVisualElement.Add(GetTitleLabel("Input Code",15));
        scrollInput.Add(inputField);
        rootVisualElement.Add(scrollInput);
        rootVisualElement.Add(GetTitleLabel("Output Data",15));
        scrollOutput.Add(outputField);
        rootVisualElement.Add(scrollOutput);
        //Button
        compileButton=GetButton("Compile",new Color(140/255f, 160/255f, 250/255f),Compile);
        rootVisualElement.Add(compileButton);
        //Type Dictionary
        //Setting
        usingBehaviorTreeSettingMask=new Toggle("Using BehaviorTreeSetting Mask"){tooltip="Ignoring Node Group when creating dictionary using BehaviorTreeSetting"};
        usingBehaviorTreeSettingMask.style.fontSize=15;
        editorName=new TextField("Editor Name"){value="AkiBT",tooltip="EditorName to use for BehaviorTreeSetting"};
        editorName.style.fontSize=15;
        rootVisualElement.Add(usingBehaviorTreeSettingMask);
        rootVisualElement.Add(editorName);
        dictionaryName=new TextField("Dictionary Name"){value=ReadFileName,tooltip=$"Output Dictionary Name, compiler editor only read default name:'{ReadFileName}'"};
        dictionaryName.style.fontSize=15;
        rootVisualElement.Add(dictionaryName);
        //Button
        var button=GetButton("Create Type Dictionary",new Color(253/255f, 163/255f, 255/255f),async ()=>await GetTypeDict());
        rootVisualElement.Add(button);
    }
    private static Label GetTitleLabel(string text,int frontSize=20)
    {
        var label=new Label(text);
        label.style.fontSize=frontSize;
        return label;
    }
    private void Compile()
    {
        outputField.value=compiler.Compile(inputField.value);
    }
    private async Task GetTypeDict()
    {
        IEnumerable<Type> list=SearchUtility.FindSubClassTypes(typeof(NodeBehavior));
        string[] showGroups=null;
        string[] notShowGroups=null;
        if(usingBehaviorTreeSettingMask.value)(showGroups,notShowGroups)=BehaviorTreeSetting.GetMask(editorName.value);
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
        string path=$"{Application.streamingAssetsPath}/{dictionaryName.value}.json";
        if(!File.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath);
        }
        Debug.Log("Creating AkiBT Type Dictionary...");
        //Write to file
        await File.WriteAllTextAsync(path,JsonConvert.SerializeObject(nodeDict,Formatting.Indented),System.Text.Encoding.UTF8);
        Debug.Log($"Create Success, file path:{path}");
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
        return info;
    }
    private static Button GetButton(string text,Color? color=null,System.Action callBack=null)
    {
        var button=new Button();
        if(callBack!=null)button.clicked+=callBack;
        if(color.HasValue)button.style.backgroundColor=color.Value;
        button.text=text;
        button.style.fontSize=20;
        return button;
    }
}
}