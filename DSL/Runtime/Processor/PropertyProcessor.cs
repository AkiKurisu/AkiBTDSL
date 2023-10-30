namespace Kurisu.AkiBT.DSL
{
    internal class PropertyProcessor : Processor
    {
        private string name;
        public string PropertyName => name;
        private object value;
        private bool isShared;
        public bool IsShared => isShared;
        protected sealed override void OnInit()
        {
            isShared = false;
            Process();
        }
        internal (string, object) GetProperty()
        {
            return (name, value);
        }
        private void Process()
        {
            if (CurrentIndex == TotalCount) return;
            GetPropertyName();
            GetPropertyValue();
        }

        private void GetPropertyName()
        {
            Scanner.MoveNextNoSpace();
            name = CurrentToken;
        }
        private void GetPropertyValue()
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
            using ValueProcessor processor = Compiler.GetProcessor<ValueProcessor>(this);
            value = processor.GetPropertyValue();
        }
    }


}
