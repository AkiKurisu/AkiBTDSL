using System;
using System.Text;
namespace Kurisu.AkiBT.DSL
{
    public class Scanner
    {
        private string[] tokens;
        private int currentIndex;
        public int CurrentIndex => currentIndex;
        public int TotalCount { get; private set; }
        public string CurrentToken => currentIndex < TotalCount ? tokens[currentIndex] : null;
        public string Peek
        {
            get
            {
                if (currentIndex + 1 >= tokens.Length) return null;
                return tokens[currentIndex + 1];
            }
        }
        public void Init(string[] tokens)
        {
            currentIndex = -1;
            this.tokens = tokens;
            TotalCount = tokens.Length;
        }
        public void MoveBack()
        {
            currentIndex--;
        }
        public void MoveNext()
        {
            currentIndex++;
        }
        public void MoveTo(int index)
        {
            currentIndex = index;
        }
        /// <summary>
        /// 跳转到下一个非空字符(会跳过当前字符)
        /// </summary>
        public void MoveNextNoSpace()
        {
            //已到末尾无法继续查找
            if (currentIndex >= TotalCount - 1) return;
            currentIndex++;
            SkipSpace();
        }
        /// <summary>
        /// 跳过所有空格直到下一个非空字符(如果当前字符不为空则不会跳过)
        /// </summary>
        public void SkipSpace()
        {
            while (string.IsNullOrWhiteSpace(CurrentToken) || CurrentToken is Symbol.Return or Symbol.Line or Symbol.Tab)
            {
                currentIndex++;
                if (currentIndex >= TotalCount - 1) return;
            }
        }
        /// <summary>
        /// 判断当前或下一个Token是否为指定Token
        /// </summary>
        /// <param name="token"></param>
        public void AssertToken(string assertToken)
        {
            try
            {
                SkipSpace();
            }
            catch (ArgumentNullException)
            {
                throw new CompileException("Syntax error, next valid character not found");
            }
            if (CurrentToken != assertToken)
            {
                throw new CompileException($"Syntax error, assert symbol not found '{assertToken}'");
            }
        }
        public int IndexOfNext(string token)
        {
            for (int i = CurrentIndex + 1; i < TotalCount; i++)
            {
                if (tokens[i] == token) return i;
            }
            throw new CompileException($"Syntax error, search symbol not found '{token}'");
        }
        public string GetHistory()
        {
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < currentIndex; i++) stringBuilder.Append(tokens[i]);
            return stringBuilder.ToString();
        }
        internal VariableCompileType? TryGetVariableType()
        {
            return TryGetVariableType(CurrentToken);
        }
        internal static VariableCompileType? TryGetVariableType(string token)
        {
            switch (token)
            {
                case Symbol.Int:
                    {
                        return VariableCompileType.Int;
                    }
                case Symbol.Bool:
                    {
                        return VariableCompileType.Bool;
                    }
                case Symbol.Float:
                    {
                        return VariableCompileType.Float;
                    }
                case Symbol.String:
                    {
                        return VariableCompileType.String;
                    }
                case Symbol.Vector3:
                    {
                        return VariableCompileType.Vector3;
                    }
                case Symbol.Object:
                    {
                        return VariableCompileType.Object;
                    }
            }
            return null;
        }

        internal object ParseValue()
        {
            //检测是否为数字
            if (int.TryParse(CurrentToken, out int intNum))
            {
                return intNum;
            }
            if (float.TryParse(CurrentToken, out float floatNum))
            {
                return floatNum;
            }
            if (bool.TryParse(CurrentToken, out bool boolValue))
            {
                return boolValue;
            }
            int index = CurrentIndex;
            if (this.TryGetVector3(out Vector3 vector3))
            {
                return vector3;
            }
            MoveTo(index);
            if (this.TryGetVector2(out Vector2 vector2))
            {
                return vector2;
            }
            MoveTo(index);
            return CurrentToken;
        }

    }
}
