namespace Kurisu.AkiBT.Compiler
{
    internal class VariableProcessor : Processor
    {
        private enum VariableProcessState
        {
            IsShared, GetValue, Over
        }
        private VariableProcessState processState;
        private readonly Variable currentVariable = new();
        protected sealed override void OnInit()
        {
            processState = VariableProcessState.IsShared;
            currentVariable.isShared = false;
            currentVariable.mName = string.Empty;
            currentVariable.value = null;
            Process();
        }
        private void Process()
        {
            while (CurrentIndex < TotalCount)
            {
                switch (processState)
                {
                    case VariableProcessState.IsShared:
                        {
                            CheckIsShared();
                            break;
                        }
                    case VariableProcessState.GetValue:
                        {
                            GetValue();
                            break;
                        }
                    case VariableProcessState.Over:
                        {
                            return;
                        }
                }
            }
        }
        private void CheckIsShared()
        {
            bool isShared = GetLastProcessor<PropertyProcessor>().IsShared;
            if (isShared)
            {
                Scanner.MoveNextNoSpace();
                currentVariable.isShared = true;
                currentVariable.mName = CurrentToken;
                processState = VariableProcessState.Over;
                return;
            }
            processState = VariableProcessState.GetValue;
        }
        private void GetValue()
        {
            Scanner.MoveNextNoSpace();
            currentVariable.value = Scanner.ParseValue();
            processState = VariableProcessState.Over;
        }

        internal Variable GetVariable()
        {
            return currentVariable;
        }
    }
}
