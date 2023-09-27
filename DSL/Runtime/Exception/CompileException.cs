using System;
namespace Kurisu.AkiBT.DSL
{
    public class CompileException : Exception
    {
        public CompileException(string message) : base(message) { }
    }
}
