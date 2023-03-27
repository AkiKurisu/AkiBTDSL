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
        private Variable currentVariable;
        private VariableCompileType variableType;
        internal VariableProcessor(AkiBTCompiler compiler, string[] tokens, int currentIndex) : base(compiler, tokens, currentIndex)
        {
            currentVariable=new Variable();
            Process();
        }
        private void Process()
        {
            while(currentIndex<totalCount)
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
            NextNoSpace();
            var type=TryGetVariableType();
            if(!type.HasValue)
            {
                throw new Exception("语法错误,没有申明变量类型");
            }
            variableType=type.Value;
            processState=VariableProcessState.IsShared;
        }
        private void CheckIsShared()
        {
            NextNoSpace();
            try{
                FindToken(SharedToken);
                currentVariable.isShared=true;
                NextNoSpace();
                currentVariable.mName=CurrentToken;
                GetDefaultValue();
                processState=VariableProcessState.Over;
                return;
            }
            catch
            {
                currentIndex--;
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
            NextNoSpace();
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
                    currentVariable.value=Vector3Helper.GetVector3(this);
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
