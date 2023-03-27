using System;
namespace Kurisu.AkiBT.Compiler
{
    internal class NodeProcessor:Processor
    {
        private Node currentNode;
        private enum NodeProcessState
        {
            Check,GetType,GetData,Over
        }
        private NodeProcessState processState;
        public string nodeType=string.Empty;
        internal NodeProcessor(AkiBTCompiler compiler,string[] tokens,int currentIndex):base(compiler,tokens,currentIndex)
        {
            currentNode=new Node();
            processState=NodeProcessState.Check;   
            Process();
        }
        private void Process()
        {
            while(currentIndex<totalCount)
            {
                switch(processState)
                {
                    case NodeProcessState.Check:
                    {
                        CheckVaildInit();
                        break;
                    }
                    case NodeProcessState.GetType:
                    {
                        GetNodeType();
                        break;
                    }
                    case NodeProcessState.GetData:
                    {
                        GetNodeData();
                        break;
                    }
                    case NodeProcessState.Over:
                    {
                        return;
                    }
                }
            }
        }
        private void CheckVaildInit()
        {
            NextNoSpace();
            var type=TryGetNodeType();
            if(!type.HasValue)
            {
                throw new Exception("语法错误,没有申明结点类型");
            }
            processState=NodeProcessState.GetType;
        }
        private void GetNodeType()
        {
            NextNoSpace();
            nodeType=CurrentToken;
            CheckValidStart();
        }
        private void CheckValidStart()
        {
            NextNoSpace();
            FindToken(LeftParenthesis);
            processState=NodeProcessState.GetData;
        }
        private void GetNodeData()
        {
            using (DataProcessor processor=new DataProcessor(compiler,tokens,currentIndex))
            {
                currentNode.data=processor.GetData();
                currentIndex=processor.CurrentIndex;
            }
            //Data结束后Node一同结束,无需额外检测
            processState=NodeProcessState.Over;
        }
        internal Reference GetNode()
        {
            return compiler.RegisterNode(nodeType,currentNode);
        }
    }
}
