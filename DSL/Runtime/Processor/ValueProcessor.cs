namespace Kurisu.AkiBT.DSL
{
    internal class ValueProcessor : Processor
    {
        private enum ValueProcessorState
        {
            GetValue, Over
        }
        private ValueProcessorState processState;
        private object value;
        protected sealed override void OnInit()
        {
            processState = ValueProcessorState.GetValue;
            value = null;
            Process();
        }
        private void Process()
        {
            while (CurrentIndex < TotalCount)
            {
                switch (processState)
                {
                    case ValueProcessorState.GetValue:
                        {
                            GetValue();
                            break;
                        }
                    case ValueProcessorState.Over:
                        {
                            return;
                        }
                }
            }
        }

        private void GetValue()
        {
            Scanner.MoveNextNoSpace();
            //检测是否存在Children
            if (CurrentToken == Symbol.LeftBracket)
            {
                using (ArrayProcessor processor = Compiler.GetProcessor<ArrayProcessor>(this))
                {
                    value = processor.GetArray();
                }
                processState = ValueProcessorState.Over;
                return;
            }
            //检测是否存在Child
            if (Compiler.IsNode(CurrentToken))
            {
                Scanner.MoveBack();
                using (NodeProcessor processor = Compiler.GetProcessor<NodeProcessor>(this))
                {
                    value = processor.GetNode();
                }
                processState = ValueProcessorState.Over;
                return;
            }
            var nodeType = GetLastProcessor<NodeProcessor>().NodeType;
            var fieldLabel = GetLastProcessor<PropertyProcessor>().PropertyName;
            if (Compiler.IsVariable(nodeType, fieldLabel))
            {
                Scanner.MoveBack();
                using (VariableProcessor processor = Compiler.GetProcessor<VariableProcessor>(this))
                {
                    value = processor.GetVariable();
                }
                processState = ValueProcessorState.Over;
                return;
            }
            value = Scanner.ParseValue();
            processState = ValueProcessorState.Over;
        }
        internal object GetPropertyValue()
        {
            return value;
        }
    }
}
