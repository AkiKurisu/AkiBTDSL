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
        internal static bool TryGetVector3(Processor processor,out Vector3 vector3)
        {
            try
            {
               vector3=GetVector3(processor);
               return true;
            }
            catch
            {
                vector3=new Vector3();
                return false;
            }
           
        }
        internal static Vector3 GetVector3(Processor processor)
        {
            float x,y,z;
            if(processor.CurrentToken!=Processor.LeftParenthesis)
            {
                throw new Exception("语法错误,Vector3缺少'('");
            }
            try
            {
                processor.NextNoSpace();
                x=float.Parse(processor.CurrentToken);
                processor.NextNoSpace();
                processor.FindToken(Processor.Comma);
                processor.NextNoSpace();
                y=float.Parse(processor.CurrentToken);
                processor.NextNoSpace();
                processor.FindToken(Processor.Comma);
                processor.NextNoSpace();
                z=float.Parse(processor.CurrentToken);
            }
            catch
            {
                throw new Exception("语法错误,无法识别出Vector3");
            }
            processor.NextNoSpace();
            if(processor.CurrentToken!=Processor.RightParenthesis)
            {
                throw new Exception("语法错误,Vector3缺少')'");
            }
            return new Vector3(x,y,z);
        }
    }
}
