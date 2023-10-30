using System.Collections.Generic;
namespace Kurisu.AkiBT.DSL
{
    internal class MainProcessor : Processor
    {
        private bool rootProcessed;
        protected sealed override void OnInit()
        {
            rootProcessed = false;
            Process();
        }
        private void Process()
        {
            while (CurrentIndex < TotalCount - 1)
            {
                Scanner.MoveNextNoSpace();
                if (CurrentIndex >= TotalCount - 1) return;
                //Skip comment
                if (CurrentToken == Symbol.Comment)
                {
                    Scanner.MoveTo(Scanner.IndexOfNext(Symbol.Comment) + 1);
                    continue;
                }
                Scanner.MoveBack();
                if (Scanner.TryGetVariableType(Scanner.Peek).HasValue)
                {

                    using (ReferencedVariableProcessor processor = Compiler.GetProcessor<ReferencedVariableProcessor>(this)) { };
                    continue;
                }
                if (Compiler.IsNode(Scanner.Peek))
                {
                    using NodeProcessor nodeProcessor = Compiler.GetProcessor<NodeProcessor>(this);
                    var reference = nodeProcessor.GetNode();
                    if (!rootProcessed)
                    {
                        rootProcessed = true;
                        ProcessRoot(reference);
                    }
                    continue;
                }
            }
        }
        private void ProcessRoot(Reference reference)
        {
            var node = new Node
            {
                data = new Dictionary<string, object>()
            };
            node.data["child"] = reference;
            Compiler.RegisterRoot(node);
        }
    }
}
