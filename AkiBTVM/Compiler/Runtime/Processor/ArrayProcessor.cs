using System;
using System.Collections.Generic;
using UnityEngine;
namespace Kurisu.AkiBT.Compiler
{
    internal class ArrayProcessor : Processor
    {
        private List<object> childCache;
        internal ArrayProcessor(AkiBTCompiler compiler, string[] tokens, int currentIndex) : base(compiler, tokens, currentIndex)
        {
            childCache=new List<object>();
            Process();
        }
        private enum ArrayProcessState
        {
            GetChild,Over
        }
        private ArrayProcessState processState;
        private void Process()
        {
            while(currentIndex<totalCount)
            {
                switch(processState)
                {
                    case ArrayProcessState.GetChild:
                    {
                        GetChild();
                        break;
                    }
                    case ArrayProcessState.Over:
                    {
                        return;
                    }
                }
            }
        }
        private void GetChild()
        {
            NextNoSpace();
            var type=TryGetNodeType();
            if(type.HasValue)
            {
                currentIndex--;
                var processor=new NodeProcessor(compiler,tokens,currentIndex);
                childCache.Add(processor.GetNode());
                currentIndex=processor.CurrentIndex;
            }
            else
            {
                currentIndex--;
                var processor=new ValueProcessor(compiler,tokens,currentIndex);
                childCache.Add(processor.GetPropertyValue());
                currentIndex=processor.CurrentIndex;
            }
            NextNoSpace();
            if(CurrentToken==RightBracket)
            {
                processState=ArrayProcessState.Over;
                return;
            }
            if(CurrentToken==Comma)
            {
                return;
            }
            throw new Exception("语法错误,找不到下一个有效字符");
        }
        internal object[] GetArray()
        {
            //Debug.Log("输出Child:"+childCache);
            return childCache.ToArray();
        }
    }
}
