using System;
using UnityEngine;
namespace Kurisu.AkiBT.Compiler
{
    internal class PropertyProcessor : Processor
    {
        private string name;
        public string PropertyName => name;
        private object value;
        private const string SharedToken = "=>";
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
            if (CurrentToken == SharedToken)
                isShared = true;
            else
            {
                isShared = false;
                if (CurrentToken != Scanner.Colon)
                {
                    throw new Exception($"<color=#ff2f2f>AkiBTCompiler</color> : Syntax error, pairing symbol not found '{Scanner.Colon}'");
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
