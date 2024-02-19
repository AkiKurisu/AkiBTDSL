using System;
namespace Kurisu.AkiBT.DSL
{
    internal enum VariableCompileType
    {
        Int, Float, Bool, String, Vector3, Object
    }
    internal abstract class Processor : IDisposable
    {
        protected Compiler Compiler { get; private set; }
        protected Scanner Scanner { get; private set; }
        protected int CurrentIndex => Scanner.CurrentIndex;
        protected int TotalCount => Scanner.TotalCount;
        protected string CurrentToken => Scanner.CurrentToken;
        protected Processor Parent { get; private set; }
        internal void Process(Compiler compiler, Scanner scanner)
        {
            Compiler = compiler;
            Scanner = scanner;
            Parent = null;
            OnProcess();
        }
        internal void Process(Processor parentProcessor)
        {
            Compiler = parentProcessor.Compiler;
            Scanner = parentProcessor.Scanner;
            Parent = parentProcessor;
            OnProcess();
        }
        protected virtual void OnProcess() { }
        public void Dispose()
        {
            Compiler.PushProcessor(this);
        }
        protected T GetLastProcessor<T>() where T : Processor
        {
            Processor processor = Parent;
            while (processor != null)
            {
                if (processor is T target) return target;
                processor = processor.Parent;
            }
            return null;
        }
        protected T Process<T>() where T : Processor, new()
        {
            return Compiler.Process<T>(this);
        }
    }
}
