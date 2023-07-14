using System;
using System.Collections.Generic;
namespace Kurisu.AkiBT.Compiler
{
    internal class DataProcessor:Processor
    {
        public Dictionary<string,object> properties=new Dictionary<string, object>();
        private enum DataProcessState
        {
            GetProperty,Over
        }
        private DataProcessState processState;
        protected sealed override void OnInit()
        {
            processState=DataProcessState.GetProperty;
            properties.Clear();
            Process();
        }
        internal Dictionary<string,object> GetData()
        {
            return properties;
        }
        private void Process()
        {
            while(CurrentIndex<TotalCount)
            {
                switch(processState)
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
            using (PropertyProcessor propertyProcessor=Compiler.GetProcessor<PropertyProcessor>(Compiler,Scanner))
            {
                var tuple=propertyProcessor.GetProperty();
                properties.Add(tuple.Item1,tuple.Item2);
            }
            CheckValidEnd();
        }
        private void CheckValidEnd()
        {
            Scanner.MoveNextNoSpace();
            if(CurrentToken==Scanner.RightParenthesis)
            {
                processState=DataProcessState.Over;
                return;
            }
            if(CurrentToken==Scanner.Comma)
            {
                return;
            }
            throw new Exception("<color=#ff2f2f>AkiBTCompiler</color> : Syntax error, next valid character not found");
        }
    }
}
