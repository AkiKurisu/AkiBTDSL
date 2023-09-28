using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace Kurisu.AkiBT.DSL
{
    public class BehaviorTreeDecompiler
    {
        private readonly StringBuilder stringBuilder = new();
        public string Decompile(IBehaviorTree behaviorTree)
        {
            stringBuilder.Clear();
            WriteVariable(behaviorTree);
            WriteNode(behaviorTree.Root.Child, 0);
            return stringBuilder.ToString();
        }
        private void WriteVariable(IBehaviorTree behaviorTree)
        {
            foreach (var variable in behaviorTree.SharedVariables)
            {
                Write(variable.GetType().Name[6..]);
                Space();
                Write(variable.Name);
                Space();
                var value = variable.GetValue();
#if UNITY_EDITOR
                if (value is UnityEngine.Object UObject)
                {
                    string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(UObject));
                    if (string.IsNullOrEmpty(guid)) Write("Null");
                    else Write(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(UObject)));
                }
                else
#endif
                    Write(value.ToString());
                NewLine();
            }
        }
        private void WriteNode(NodeBehavior node, int indentLevel)
        {
            WriteIndent(indentLevel);
            var type = node.GetType();
            Write(type.Name);
            Write('(');
            int index = 0;
            if (WriteChildren(node, indentLevel))
            {
                index = stringBuilder.Length;
            }
            if (WriteProperties(type, node) && index > 0)
            {
                for (int i = 0; i < indentLevel * 4; i++)
                    stringBuilder.Insert(index, ' ');
                stringBuilder.Insert(index, ",\n");
            }
            Write(')');
        }
        private bool WriteChildren(NodeBehavior node, int indentLevel)
        {
            bool haveChildren = false;
            switch (node)
            {
                case Composite composite:
                    {
                        Write("children:[");
                        NewLine();
                        for (var i = 0; i < composite.Children.Count; i++)
                        {
                            haveChildren = true;
                            WriteNode(composite.Children[i], indentLevel + 1);
                            if (i < composite.Children.Count - 1)
                            {
                                Write(',');
                            }
                            NewLine();
                        }
                        WriteIndent(indentLevel);
                        Write(']');
                        break;
                    }
                case Conditional conditional:
                    {
                        if (conditional.Child == null) return false;
                        haveChildren = true;
                        Write("child:");
                        NewLine();
                        WriteNode(conditional.Child, indentLevel + 1);
                        break;
                    }
                case Decorator decorator:
                    {
                        haveChildren = true;
                        Write("child:");
                        NewLine();
                        WriteNode(decorator.Child, indentLevel + 1);
                        break;
                    }
            }
            return haveChildren;
        }
        private bool WriteProperties(Type type, NodeBehavior node)
        {
            bool haveProperty = false;
            var properties = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                            .Concat(GetAllFields(type))
                            .Where(field => field.IsInitOnly == false
                             && field.GetCustomAttribute<HideInEditorWindow>() == null
                             && !field.FieldType.IsSubclassOf(typeof(UnityEngine.Object))
                             )
                            .ToList();
            for (var i = 0; i < properties.Count; i++)
            {
                var p = properties[i];
                var value = p.GetValue(node);
                if (value == null) continue;
                haveProperty = true;
                WritePropertyName(p);
                if (p.FieldType.IsSubclassOf(typeof(SharedVariable)))
                {
                    WriteVariableValue(value as SharedVariable);
                }
                else
                {
                    WritePropertyValue(value);
                }
                if (i < properties.Count - 1)
                {
                    Write(',');
                }
            }
            return haveProperty;
        }
        private void WritePropertyName(FieldInfo fieldInfo)
        {
            AkiLabelAttribute label;
            if ((label = fieldInfo.GetCustomAttribute<AkiLabelAttribute>()) != null)
            {
                Write(label.Title);
            }
            else
            {
                Write(fieldInfo.Name);
            }
        }
        private void WriteVariableValue(SharedVariable variable)
        {
            if (variable.IsShared)
            {
                Write("=>");
                Write(variable.Name);
            }
            else
            {
                Write(':');
                Write(variable.GetValue().ToString());
            }
        }
        private void WritePropertyValue(object value)
        {
            Write(':');
            Write(value.ToString());
        }
        private static IEnumerable<FieldInfo> GetAllFields(Type t)
        {
            if (t == null)
                return Enumerable.Empty<FieldInfo>();

            return t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(field => field.GetCustomAttribute<SerializeField>() != null &&
                 //Skip Reference Property
                 field.GetCustomAttribute<SerializeReference>() == null)
                .Concat(GetAllFields(t.BaseType));
        }
        private void Space()
        {
            stringBuilder.Append(' ');
        }
        private void WriteIndent(int indentLevel)
        {
            for (int i = 0; i < indentLevel * 4; i++)
                stringBuilder.Append(' ');
        }
        private void NewLine()
        {
            stringBuilder.Append('\n');
        }
        private void Write(string text)
        {
            stringBuilder.Append(text);
        }
        private void Write(char token)
        {
            stringBuilder.Append(token);
        }
    }
}
