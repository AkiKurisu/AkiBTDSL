#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Kurisu.AkiBT.DSL
{
    internal class ReferencedVariableProcessor : Processor
    {
        private struct UObject
        {
            public int instanceID;
        }
        private readonly ReferencedVariable currentVariable = new();
        private VariableCompileType variableType;
        private string type;
        private object Value
        { set => currentVariable.data["value"] = value; }
        private object Name { set => currentVariable.data["mName"] = value; }
        protected sealed override void OnInit()
        {
            type = null;
            currentVariable.type.Clear();
            currentVariable.data.Clear();
            currentVariable.data["isShared"] = true;
            Process();
        }
        private void Process()
        {
            if (CurrentIndex == TotalCount) return;
            GetVariableType();
            GetName();
            GetValue();
            Compiler.RegisterReferencedVariable(type, currentVariable);
        }

        private void GetVariableType()
        {
            Scanner.MoveNextNoSpace();
            var type = Scanner.TryGetVariableType();
            if (!type.HasValue)
            {
                throw new CompileException("Syntax error, variable type not declared");
            }
            variableType = type.Value;
            this.type = CurrentToken;
        }
        private void GetName()
        {
            Scanner.MoveNextNoSpace();
            Name = CurrentToken;
        }

        private void GetValue()
        {
            Scanner.MoveNextNoSpace();
            //根据类型转换字符串
            switch (variableType)
            {
                case VariableCompileType.Int:
                    {
                        Value = int.Parse(CurrentToken);
                        break;
                    }
                case VariableCompileType.Float:
                    {
                        Value = float.Parse(CurrentToken);
                        break;
                    }
                case VariableCompileType.Bool:
                    {
                        Value = bool.Parse(CurrentToken);
                        break;
                    }
                case VariableCompileType.String:
                    {
                        Value = CurrentToken;
                        break;
                    }
                case VariableCompileType.Vector3:
                    {
                        Value = Scanner.GetVector3();
                        break;
                    }
                case VariableCompileType.Object:
                    {
                        Value = new UObject()
                        {
#if UNITY_EDITOR
                            instanceID = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(CurrentToken)).GetInstanceID()
#else
                            instanceID=0
#endif
                        };
                        break;
                    }
                default:
                    {
                        throw new CompileException($"Unrecognized type, current character is '{CurrentToken}'");
                    }
            }
        }
    }
}
