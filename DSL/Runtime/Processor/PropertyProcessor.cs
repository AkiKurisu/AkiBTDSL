namespace Kurisu.AkiBT.DSL
{
    internal class PropertyProcessor : Processor
    {
        private string name;
        public string PropertyName => name;
        private object value;
        private bool isShared;
        public bool IsShared => isShared;
        protected sealed override void OnProcess()
        {
            isShared = false;
            if (CurrentIndex == TotalCount) return;
            GetPropertyName();
            GetPropertyValue();
        }
        internal (string, object) GetProperty()
        {
            return (name, value);
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
            using ValueProcessor processor = Process<ValueProcessor>();
            value = processor.GetPropertyValue();
        }
    }


}
