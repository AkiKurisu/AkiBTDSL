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
        protected sealed override void OnInit()
        {
            processState=ValueProcessorState.GetValue;
            value=null;
            Process();
        }
        private void Process()
        {
            while(CurrentIndex<TotalCount)
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
            Scanner.MoveNextNoSpace();
            //检测是否存在Children
            if(CurrentToken==Scanner.LeftBracket)
            {
                using (ArrayProcessor processor=Compiler.GetProcessor<ArrayProcessor>(Compiler,Scanner))
                {
                    value=processor.GetArray();
                }
                processState=ValueProcessorState.Over;
                return;
            }
            //检测是否存在Child
            if(Scanner.IsNodeType())
            {
                Scanner.MoveBack();
                using (NodeProcessor processor=Compiler.GetProcessor<NodeProcessor>(Compiler,Scanner))
                {
                    value=processor.GetNode();
                }
                processState=ValueProcessorState.Over;
                return;
            }
            var variableType=Scanner.TryGetVariableType();
            if(variableType.HasValue)
            {
                Scanner.MoveBack();//回退
                using (VariableProcessor processor=Compiler.GetProcessor<VariableProcessor>(Compiler,Scanner))
                {
                    value=processor.GetVariable();
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
            int index=Scanner.CurrentIndex;
            if(Scanner.TryGetVector3(out Vector3 vector3))
            {
                value=vector3;
                return;
            }
            //失败回退
            Scanner.MoveTo(index);
            if(Scanner.TryGetVector2(out Vector2 vector2))
            {
                value=vector2;
                return;
            }
            //失败回退
            Scanner.MoveTo(index);
            value=CurrentToken;
        }
        internal object GetPropertyValue()
        {
            return value;
        }
    }
}
