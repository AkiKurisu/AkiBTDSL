using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using UnityEngine;
using Kurisu.AkiBT.Editor;
using System.Reflection;
using Kurisu.AkiBT;
namespace Kurisu.AkiBT.VM.Editor
{
    [CustomEditor(typeof(BehaviorTreeVM))]
    public class BehaviorTreeVMEditor : UnityEditor.Editor
    {
        private TextField inputField;
        private Button createButton;
        private Button clearButton;
        private Button runButton;
        private Button stopButton;
        private BehaviorTreeVM vm;
        const string LabelText="AkiBTVM Version1.0";
        public override VisualElement CreateInspectorGUI()
        {
            vm=target as BehaviorTreeVM;
            VisualElement inspectorRoot = new VisualElement();
            inspectorRoot.Add(VMBehaviorTreeEditorUtility.GetTitleLabel(LabelText));
            InspectorElement.FillDefaultInspector(inspectorRoot, serializedObject, this);
            inspectorRoot.Remove(inspectorRoot.Q<PropertyField>("PropertyField:isPlaying"));
            inspectorRoot.Remove(inspectorRoot.Q<PropertyField>("PropertyField:behaviorTreeSO"));
            inspectorRoot.Remove(inspectorRoot.Q<PropertyField>("PropertyField:vmCode"));
            //Input
            inputField=new TextField(){bindingPath="vmCode"};
            inputField.multiline=true;
            inputField.style.whiteSpace=WhiteSpace.Normal;
            inputField.style.minHeight=200;
            inspectorRoot.Add(VMBehaviorTreeEditorUtility.GetTitleLabel("Input Code",15));
            inspectorRoot.Add(inputField);
            //Toggle
            var toggle=new Toggle("IsPlaying"){
                bindingPath="isPlaying"
            };
            inspectorRoot.Add(toggle);
            inspectorRoot.Add(new ObjectField("vmOutPut"){bindingPath="behaviorTreeSO",objectType=typeof(BehaviorTreeSO)});
            //group
            var group1=VMBehaviorTreeEditorUtility.GetGroup();
            var group2=VMBehaviorTreeEditorUtility.GetGroup();
            //Button
            createButton=VMBehaviorTreeEditorUtility.GetButton("Compile",callBack:Compile,widthPercent:100);
            inspectorRoot.Add(createButton);

            var saveButton=VMBehaviorTreeEditorUtility.GetButton("Save",callBack:Save);
            group1.Add(saveButton);

            clearButton=VMBehaviorTreeEditorUtility.GetButton("Clear",callBack:vm.Clear);
            group1.Add(clearButton);
            inspectorRoot.Add(group1);

            runButton=VMBehaviorTreeEditorUtility.GetButton("Run",new Color(140/255f, 160/255f, 250/255f),vm.Run);
            group2.Add(runButton);

            stopButton=VMBehaviorTreeEditorUtility.GetButton("Stop",new Color(253/255f, 163/255f, 255/255f),vm.Stop);
            group2.Add(stopButton);
            inspectorRoot.Add(group2);

            return inspectorRoot;
        }
        private void Save()
        {
            if(vm.BehaviorTreeSO==null)
            {
                Debug.Log("<color=#ff2f2f>AkiBTVM</color>:VMBehaviorTreeSO Hasn't Created Yet!");
                return;
            }
            string path=EditorUtility.OpenFolderPanel("选择保存路径",Application.dataPath,"");
            if(string.IsNullOrEmpty(path))return;
            var savePath=path.Replace(Application.dataPath,string.Empty);
            string outPutPath=$"Assets/{savePath}/{vm.gameObject.name}.asset";
            AssetDatabase.CreateAsset(vm.BehaviorTreeSO,outPutPath);
            AssetDatabase.SaveAssets();
            vm.BehaviorTreeSO=null;
            Debug.Log($"<color=#3aff48>AkiBTVM</color>:VMBehaviorTreeSO Saved Success! File Path:{outPutPath}");
        }
        private void Compile()
        {
            if(string.IsNullOrEmpty(inputField.value))
            {
                Debug.Log("<color=#ff2f2f>AkiBTVM</color>:Input Code Is Empty!");
                return;
            }
            try
            {
                vm.Compile(inputField.value);
                Debug.Log("<color=#3aff48>AkiBTVM</color>:Compile Success!");
            }
            catch(Exception e)
            {
                Debug.Log("<color=#ff2f2f>AkiBTVM</color>:Compile Fail!");
                throw e;
            }
        }
    }
    }
    internal class VMBehaviorTreeEditorUtility
    {
        internal static Foldout DrawSharedVariable(IBehaviorTree bt,FieldResolverFactory factory, UnityEngine.Object target,UnityEditor.Editor editor)
        {
            if(bt.SharedVariables.Count==0)return null;
            var foldout=new Foldout();
            foldout.value=false;
            foldout.text="SharedVariables";
            foreach(var variable in bt.SharedVariables)
            { 
                var grid=new Foldout();
                grid.text=$"{variable.GetType().Name}  :  {variable.Name}";
                grid.value=false;
                var content=new VisualElement();
                content.style.flexDirection=FlexDirection.Row;
                var valueField=factory.Create(variable.GetType().GetField("value",BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Public)).GetEditorField(bt.SharedVariables,variable);
                content.Add(valueField);
                var deleteButton=new Button(()=>{ 
                    bt.SharedVariables.Remove(variable);
                    foldout.Remove(grid);
                    EditorUtility.SetDirty(target);
                    EditorUtility.SetDirty(editor);
                    AssetDatabase.SaveAssets();
                });
                deleteButton.text="Delate";
                deleteButton.style.width=50;
                content.Add(deleteButton);
                grid.Add(content);
                foldout.Add(grid);   
            }
            return foldout;
        }
        internal static Button GetButton(string text,Color? color=null,System.Action callBack=null,float widthPercent=50)
        {
            var button=new Button();
            if(callBack!=null)button.clicked+=callBack;
            if(color.HasValue)button.style.backgroundColor=color.Value;
            button.style.width=Length.Percent(widthPercent);
            button.text=text;
            button.style.fontSize=20;
            return button;
        }
        internal static VisualElement GetGroup()
        {
            var group=new VisualElement();
            group.style.flexDirection=FlexDirection.Row;
            return group;
        }
        internal static Label GetTitleLabel(string text,int frontSize=20)
        {
            var label=new Label(text);
            label.style.fontSize=frontSize;
            return label;
        }
    }

