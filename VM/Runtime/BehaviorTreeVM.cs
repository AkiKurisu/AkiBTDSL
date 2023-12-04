using UnityEngine;
using Kurisu.AkiBT.DSL;
using System;
namespace Kurisu.AkiBT.VM
{
    /// <summary>
    /// Virtual Machine Component to Run VMBehaviorTreeSO
    /// </summary>
    public class BehaviorTreeVM : MonoBehaviour
    {
#if UNITY_EDITOR
        public TextAsset vmCode;
#endif
#if UNITY_EDITOR
        private Compiler compiler;
#else
        private BehaviorTreeCompiler compiler;
        private void Awake() {
            compiler=new BehaviorTreeCompiler();
        }
#endif
        [SerializeField]
        private bool isPlaying;
        public bool IsPlaying => isPlaying;
        [SerializeField]
        private BehaviorTreeSO behaviorTreeSO;
        public BehaviorTreeSO BehaviorTreeSO => behaviorTreeSO;
        /// <summary>
        /// Compile vmCode. if success, you will have a temporary VMBehaviorTreeSO inside this component.
        /// You can save this VMBehaviorTreeSO in the editor by clicking 'Save' button
        /// </summary>
        /// <param name="vmCode"></param>
        public void Compile(string vmCode)
        {
#if UNITY_EDITOR
            compiler = new();
#endif
            if (behaviorTreeSO != null) Clear();
            behaviorTreeSO = ScriptableObject.CreateInstance<BehaviorTreeSO>();
            try
            {
                behaviorTreeSO.Deserialize(compiler.Compile(vmCode));
            }
            catch (Exception e)
            {
                behaviorTreeSO = null;
#if UNITY_EDITOR
                throw e;
#else
                Debug.LogError(e);
#endif
            }
        }
        public void Clear()
        {
            behaviorTreeSO = null;
            isPlaying = false;
        }
        /// <summary>
        /// VMBehaviorTreeSO doesn't automatically awake and start, you need to run it manually
        /// </summary>
        public void Run()
        {
            if (behaviorTreeSO == null || isPlaying) return;
            behaviorTreeSO.Init(gameObject);
            isPlaying = true;
        }
        public void Stop()
        {
            isPlaying = false;
        }
        private void Update()
        {
            if (isPlaying) behaviorTreeSO.Update();
        }

    }
}
