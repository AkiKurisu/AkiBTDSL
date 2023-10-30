using System.Collections.Generic;
namespace Kurisu.AkiBT.DSL
{
    internal class DataProcessor : Processor
    {
        public Dictionary<string, object> properties = new();
        private bool flag;
        protected sealed override void OnInit()
        {
            flag = true;
            properties.Clear();
            Process();
        }
        internal Dictionary<string, object> GetData()
        {
            return properties;
        }
        private void Process()
        {
            while (CurrentIndex < TotalCount && flag)
            {
                GetProperty();
            }
        }
        private void GetProperty()
        {
            using (PropertyProcessor propertyProcessor = Compiler.GetProcessor<PropertyProcessor>(this))
            {
                var tuple = propertyProcessor.GetProperty();
                properties.Add(tuple.Item1, tuple.Item2);
            }
            Scanner.MoveNextNoSpace();
            //Validate end symbol
            if (CurrentToken == Symbol.RightParenthesis)
            {
                flag = true;
                return;
            }
            if (CurrentToken == Symbol.Comma)
            {
                return;
            }
            throw new CompileException("Syntax error, next valid character not found");
        }
    }
}
