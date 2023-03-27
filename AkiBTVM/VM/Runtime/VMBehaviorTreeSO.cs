using System.Collections.Generic;
using Kurisu.AkiBT.Convertor;
namespace Kurisu.AkiBT.VM
{
    /// <summary>
    /// Implemented BehaviorTreeSO for injecting external Root and SharedVariables.
    /// You can use Serialization to convert it to normal BehaviorTreeSO or others.
    /// </summary>
    public class VMBehaviorTreeSO : BehaviorTreeSO
    {
        public void InitVM(BehaviorTreeTemplate template)
        {
            this.root=template.Root;
            this.sharedVariables=new List<SharedVariable>(template.Variables);
        }
    }
}
