using UnityEngine;
using Kurisu.AkiBT.Compiler;
using System;
namespace Kurisu.AkiBT.VM
{
    /// <summary>
    /// Virtual Machine Component to Run VMBehaviorTreeSO
    /// </summary>
    public class BehaviorTreeVM : MonoBehaviour
    {
        #if UNITY_EDITOR
        /// <summary>
        /// Editor Only, just cache last input in the editor
        /// </summary>
        [SerializeField]
        private string vmCode;
        #endif
        #if UNITY_EDITOR
            private AkiBTCompiler compiler=new AkiBTCompiler();
        #else
            private AkiBTCompiler compiler;
            private void Awake() {
                compiler=new AkiBTCompiler();
            }
        # endif
        [SerializeField]
        private bool isPlaying;
        public bool IsPlaying=>isPlaying;
        [SerializeField,Tooltip("覆盖运行时绑定的GameObject,默认为当前GameObject")]
        private GameObject runGameObjectOverride;
        [SerializeField]
        private BehaviorTreeSO behaviorTreeSO;
        public BehaviorTreeSO BehaviorTreeSO{get=>behaviorTreeSO;
            #if UNITY_EDITOR
                set=>behaviorTreeSO=value;
            #endif
        }
        /// <summary>
        /// Compile vmCode. if success, you will have a temporary VMBehaviorTreeSO inside this component.
        /// You can save this VMBehaviorTreeSO in the editor by clicking 'Save' button
        /// </summary>
        /// <param name="vmCode"></param>
        public void Compile(string vmCode)
        {
            if(behaviorTreeSO!=null)Clear();
            behaviorTreeSO=ScriptableObject.CreateInstance<BehaviorTreeSO>();
            try
            {
                behaviorTreeSO.Deserialize(compiler.Compile(vmCode));
            }
            catch(Exception e)
            {
                behaviorTreeSO=null;
                Debug.LogError(e);
            }
        }
        public void Clear()
        {
            behaviorTreeSO=null;
            isPlaying=false;
        }
        /// <summary>
        /// VMBehaviorTreeSO doesn't automatically awake and start, you need to run it manually
        /// </summary>
        public void Run()
        {
            if(behaviorTreeSO==null||isPlaying)return;
            behaviorTreeSO.Init(runGameObjectOverride!=null?runGameObjectOverride:gameObject);
            isPlaying=true;
        }
        public void Stop()
        {
            isPlaying=false;
        }
        private void Update() {
            if(isPlaying)behaviorTreeSO.Update();
        }
    
    }
}
