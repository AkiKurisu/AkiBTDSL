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
        private ReferencedVariable currentVariable=new ReferencedVariable();
        private VariableCompileType variableType;
        private string type;
        private object Value{set=>currentVariable.data["value"]=value;}
        private object Name{set=>currentVariable.data["mName"]=value;}
        protected sealed override void OnInit()
        {
            processState=VariableProcessState.GetType;
            type=null;
            currentVariable.type=null;
            currentVariable.data.Clear();
            currentVariable.data["isShared"]=true;
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
                        Compiler.RegisterReferencedVariable(type,currentVariable);
                        return;
                    }
                }
            }
        }

        private void GetVariableType()
        {
            Scanner.MoveNextNoSpace();
            var type=Scanner.TryGetVariableType();
            if(!type.HasValue)
            {
                throw new Exception("<color=#ff2f2f>AkiBTCompiler</color> : Syntax error, variable type not declared");
            }
            variableType=type.Value;
            this.type=CurrentToken;
            processState=VariableProcessState.GetName;
        }  
        private void GetName()
        {
            Scanner.MoveNextNoSpace();
            Name=CurrentToken;
            processState=VariableProcessState.GetValue;
        }

        private void GetValue()
        {
            Scanner.MoveNextNoSpace();
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
                    Value=Scanner.GetVector3();
                    break;
                }
                default:
                {
                    throw new Exception($"<color=#ff2f2f>AkiBTCompiler</color> : Unrecognized type, current character is '{CurrentToken}'");
                }
            }
            processState=VariableProcessState.Over;
        }
    }
}
