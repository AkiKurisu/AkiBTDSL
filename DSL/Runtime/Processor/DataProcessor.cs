using System.Collections.Generic;
using UnityEngine;
namespace Kurisu.AkiBT.DSL
{
    internal class DataProcessor : Processor
    {
        public Dictionary<string, object> properties = new();
        private bool endFlag;
        protected sealed override void OnProcess()
        {
            endFlag = false;
            properties.Clear();
            while (CurrentIndex < TotalCount && !endFlag)
            {
                GetProperty();
            }
        }
        internal Dictionary<string, object> GetData()
        {
            return properties;
        }
        private void GetProperty()
        {
            using (PropertyProcessor propertyProcessor = Process<PropertyProcessor>())
            {
                var tuple = propertyProcessor.GetProperty();
                properties.Add(tuple.Item1, tuple.Item2);
            }
            Scanner.MoveNextNoSpace();
            //Validate end symbol
            if (CurrentToken == Symbol.RightParenthesis)
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
    }
}
