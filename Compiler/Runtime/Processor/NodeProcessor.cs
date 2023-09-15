namespace Kurisu.AkiBT.Compiler
{
    internal class NodeProcessor : Processor
    {
        private readonly Node currentNode = new();
        private enum NodeProcessState
        {
            GetType, GetData, Over
        }
        private NodeProcessState processState;
        private string nodeType = string.Empty;
        public string NodeType => nodeType;
        protected override void OnInit()
        {
            currentNode.data = null;
            currentNode.type.Clear();
            processState = NodeProcessState.GetType;
            Process();
        }
        private void Process()
        {
            while (CurrentIndex < TotalCount)
            {
                switch (processState)
                {
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
        private void GetNodeType()
        {
            Scanner.MoveNextNoSpace();
            nodeType = CurrentToken;
            CheckValidStart();
        }
        private void CheckValidStart()
        {
            Scanner.MoveNextNoSpace();
            Scanner.FindToken(Scanner.LeftParenthesis);
            processState = NodeProcessState.GetData;
        }
        private void GetNodeData()
        {
            using (DataProcessor processor = Compiler.GetProcessor<DataProcessor>(this))
            {
                currentNode.data = processor.GetData();
            }
            //Data结束后Node一同结束,无需额外检测
            processState = NodeProcessState.Over;
        }
        internal Reference GetNode()
        {
            return Compiler.RegisterNode(nodeType, currentNode);
        }
    }
}
