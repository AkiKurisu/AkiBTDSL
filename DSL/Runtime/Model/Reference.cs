using System;
using System.Collections.Generic;
namespace Kurisu.AkiBT.DSL
{
    internal interface IReference
    {
        public int Rid { set; }
    }
    [Serializable]
    public readonly struct Reference
    {
        public readonly int rid;
        public Reference(int rid)
        {
            this.rid = rid;
        }
    }
    [Serializable]
    internal class Node : IReference
    {
        /// <summary>
        /// ReferenceID will be created by compiler
        /// </summary>
        public int rid;
        public int Rid { set => rid = value; }
        // <summary>
        // Type will be created by compiler
        // </summary>
        public Dictionary<string, string> type = new();
        public Dictionary<string, object> data;
    }
    [Serializable]
    internal class ReferencedVariable : IReference
    {
        /// <summary>
        /// ReferenceID will be created by compiler
        /// </summary>
        public int rid;
        public int Rid { set => rid = value; }
        // <summary>
        // Type will be created by compiler
        // </summary>
        public Dictionary<string, string> type = new();
        public Dictionary<string, object> data = new();
    }
    [Serializable]
    internal class Variable
    {
        public bool isShared;
        public string mName = string.Empty;
        public object value;
    }
}
