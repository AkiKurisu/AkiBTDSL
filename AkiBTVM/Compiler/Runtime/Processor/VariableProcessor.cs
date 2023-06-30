using System;
namespace Kurisu.AkiBT.Compiler
{
    internal class VariableProcessor : Processor
    {
        private enum VariableProcessState
        {
            GetType,IsShared,GetValue,Over
        }
        private const string SharedToken="=>";
        private VariableProcessState processState;
        private Variable currentVariable=new Variable();
        private VariableCompileType variableType;
        protected sealed override void OnInit()
        {
            processState=VariableProcessState.GetType;
            currentVariable.isShared=false;
            currentVariable.mName=string.Empty;
            currentVariable.value=null;
            Process();
        }
        private void Process()
        {
            while(CurrentIndex<TotalCount)
            {
                switch(processState)
                {
                    case VariableProcessState.GetType:
                    {
                        CheckVaildType();
                        break;
                    }
                    case VariableProcessState.IsShared:
                    {
                        CheckIsShared();
                        break;
                    }
                    case VariableProcessState.GetValue:
                    {
                        GetValue();
                        break;
                    }
                    case VariableProcessState.Over:
                    {
                        return;
                    }
                }
            }
        }
        private void CheckVaildType()
        {
            Scanner.MoveNextNoSpace();
            var type=Scanner.TryGetVariableType();
            if(!type.HasValue)
            {
                throw new Exception("<color=#ff2f2f>AkiBTCompiler</color> : Syntax error, variable type not declared");
            }
            variableType=type.Value;
            processState=VariableProcessState.IsShared;
        }
        private void CheckIsShared()
        {
            Scanner.MoveNextNoSpace();
            try{
                Scanner.FindToken(SharedToken);
                currentVariable.isShared=true;
                Scanner.MoveNextNoSpace();
                currentVariable.mName=CurrentToken;
                GetDefaultValue();
                processState=VariableProcessState.Over;
                return;
            }
            catch
            {
                Scanner.MoveBack();
                processState=VariableProcessState.GetValue;
                return;
            }
        }
        private void GetDefaultValue()
        {
            switch(variableType)
            {
                case VariableCompileType.Int:
                {
                    currentVariable.value=0;
                    break;
                }
                case VariableCompileType.Float:
                {
                    currentVariable.value= 0;
                    break;
                }
                case VariableCompileType.Bool:
                {
                    currentVariable.value= false;
                    break;
                }
                case VariableCompileType.String:
                {
                    currentVariable.value= string.Empty;
                    break;
                }
                case VariableCompileType.Vector3:
                {
                    currentVariable.value= new Vector3();
                    break;
                }
            }
        }
        private void GetValue()
        {
            Scanner.MoveNextNoSpace();
            //根据类型转换字符串
            switch(variableType)
            {
                case VariableCompileType.Int:
                {
                    currentVariable.value=Int32.Parse(CurrentToken);
                    break;
                }
                case VariableCompileType.Float:
                {
                    currentVariable.value=float.Parse(CurrentToken);
                    break;
                }
                case VariableCompileType.Bool:
                {
                    currentVariable.value=Boolean.Parse(CurrentToken);
                    break;
                }
                case VariableCompileType.String:
                {
                    currentVariable.value=CurrentToken;
                    break;
                }
                case VariableCompileType.Vector3:
                {
                    currentVariable.value=Scanner.GetVector3();
                    break;
                }
            }
            processState=VariableProcessState.Over;
        }
        internal Variable GetVariable()
        {
            return currentVariable;
        }
    }
}
