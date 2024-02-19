namespace Kurisu.AkiBT.DSL
{
    internal class NodeProcessor : Processor
    {
        private readonly Node currentNode = new();
        private string nodeType = string.Empty;
        public string NodeType => nodeType;
        protected override void OnProcess()
        {
            currentNode.data = null;
            currentNode.type.Clear();
            if (CurrentIndex == TotalCount) return;
            GetNodeType();
            GetNodeData();
        }
        private void GetNodeType()
        {
            Scanner.MoveNextNoSpace();
            nodeType = CurrentToken;
            Scanner.MoveNextNoSpace();
            Scanner.AssertToken(Symbol.LeftParenthesis);
        }
        private void GetNodeData()
        {
            using DataProcessor processor = Process<DataProcessor>();
            currentNode.data = processor.GetData();
            //Data结束后Node一同结束,无需额外检测
        }
        internal Reference GetNode()
        {
            return Compiler.RegisterNode(nodeType, currentNode);
        }
    }
}
