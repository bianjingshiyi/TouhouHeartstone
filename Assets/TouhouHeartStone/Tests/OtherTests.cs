using NUnit.Framework;
using TouhouHeartstone;
using TouhouCardEngine;
using Controllers;
using System.Collections.Generic;
using MongoDB.Bson.Serialization;

namespace Tests
{
    public class OtherTests
    {
        [Test]
        public void arraySerializeTest()
        {
            List<GeneratedEffect> effects = new List<GeneratedEffect>()
            {
                new GeneratedHalo(1,new string[]{ PILE.MASTER },new string[]{ PILE.HAND },null)
            };
            string json = effects.toJson();
            effects = JsonHelper.fromJson<List<GeneratedEffect>>(json);
            Assert.IsInstanceOf<GeneratedHalo>(effects[0]);
        }
        [Test]
        public void generatedBuffDefineArraySerializeTest()
        {
            GeneratedBuffDefine[] buffDefines = new GeneratedBuffDefine[]
            {
                new GeneratedBuffDefine(1,null, new GeneratedEffect[]
                {
                    new GeneratedHalo(2,new string[]{ PILE.MASTER },new string[]{ PILE.HAND },null)
                })
            };
            //string json = buffDefines.toJson();
            //buffDefines = JsonHelper.fromJson<GeneratedBuffDefine[]>(json);
            //Assert.IsTrue(buffDefines[0].effectList[0] is GeneratedHalo);
            //BsonClassMap.RegisterClassMap<GeneratedHalo>();
            string json = "{ \"_t\" : \"TouhouCardEngine.GeneratedBuffDefine[], TouhouCardEngine\", \"_v\" : [{ \"_id\" : 1, \"propModifierList\" : [], \"effectList\" : [{ \"_t\" : \"GeneratedHalo\", \"pileList\" : [], \"onEnableAction\" : null, \"onDisableAction\" : null, \"triggerList\" : [], \"tagList\" : [], \"propDict\" : { \"haloBuffDefineId\" : 2, \"affectCondition\" : null, \"rangeList\" : { \"_t\" : \"TouhouCardEngine.PileNameCollection, TouhouCardEngine\", \"_v\" : [] } } }] }] }";
            buffDefines = JsonHelper.fromJson<GeneratedBuffDefine[]>(json);
            Assert.IsTrue(buffDefines[0].effectList[0] is GeneratedHalo);
        }
    }
}

