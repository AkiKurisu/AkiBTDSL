using System.Collections.Generic;
namespace Kurisu.AkiBT.DSL
{
    internal class ArrayProcessor : Processor
    {
        private readonly List<object> childCache = new();
        private bool endFlag;
        protected sealed override void OnProcess()
        {
            childCache.Clear();
            endFlag = false;
            while (CurrentIndex < TotalCount && !endFlag)
            {
                GetChild();
            }
        }
        private void GetChild()
        {
            Scanner.MoveNextNoSpace();
            Scanner.MoveBack();
            if (Compiler.IsNode(Scanner.Peek()))
            {
                using NodeProcessor processor = Process<NodeProcessor>();
                childCache.Add(processor.GetNode());
            }
            else
            {
                using ValueProcessor processor = Process<ValueProcessor>();
                childCache.Add(processor.GetPropertyValue());
            }
            Scanner.MoveNextNoSpace();
            //Validate end symbol
            if (CurrentToken == Symbol.RightBracket)
            {
                endFlag = true;
                return;
            }
            if (CurrentToken == Symbol.Comma)
            {
                return;
            }
            throw new CompileException("Syntax error, next valid character not found");
        }
        internal object[] GetArray()
        {
            return childCache.ToArray();
        }
    }
}
