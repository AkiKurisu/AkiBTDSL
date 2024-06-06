using System;
using UnityEngine;
namespace Kurisu.AkiBT.DSL
{
    public class Vector3IntContract : ITypeContract
    {
        public bool CanConvert(Type inputType, Type expectType)
        {
            return (inputType == typeof(Vector3Int)) && expectType == typeof(Vector3);
        }

        public object Convert(in object value, Type inputType, Type expectType)
        {
            return (Vector3)(Vector3Int)value;
        }
    }
    public class Vector2IntContract : ITypeContract
    {
        public bool CanConvert(Type inputType, Type expectType)
        {
            return (inputType == typeof(Vector2Int)) && expectType == typeof(Vector2);
        }

        public object Convert(in object value, Type inputType, Type expectType)
        {
            return (Vector2)(Vector2Int)value;
        }
    }
}