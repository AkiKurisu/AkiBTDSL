using System;
namespace Kurisu.AkiBT.Compiler
{
    /// <summary>
    /// To support UnityEngine.Vector2 && Vector3, we need to make a similar template for reason that we can't serialize them suitably using JsonConvert
    /// </summary>
    [System.Serializable]
    internal struct Vector2
    {
        public float x;
        public float y;
        internal Vector2(float x,float y)
        {
            this.x=x;
            this.y=y;
        }
    }
    internal class Vector2Helper
    {
        internal static bool TryGetVector2(Scanner scanner,out Vector2 vector2)
        {
            try
            {
               vector2=GetVector2(scanner);
               return true;
            }
            catch
            {
                vector2=new Vector2();
                return false;
            }
        }
        internal static Vector2 GetVector2(Scanner scanner)
        {
            float x,y;
            if(scanner.CurrentToken!=Scanner.LeftParenthesis)
            {
                throw new Exception("语法错误,Vector2缺少'('");
            }
            try
            {
                scanner.MoveNextNoSpace();
                x=float.Parse(scanner.CurrentToken);
                scanner.MoveNextNoSpace();
                scanner.FindToken(Scanner.Comma);
                scanner.MoveNextNoSpace();
                y=float.Parse(scanner.CurrentToken);
            }
            catch
            {
                throw new Exception("语法错误,无法识别出Vector2");
            }
            scanner.MoveNextNoSpace();
            if(scanner.CurrentToken!=Scanner.RightParenthesis)
            {
                throw new Exception("语法错误,Vector2缺少')'");
            }
            return new Vector2(x,y);
        }
    }
}
