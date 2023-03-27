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
        internal PropertyProcessor(AkiBTCompiler compiler,string[] tokens,int currentIndex):base(compiler,tokens,currentIndex)
        {
            Process();
        }
        internal (string,object) GetProperty()
        {
            return (name,value);
        }
        private void Process()
        {
            while(currentIndex<totalCount)
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
            NextNoSpace();
            name=CurrentToken;
            CheckValidPair();
        }
        /// <summary>
        /// 检测是否有配对符号':'
        /// </summary>
        private void CheckValidPair()
        {
            NextNoSpace();
            try
            {
                FindToken(Colon);
            }
            catch
            {
                throw new Exception($"语法错误,找不到配对符号'{Colon}'");
            }
            processState=PropertyProcessState.PropertyValue;
        }

        private void GetPropertyValue()
        {
            using (ValueProcessor processor=new ValueProcessor(compiler,tokens,currentIndex))
            {
                value=processor.GetPropertyValue();
                currentIndex=processor.CurrentIndex;
            }
            processState=PropertyProcessState.Over;
        }
    }
    
    
}
