using System;
using System.Text;

namespace Kurisu.AkiBT.Compiler
{
    internal class Scanner
    {
        internal const string Int="Int";
        internal const string Bool="Bool";
        internal const string Float="Float";
        internal const string String="String";
        internal const string Vector3="Vector3";
        internal const string LeftParenthesis="(";
        internal const string RightParenthesis=")";
        internal const string LeftBracket="[";
        internal const string RightBracket="]";
        internal const string Comma=",";
        internal const string Colon=":";
        internal const string Line="\n";
        internal const string Return="\r";
        internal const string Tab="\t";
        private string[] tokens;
        private int currentIndex;
        internal int CurrentIndex=>currentIndex;
        internal int TotalCount{get;private set;}
        internal string CurrentToken=>currentIndex<TotalCount?tokens[currentIndex]:null;
        private AkiBTCompiler compiler;
        internal void Init(AkiBTCompiler compiler,string[] tokens)
        {
            currentIndex=-1;
            this.compiler=compiler;
            this.tokens=tokens;
            TotalCount=tokens.Length;
        }
        internal void MoveBack()
        {
            currentIndex--;
        }
        internal void MoveTo(int index)
        {
            currentIndex=index;
        }
        /// <summary>
        /// 跳转到下一个非空字符(会跳过当前字符)
        /// </summary>
        internal void MoveNextNoSpace()
        {
            //已到末尾无法继续查找
            if(currentIndex>=TotalCount-1)return;
            currentIndex++;
            SkipSpace();
        }
        /// <summary>
        /// 跳过所有空格直到下一个非空字符(如果当前字符不为空则不会跳过)
        /// </summary>
        internal void SkipSpace()
        {
            while(string.IsNullOrWhiteSpace(CurrentToken)||CurrentToken==Return||CurrentToken==Line||CurrentToken==Tab)
            {
                currentIndex++;
                if(currentIndex>=TotalCount-1)return;
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
                throw new Exception($"<color=#ff2f2f>AkiBTCompiler</color> : 语法错误,找不到下一个有效字符");
            }
            if(CurrentToken!=token)
            {
                var stringBuilder=new StringBuilder();
                for(int i=0;i<currentIndex;i++)stringBuilder.Append(tokens[i]);
                throw new Exception($"<color=#ff2f2f>AkiBTCompiler</color> : 语法错误,未检到字符'{token}',已检测字符'{stringBuilder.ToString()}'");
            }
        }
        internal bool IsNodeType()
        {
            return compiler.IsNode(CurrentToken);
        }
        internal VariableCompileType? TryGetVariableType()
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

    }
}
