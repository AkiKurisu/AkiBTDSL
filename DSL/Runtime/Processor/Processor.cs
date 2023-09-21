using System;
namespace Kurisu.AkiBT.DSL
{
    internal enum VariableCompileType
    {
        Int, Float, Bool, String, Vector3, Object
    }
    internal abstract class Processor : IDisposable
    {
        protected BehaviorTreeCompiler Compiler { get; private set; }
        protected Scanner Scanner { get; private set; }
        protected int CurrentIndex => Scanner.CurrentIndex;
        protected int TotalCount => Scanner.TotalCount;
        protected string CurrentToken => Scanner.CurrentToken;
        protected Processor Parent { get; private set; }
        internal void Init(BehaviorTreeCompiler compiler, Scanner scanner)
        {
            Compiler = compiler;
            Scanner = scanner;
            Parent = null;
            OnInit();
        }
        internal void Init(Processor parentProcessor)
        {
            Compiler = parentProcessor.Compiler;
            Scanner = parentProcessor.Scanner;
            Parent = parentProcessor;
            OnInit();
        }
        protected virtual void OnInit() { }
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
    }
    internal static class ScannerExtension
    {
        internal static bool TryGetVector3(this Scanner scanner, out Vector3 vector3)
        {
            return Vector3Helper.TryGetVector3(scanner, out vector3);
        }
        internal static Vector3 GetVector3(this Scanner scanner)
        {
            return Vector3Helper.GetVector3(scanner);
        }
        internal static bool TryGetVector2(this Scanner scanner, out Vector2 vector2)
        {
            return Vector2Helper.TryGetVector2(scanner, out vector2);
        }
        internal static Vector2 GetVector2(this Scanner scanner)
        {
            return Vector2Helper.GetVector2(scanner);
        }
    }
}
