using System;
namespace Kurisu.AkiBT.Compiler
{
    internal class ValueProcessor : Processor
    {
        private enum ValueProcessorState
        {
            GetValue,Over
        }
        private ValueProcessorState processState;
        private object value;
        internal ValueProcessor(AkiBTCompiler compiler, string[] tokens, int currentIndex) : base(compiler, tokens, currentIndex)
        {
            Process();
        }
        private void Process()
        {
            while(currentIndex<totalCount)
            {
                switch(processState)
                {
                    case ValueProcessorState.GetValue:
                    {
                        GetValue();
                        break;
                    }
                    case ValueProcessorState.Over:
                    {
                        return;
                    }
                }
            }
        }

        private void GetValue()
        {
            NextNoSpace();
            //检测是否存在Children
            if(CurrentToken==LeftBracket)
            {
                using (ArrayProcessor processor=new ArrayProcessor(compiler,tokens,currentIndex))
                {
                    value=processor.GetArray();
                    currentIndex=processor.CurrentIndex;
                }
                processState=ValueProcessorState.Over;
                return;
            }
            //检测是否存在Child
            var type=TryGetNodeType();
            if(type.HasValue)
            {
                currentIndex--;
                using (NodeProcessor processor=new NodeProcessor(compiler,tokens,currentIndex))
                {
                    value=processor.GetNode();
                    currentIndex=processor.CurrentIndex;
                }
                processState=ValueProcessorState.Over;
                return;
            }
            var variableType=TryGetVariableType();
            if(variableType.HasValue)
            {
                currentIndex--;//回退
                using (VariableProcessor processor=new VariableProcessor(compiler,tokens,currentIndex))
                {
                    value=processor.GetVariable();
                    currentIndex=processor.CurrentIndex;
                }
                processState=ValueProcessorState.Over;
                return;
            }
            TryParseValue();
            processState=ValueProcessorState.Over;
        }
        private void TryParseValue()
        {
            //检测是否为数字
            if(Int32.TryParse(CurrentToken,out int intNum))
            {
                value=intNum;
                return;
            }
            if(float.TryParse(CurrentToken,out float floatNum))
            {
                value=floatNum;
                return;
            }
            if(bool.TryParse(CurrentToken,out bool boolValue))
            {
                value=boolValue;
                return;
            }
            int index=currentIndex;
            if(Vector3Helper.TryGetVector3(this,out Vector3 vector3))
            {
                value=vector3;
                return;
            }
            //失败回退
            currentIndex=index;
            if(Vector2Helper.TryGetVector2(this,out Vector2 vector2))
            {
                value=vector2;
                return;
            }
            //失败回退
            currentIndex=index;
            value=CurrentToken;
        }
        internal object GetPropertyValue()
        {
            return value;
        }
    }
}
