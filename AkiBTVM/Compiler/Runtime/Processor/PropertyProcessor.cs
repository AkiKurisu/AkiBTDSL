using System;
namespace Kurisu.AkiBT.Compiler
{
    internal class PropertyProcessor:Processor
    {
        private string name;
        private object value;
        private enum PropertyProcessState
        {
            PropertyName,PropertyValue,Over
        }
        private PropertyProcessState processState;
        protected sealed override void OnInit()
        {
            processState=PropertyProcessState.PropertyName;
            Process();
        }
        internal (string,object) GetProperty()
        {
            return (name,value);
        }
        private void Process()
        {
            while(CurrentIndex<TotalCount)
            {
                switch(processState)
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
            name=CurrentToken;
            CheckValidPair();
        }
        /// <summary>
        /// 检测是否有配对符号':'
        /// </summary>
        private void CheckValidPair()
        {
            Scanner.MoveNextNoSpace();
            try
            {
                Scanner.FindToken(Scanner.Colon);
            }
            catch
            {
                throw new Exception($"语法错误,找不到配对符号'{Scanner.Colon}'");
            }
            processState=PropertyProcessState.PropertyValue;
        }

        private void GetPropertyValue()
        {
            using (ValueProcessor processor=Compiler.GetProcessor<ValueProcessor>(Compiler,Scanner))
            {
                value=processor.GetPropertyValue();
            }
            processState=PropertyProcessState.Over;
        }
    }
    
    
}
