using System.Collections.Generic;
namespace Kurisu.AkiBT.DSL
{
    internal class MainProcessor : Processor
    {
        private bool rootProcessed;
        protected sealed override void OnProcess()
        {
            rootProcessed = false;
            while (CurrentIndex < TotalCount - 1)
            {
                Scanner.MoveNextNoSpace();
                if (CurrentIndex >= TotalCount - 1) return;
                //Skip comment
                if (CurrentToken == Symbol.Comment)
                {
                    Scanner.MoveTo(Scanner.IndexOf(Symbol.Comment) + 1);
                    continue;
                }
                Scanner.MoveBack();
                if (Scanner.TryGetVariableType(Scanner.Peek(), out _, out _))
                {
                    Process<ReferencedVariableProcessor>().Dispose();
                    continue;
                }
                else if (Compiler.IsNode(Scanner.Peek()))
                {
                    using NodeProcessor nodeProcessor = Process<NodeProcessor>();
                    var reference = nodeProcessor.GetNode();
                    if (!rootProcessed)
                    {
                        rootProcessed = true;
                        ProcessRoot(reference);
                    }
                }
                else
                {
                    Scanner.MoveNext();
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
