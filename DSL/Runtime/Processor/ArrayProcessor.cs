using System.Collections.Generic;
namespace Kurisu.AkiBT.DSL
{
    internal class ArrayProcessor : Processor
    {
        private readonly List<object> childCache = new();
        private bool flag;
        protected sealed override void OnInit()
        {
            childCache.Clear();
            flag = true;
            Process();
        }
        private void Process()
        {
            while (CurrentIndex < TotalCount && flag)
            {
                GetChild();
            }
        }
        private void GetChild()
        {
            Scanner.MoveNextNoSpace();
            Scanner.MoveBack();
            if (Compiler.IsNode(Scanner.Peek))
            {
                using NodeProcessor processor = Compiler.GetProcessor<NodeProcessor>(this);
                childCache.Add(processor.GetNode());
            }
            else
            {
                using ValueProcessor processor = Compiler.GetProcessor<ValueProcessor>(this);
                childCache.Add(processor.GetPropertyValue());
            }
            Scanner.MoveNextNoSpace();
            //Validate end symbol
            if (CurrentToken == Symbol.RightBracket)
            {
                flag = false;
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
