using System.Collections.Generic;
namespace Kurisu.AkiBT.DSL
{
    internal class DataProcessor : Processor
    {
        public Dictionary<string, object> properties = new();
        private enum DataProcessState
        {
            GetProperty, Over
        }
        private DataProcessState processState;
        protected sealed override void OnInit()
        {
            processState = DataProcessState.GetProperty;
            properties.Clear();
            Process();
        }
        internal Dictionary<string, object> GetData()
        {
            return properties;
        }
        private void Process()
        {
            while (CurrentIndex < TotalCount)
            {
                switch (processState)
                {
                    case DataProcessState.GetProperty:
                        {
                            GetProperty();
                            break;
                        }
                    case DataProcessState.Over:
                        {
                            return;
                        }
                }
            }
        }
        private void GetProperty()
        {
            using (PropertyProcessor propertyProcessor = Compiler.GetProcessor<PropertyProcessor>(this))
            {
                var tuple = propertyProcessor.GetProperty();
                properties.Add(tuple.Item1, tuple.Item2);
            }
            CheckValidEnd();
        }
        private void CheckValidEnd()
        {
            Scanner.MoveNextNoSpace();
            if (CurrentToken == Symbol.RightParenthesis)
            {
                processState = DataProcessState.Over;
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
