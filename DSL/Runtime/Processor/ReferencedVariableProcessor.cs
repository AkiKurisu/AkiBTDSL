#if UNITY_EDITOR
using System.Text.RegularExpressions;
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
        private const string AQNPattern = @"^[A-Za-z0-9\.]+, [A-Za-z0-9\.]+, Version=\d+\.\d+\.\d+\.\d+, Culture=[a-zA-Z]+, PublicKeyToken=[a-zA-Z0-9]+";
        private string type;
        private object Value
        {
            set => currentVariable.data["value"] = value;
        }
        private object Name
        {
            set => currentVariable.data["mName"] = value;
        }
        protected sealed override void OnProcess()
        {
            type = null;
            currentVariable.type.Clear();
            currentVariable.data.Clear();
            currentVariable.data["isShared"] = true;
            currentVariable.data["isGlobal"] = false;
            if (CurrentIndex == TotalCount) return;
            GetVariableType();
            GetName();
            GetValue();
            Compiler.RegisterReferencedVariable(type, currentVariable);
        }
        private void GetVariableType()
        {
            Scanner.MoveNextNoSpace();
            if (!Scanner.TryGetVariableType(out var compileType, out bool isGlobal))
            {
                throw new CompileException("Syntax error, variable type not declared");
            }
            variableType = compileType;
            if (isGlobal) SetGlobal();
            type = compileType.ToString();
        }
        private void GetName()
        {
            Scanner.MoveNextNoSpace();
            Name = CurrentToken;
        }
        private void SetGlobal()
        {
            currentVariable.data["isGlobal"] = true;
            currentVariable.data["isExposed"] = true;
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
                        if (Regex.IsMatch(CurrentToken, AQNPattern))
                        {
                            currentVariable.data["constraintTypeAQN"] = CurrentToken;
                            Scanner.MoveNextNoSpace();
                        }
#if UNITY_EDITOR
                        var path = AssetDatabase.GUIDToAssetPath(CurrentToken);
                        if (!string.IsNullOrEmpty(path))
                        {
                            Value = new UObject()
                            {
                                instanceID = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path).GetInstanceID()
                            };
                        }
                        else
#endif
                        {
                            Value = new UObject()
                            {
                                instanceID = 0
                            };
                        }
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
