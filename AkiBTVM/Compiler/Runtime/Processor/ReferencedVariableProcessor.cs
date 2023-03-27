using System;
namespace Kurisu.AkiBT.Compiler
{
    internal class ReferencedVariableProcessor : Processor
    {
        private enum VariableProcessState
        {
            GetType,GetName,GetValue,Over
        }
        private VariableProcessState processState;
        private ReferencedVariable currentVariable;
        private VariableCompileType variableType;
        private string type;
        private object Value{set=>currentVariable.data["value"]=value;}
        private object Name{set=>currentVariable.data["mName"]=value;}
        public ReferencedVariableProcessor(AkiBTCompiler compiler, string[] tokens, int currentIndex) : base(compiler, tokens, currentIndex)
        {
            currentVariable=new ReferencedVariable();
            currentVariable.data["isShared"]=true;
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
                        GetVariableType();
                        break;
                    }
                    case VariableProcessState.GetName:
                    {
                        GetName();
                        break;
                    }
                    case VariableProcessState.GetValue:
                    {
                        GetValue();
                        break;
                    }
                    case VariableProcessState.Over:
                    {
                        compiler.RegisterReferencedVariable(type,currentVariable);
                        return;
                    }
                }
            }
        }

        private void GetVariableType()
        {
            NextNoSpace();
            var type=TryGetVariableType();
            if(!type.HasValue)
            {
                throw new Exception("语法错误,没有申明变量类型");
            }
            variableType=type.Value;
            this.type=CurrentToken;
            processState=VariableProcessState.GetName;
        }  
        private void GetName()
        {
            NextNoSpace();
            Name=CurrentToken;
            processState=VariableProcessState.GetValue;
        }

        private void GetValue()
        {
            NextNoSpace();
            //根据类型转换字符串
            switch(variableType)
            {
                case VariableCompileType.Int:
                {
                    Value=Int32.Parse(CurrentToken);
                    break;
                }
                case VariableCompileType.Float:
                {
                    Value=float.Parse(CurrentToken);
                    break;
                }
                case VariableCompileType.Bool:
                {
                    Value=Boolean.Parse(CurrentToken);
                    break;
                }
                case VariableCompileType.String:
                {
                    Value=CurrentToken;
                    break;
                }
                case VariableCompileType.Vector3:
                {
                    Value=Vector3Helper.GetVector3(this);
                    break;
                }
                default:
                {
                    throw new Exception($"无法识别类型,当前字符{CurrentToken}");
                }
            }
            processState=VariableProcessState.Over;
        }
    }
}
