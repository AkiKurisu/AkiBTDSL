using System;
using System.Collections.Generic;
namespace Kurisu.AkiBT.DSL
{
    /// <summary>
    /// An json-like template to imitate serialization of SerializeReferenceAttribute does
    /// Can deserialize to Root and SharedVariables, which are the main part of a behaviorTree
    /// If the API is changed,you may need to cover the key name such as 'version' and 'RefIds'
    /// </summary>
    [Serializable]
    public class SerializeReferenceData
    {
        [Serializable]
        public class References
        {
            public readonly int version = 2;
            public readonly object[] RefIds;
            public References(List<object> referencesCache)
            {
                RefIds = referencesCache.ToArray();
            }
        }
        public readonly Reference root;
        public readonly Reference[] variables;
        public readonly References references;
        internal SerializeReferenceData(Reference root, List<object> referencesCache, List<Reference> variableReferences)
        {
            this.root = root;
            references = new References(referencesCache);
            variables = variableReferences.ToArray();
        }
    }
}
