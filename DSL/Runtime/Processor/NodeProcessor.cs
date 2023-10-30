namespace Kurisu.AkiBT.DSL
{
    internal class NodeProcessor : Processor
    {
        private readonly Node currentNode = new();
        private string nodeType = string.Empty;
        public string NodeType => nodeType;
        protected override void OnInit()
        {
            currentNode.data = null;
            currentNode.type.Clear();
            Process();
        }
        private void Process()
        {
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
            using DataProcessor processor = Compiler.GetProcessor<DataProcessor>(this);
            currentNode.data = processor.GetData();
            //Data结束后Node一同结束,无需额外检测
        }
        internal Reference GetNode()
        {
            return Compiler.RegisterNode(nodeType, currentNode);
        }
    }
}
