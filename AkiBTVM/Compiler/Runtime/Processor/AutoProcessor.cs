using System.Collections.Generic;
namespace Kurisu.AkiBT.Compiler
{
    internal class AutoProcessor : Processor
    {
        private bool rootProcessed;
        internal AutoProcessor(AkiBTCompiler compiler, string[] tokens, int currentIndex) : base(compiler, tokens, currentIndex)
        {
            rootProcessed=false;
            Process();
        }
        private void Process()
        {
            while(currentIndex<totalCount-1)
            {
                NextNoSpace();
                if(currentIndex>=totalCount-1)return;
                if(TryGetVariableType().HasValue)
                {
                    currentIndex--;
                    var processor=new ReferencedVariableProcessor(compiler,tokens,currentIndex);
                    currentIndex=processor.CurrentIndex;
                    continue;
                }
                if(TryGetNodeType().HasValue)
                {
                    currentIndex--;
                    var nodeProcessor=new NodeProcessor(compiler,tokens,currentIndex);
                    var reference=nodeProcessor.GetNode();
                    if(!rootProcessed)
                    {
                        rootProcessed=true;
                        ProcessRoot(reference);
                    }
                    currentIndex=nodeProcessor.CurrentIndex;
                    continue;
                }
                if(currentIndex>=totalCount-1)return;
                    throw new System.Exception($"无效字符'{CurrentToken}'");
            }
        }
        private void ProcessRoot(Reference reference)
        {
            var node=new Node();
            node.data=new Dictionary<string, object>();
            node.data["child"]=reference;
            compiler.RegisterRoot(node);
        }
    }
}
