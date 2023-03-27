using System;
using System.Text;
namespace Kurisu.AkiBT.Compiler
{
    internal enum NodeCompileType
    {
        Composite,Conditional,Action,Decorator
    }
    internal enum VariableCompileType
    {
        Int,Float,Bool,String,Vector3
    }
    internal abstract class Processor:IDisposable
    {
        internal const string Composite="Composite";
        internal const string Conditional="Conditional";
        internal const string Action="Action";
        internal const string Decorator="Decorator";
        internal const string Int="Int";
        internal const string Bool="Bool";
        internal const string Float="Float";
        internal const string String="String";
        internal const string Vector3="Vector3";
        protected readonly AkiBTCompiler compiler;
        internal const string LeftParenthesis="(";
        internal const string RightParenthesis=")";
        internal const string LeftBracket="[";
        protected const string RightBracket="]";
        internal const string Comma=",";
        internal const string Colon=":";
        internal const string Line="\n";
        internal const string Return="\r";
        internal const string Tab="\t";
        protected readonly string[] tokens;
        protected int currentIndex;
        internal int CurrentIndex=>currentIndex;
        protected readonly int totalCount;
        internal string CurrentToken=>currentIndex<totalCount?tokens[currentIndex]:null;
        protected Processor(AkiBTCompiler compiler,string[] tokens,int currentIndex)
        {
            this.compiler=compiler;
            this.tokens=tokens;
            this.currentIndex=currentIndex;
            totalCount=tokens.Length;
        }
        /// <summary>
        /// 跳转到下一个非空字符(会跳过当前字符)
        /// </summary>
        internal void NextNoSpace()
        {
            //已到末尾无法继续查找
            if(currentIndex>=totalCount-1)return;
            currentIndex++;
            SkipSpace();
        }
        /// <summary>
        /// 跳过所有空格直到下一个非空字符(如果当前字符不为空则不会跳过)
        /// </summary>
        protected void SkipSpace()
        {
            while(string.IsNullOrWhiteSpace(CurrentToken)||CurrentToken==Return||CurrentToken==Line||CurrentToken==Tab)
            {
                currentIndex++;
                if(currentIndex>=totalCount-1)return;
            }
        }
        /// <summary>
        /// 找到当前或下一个指定Token
        /// </summary>
        /// <param name="token"></param>
        internal void FindToken(string token)
        {
            try
            {
                SkipSpace();
            }
            catch(ArgumentNullException)
            {
                throw new Exception("语法错误,找不到下一个有效字符");
            }
            if(CurrentToken!=token)
            {
                var stringBuilder=new StringBuilder();
                for(int i=0;i<currentIndex;i++)stringBuilder.Append(tokens[i]);
                throw new Exception($"语法错误,未检到字符'{token}',已检测字符'{stringBuilder.ToString()}'");
            }
        }
        protected NodeCompileType? TryGetNodeType()
        {
            switch(CurrentToken)
            {
                case Action:
                {
                    return NodeCompileType.Action;
                }
                case Conditional:
                {
                    return NodeCompileType.Conditional;
                }
                case Composite:
                {
                    return NodeCompileType.Composite;
                }
                case Decorator:
                {
                    return NodeCompileType.Decorator;
                }
            }
            return null;
        }
        protected VariableCompileType? TryGetVariableType()
        {
            switch(CurrentToken)
            {
                case Int:
                {
                    return VariableCompileType.Int;
                }
                case Bool:
                {
                    return VariableCompileType.Bool;
                }
                case Float:
                {
                    return VariableCompileType.Float;
                }
                case String:
                {
                    return VariableCompileType.String;
                }
                case Vector3:
                {
                    return VariableCompileType.Vector3;
                }
            }
            return null;
        }

        public void Dispose()
        {
            OnDispose();
        }
        protected virtual void OnDispose(){}
    }
}
