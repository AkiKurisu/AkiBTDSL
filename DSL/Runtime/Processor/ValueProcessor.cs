namespace Kurisu.AkiBT.DSL
{
    internal class ValueProcessor : Processor
    {
        private object value;
        protected sealed override void OnProcess()
        {
            value = null;
            if (CurrentIndex == TotalCount) return;
            GetValue();
        }
        private void GetValue()
        {
            Scanner.MoveNextNoSpace();
            //检测是否存在Children
            if (CurrentToken == Symbol.LeftBracket)
            {
                using ArrayProcessor processor = Process<ArrayProcessor>();
                value = processor.GetArray();
                return;
            }
            Scanner.MoveBack();
            //检测是否存在Child
            if (Compiler.IsNode(Scanner.Peek()))
            {
                using NodeProcessor processor = Process<NodeProcessor>();
                value = processor.GetNode();
                return;
            }
            var nodeType = GetLastProcessor<NodeProcessor>().NodeType;
            var fieldLabel = GetLastProcessor<PropertyProcessor>().PropertyName;
            if (Compiler.IsVariable(nodeType, fieldLabel))
            {
                using VariableProcessor processor = Process<VariableProcessor>();
                value = processor.GetVariable();
            }
            else
            {
                Scanner.MoveNext();
                value = Scanner.ParseValue();
            }
        }
        internal object GetPropertyValue()
        {
            return value;
        }
    }
}
