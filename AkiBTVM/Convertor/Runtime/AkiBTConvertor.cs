using UnityEngine;
using Kurisu.AkiBT.Compiler;
#if UNITY_EDITOR
using Newtonsoft.Json.Linq;
using System.Linq;
using Newtonsoft.Json;
#endif
namespace Kurisu.AkiBT.Convertor
{
    /// <summary>
    /// The Convertor to convert AkiBTCode to AkiBTIL, then to BehaviorTreeTemplate for behaviorTree
    /// You can push your own typeDictionary to the constructor for custom naming, node limit, etc
    /// </summary>
    public class AkiBTConvertor
    {
        public AkiBTConvertor()
        {
            compiler=new AkiBTCompiler();
        }
        public AkiBTConvertor(string path)
        {
            compiler=new AkiBTCompiler(path);
        }
        private AkiBTCompiler compiler;
        public BehaviorTreeTemplate Convert(string code)
        {
            return TranslateIL(compiler.Compile(code));
        }
        public static BehaviorTreeTemplate TranslateIL(string IL)
        {
            return JsonUtility.FromJson<BehaviorTreeTemplate>(IL);
        }
        public static string TreeToIL(BehaviorTreeSO behaviorTreeSO,bool indented=false)
        {
            return TemplateToIL(TreeToTemplate(behaviorTreeSO),indented);
        }
        public static BehaviorTreeTemplate TreeToTemplate(BehaviorTreeSO behaviorTreeSO)
        {
            var template=new BehaviorTreeTemplate(behaviorTreeSO);
            return template;
        }
        public static string TemplateToIL(BehaviorTreeTemplate template,bool indented=false)
        {
            var json=JsonUtility.ToJson(template);
            #if UNITY_EDITOR
                //Remove editor fields in behaviorTree manually
                JObject obj = JObject.Parse(json);
                foreach (JProperty prop in obj.Descendants().OfType<JProperty>().ToList())
                {
                    if (prop.Name == "graphPosition"||prop.Name == "description"||prop.Name == "guid")
                    {
                        prop.Remove();
                    }
                }
                return obj.ToString(indented?Formatting.Indented:Formatting.None);
            #else
                //Dont need remove in build as they won't be serialized
                return json;
            #endif
        }
    }
}