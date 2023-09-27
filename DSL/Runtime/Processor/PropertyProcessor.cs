namespace Kurisu.AkiBT.DSL
{
    internal class PropertyProcessor : Processor
    {
        private string name;
        public string PropertyName => name;
        private object value;
        private enum PropertyProcessState
        {
            PropertyName, PropertyValue, Over
        }
        private PropertyProcessState processState;
        private bool isShared;
        public bool IsShared => isShared;
        protected sealed override void OnInit()
        {
            isShared = false;
            processState = PropertyProcessState.PropertyName;
            Process();
        }
        internal (string, object) GetProperty()
        {
            return (name, value);
        }
        private void Process()
        {
            while (CurrentIndex < TotalCount)
            {
                switch (processState)
                {
                    case PropertyProcessState.PropertyName:
                        {
                            GetPropertyName();
                            break;
                        }
                    case PropertyProcessState.PropertyValue:
                        {
                            GetPropertyValue();
                            break;
                        }
                    case PropertyProcessState.Over:
                        {
                            return;
                        }
                }
            }
        }

        private void GetPropertyName()
        {
            Scanner.MoveNextNoSpace();
            name = CurrentToken;
            processState = PropertyProcessState.PropertyValue;
        }
        private void ValidateSyntax()
        {
            Scanner.MoveNextNoSpace();
            if (CurrentToken == Symbol.Shared)
                isShared = true;
            else
            {
                isShared = false;
                if (CurrentToken != Symbol.Colon)
                {
                    throw new CompileException($"Syntax error, pairing symbol not found '{Symbol.Colon}'");
                }
            }
        }

        private void GetPropertyValue()
        {
            ValidateSyntax();
            using (ValueProcessor processor = Compiler.GetProcessor<ValueProcessor>(this))
            {
                value = processor.GetPropertyValue();
            }
            processState = PropertyProcessState.Over;
        }
    }


}
