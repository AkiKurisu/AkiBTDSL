namespace Kurisu.AkiBT.DSL
{
    internal class VariableProcessor : Processor
    {
        private readonly Variable currentVariable = new();
        protected sealed override void OnProcess()
        {
            currentVariable.isShared = false;
            currentVariable.mName = string.Empty;
            currentVariable.value = null;
            if (CurrentIndex == TotalCount) return;
            if (IsShared()) return;
            GetValue();
        }
        private bool IsShared()
        {
            bool isShared = GetLastProcessor<PropertyProcessor>().IsShared;
            if (isShared)
            {
                Scanner.MoveNextNoSpace();
                currentVariable.isShared = true;
                currentVariable.mName = CurrentToken;
                return true;
            }
            return false;
        }
        private void GetValue()
        {
            Scanner.MoveNextNoSpace();
            currentVariable.value = Scanner.ParseValue();
        }

        internal Variable GetVariable()
        {
            return currentVariable;
        }
    }
}
