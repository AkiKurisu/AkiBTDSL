using System;
using System.Collections.Generic;
namespace Kurisu.AkiBT.Compiler
{
    internal class DataProcessor:Processor
    {
        public Dictionary<string,object> properties;
        private enum DataProcessState
        {
            GetProperty,Over
        }
        private DataProcessState processState;
        internal DataProcessor(AkiBTCompiler compiler,string[] tokens,int currentIndex):base(compiler,tokens,currentIndex)
        {   
            properties=new Dictionary<string,object>();
            Process();
        }
        internal Dictionary<string,object> GetData()
        {
            //Debug.Log("输出Property:"+properties);
            return properties;
        }
        private void Process()
        {
            while(currentIndex<totalCount)
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
            var propertyProcessor=new PropertyProcessor(compiler,tokens,currentIndex);
            var tuple=propertyProcessor.GetProperty();
            properties.Add(tuple.Item1,tuple.Item2);
            currentIndex=propertyProcessor.CurrentIndex;
            CheckValidEnd();
        }
        private void CheckValidEnd()
        {
            NextNoSpace();
            if(CurrentToken==RightParenthesis)
            {
                processState=DataProcessState.Over;
                return;
            }
            if(CurrentToken==Comma)
            {
                return;
            }
            throw new Exception("语法错误,找不到下一个有效字符");
        }
    }
}
