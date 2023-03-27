using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
namespace Kurisu.AkiBT.Convertor.Editor
{
    internal class AkiBTConvertorEditorWindow : EditorWindow
    {
        private TextField outputField;
        private BehaviorTreeSO behaviorTreeSO;
        private Toggle indented;
       [MenuItem("Tools/AkiBTVM/Convertor Editor")]
        public static void OpenEditor()
        {
            GetWindow<AkiBTConvertorEditorWindow>("AkiBT Convertor Editor");
        }
        public void CreateGUI()
        {
            rootVisualElement.Add(GetTitleLabel("AkiBT Convertor"));
            var objectField=new ObjectField("Convert BehaviorTreeSO");
            objectField.objectType=typeof(BehaviorTreeSO);
            objectField.RegisterValueChangedCallback((obj)=>behaviorTreeSO=obj.newValue as BehaviorTreeSO);
            rootVisualElement.Add(objectField);
            //Output
            var scroll=new ScrollView();
            outputField=new TextField();
            outputField.style.minHeight=300;
            outputField.multiline=true;
            outputField.style.whiteSpace=WhiteSpace.Normal;
            //Add
            rootVisualElement.Add(GetTitleLabel("Output Code",15));
            scroll.Add(outputField);
            rootVisualElement.Add(scroll);
            //Toggle
            indented=new Toggle("Indented");
            rootVisualElement.Add(indented);
            //Button
            var button=GetButton("Convert",new Color(140/255f, 160/255f, 250/255f),Convert);
            rootVisualElement.Add(button);
        }
        private void Convert()
        {
            if(behaviorTreeSO==null){
                Debug.LogWarning("BehaviorTreeSO to convert is null!");
                return;
            }
            outputField.value=AkiBTConvertor.TreeToIL(behaviorTreeSO,indented.value);
        }
        private static Label GetTitleLabel(string text,int frontSize=20)
        {
            var label=new Label(text);
            label.style.fontSize=frontSize;
            return label;
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
