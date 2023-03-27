using System.Collections.Generic;
using UnityEngine;
namespace Kurisu.AkiBT.Convertor
{
    public class BehaviorTreeTemplate
    {
        [SerializeReference]
        private List<SharedVariable> variables;
        [SerializeReference]
        private Root root;
        public List<SharedVariable> Variables=>variables;
        public Root Root=>root;
        internal BehaviorTreeTemplate(BehaviorTreeSO behaviorTreeSO)
        {
            variables=new List<SharedVariable>();
            foreach(var variable in behaviorTreeSO.SharedVariables)
            {
                variables.Add(variable.Clone() as SharedVariable);
            }
            root=behaviorTreeSO.Root;            
        }
    }
}
