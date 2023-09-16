using System;
using System.Collections.Generic;
namespace Kurisu.AkiBT.DSL
{
    internal class ArrayProcessor : Processor
    {
        private readonly List<object> childCache = new();
        private enum ArrayProcessState
        {
            GetChild, Over
        }
        private ArrayProcessState processState;
        protected sealed override void OnInit()
        {
            childCache.Clear();
            processState = ArrayProcessState.GetChild;
            Process();
        }
        private void Process()
        {
            while (CurrentIndex < TotalCount)
            {
                switch (processState)
                {
                    case ArrayProcessState.GetChild:
                        {
                            GetChild();
                            break;
                        }
                    case ArrayProcessState.Over:
                        {
                            return;
                        }
                }
            }
        }
        private void GetChild()
        {
            Scanner.MoveNextNoSpace();
            if (Compiler.IsNode(CurrentToken))
            {
                Scanner.MoveBack();
                using NodeProcessor processor = Compiler.GetProcessor<NodeProcessor>(this);
                childCache.Add(processor.GetNode());
            }
            else
            {
                Scanner.MoveBack();
                using ValueProcessor processor = Compiler.GetProcessor<ValueProcessor>(this);
                childCache.Add(processor.GetPropertyValue());
            }
            Scanner.MoveNextNoSpace();
            if (CurrentToken == Scanner.RightBracket)
            {
                processState = ArrayProcessState.Over;
                return;
            }
            if (CurrentToken == Scanner.Comma)
            {
                return;
            }
            throw new Exception("<color=#ff2f2f>AkiBTCompiler</color> : Syntax error, next valid character not found");
        }
        internal object[] GetArray()
        {
            return childCache.ToArray();
        }
    }
}
