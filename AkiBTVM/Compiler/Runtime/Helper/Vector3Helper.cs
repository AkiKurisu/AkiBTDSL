using System;
namespace Kurisu.AkiBT.Compiler
{
    /// <summary>
    /// To support UnityEngine.Vector2 && Vector3, we need to make a similar template for reason that we can't serialize them suitably using JsonConvert
    /// </summary>
    [System.Serializable]
    internal struct Vector3
    {
        public float x;
        public float y;
        public float z;
        internal Vector3(float x,float y,float z)
        {
            this.x=x;
            this.y=y;
            this.z=z;
        }
    }
    internal class Vector3Helper
    {
        internal static bool TryGetVector3(Scanner scanner,out Vector3 vector3)
        {
            try
            {
               vector3=GetVector3(scanner);
               return true;
            }
            catch
            {
                vector3=new Vector3();
                return false;
            }
           
        }
        internal static Vector3 GetVector3(Scanner scanner)
        {
            float x,y,z;
            if(scanner.CurrentToken!=Scanner.LeftParenthesis)
            {
                throw new Exception("语法错误,Vector3缺少'('");
            }
            try
            {
                scanner.MoveNextNoSpace();
                x=float.Parse(scanner.CurrentToken);
                scanner.MoveNextNoSpace();
                scanner.FindToken(Scanner.Comma);
                scanner.MoveNextNoSpace();
                y=float.Parse(scanner.CurrentToken);
                scanner.MoveNextNoSpace();
                scanner.FindToken(Scanner.Comma);
                scanner.MoveNextNoSpace();
                z=float.Parse(scanner.CurrentToken);
            }
            catch
            {
                throw new Exception("语法错误,无法识别出Vector3");
            }
            scanner.MoveNextNoSpace();
            if(scanner.CurrentToken!=Scanner.RightParenthesis)
            {
                throw new Exception("语法错误,Vector3缺少')'");
            }
            return new Vector3(x,y,z);
        }
    }
}
