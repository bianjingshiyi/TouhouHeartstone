using System.Collections;
using NUnit.Framework;

using TouhouCardEngine;
using TouhouHeartstone;

namespace Tests
{
    public class TestCardsTest
    {
        [Test]
        public void normalServantTest()
        {
            //绝望了，凭什么我要为CardID头疼呀？自动分配自动分配！
            Game game = new Game(new UnitTestGameEnv(), false);

            int kd = game.registerCardDefine(new TestSkillDefine(2));
            int cd = game.registerCardDefine(new TestMasterDefine(kd));
            int sd = game.registerCardDefine(new TestServantDefine(0, 1, 1));
            TestFrontend[] frontends = new TestFrontend[2];
            frontends[0] = new TestFrontend();
            frontends[1] = new TestFrontend();
            game.addPlayer(frontends[0], new int[] { cd, sd, sd, sd, sd, sd, sd, sd, sd, sd, sd });
            game.addPlayer(frontends[1], new int[] { cd, sd, sd, sd, sd, sd, sd, sd, sd, sd, sd });
            game.init();
            game.initReplace(0, new int[0]);
            game.initReplace(1, new int[0]);

        }
        class TestMasterDefine : MasterCardDefine
        {
            public override int skillID { get; } = 0;
            public override int category { get; } = 0;
            public override int id { get; set; }
            public override Effect[] effects { get; } = new Effect[0];
            public TestMasterDefine(int skillID, int category = 0, params Effect[] effects)
            {
                this.skillID = skillID;
                this.category = category;
                this.effects = effects;
            }
        }
        class TestSkillDefine : SkillCardDefine
        {
            public override int cost { get; } = 0;
            public override int id { get; set; } = 0;
            public override Effect[] effects { get; } = new Effect[0];
            public TestSkillDefine(int cost, params Effect[] effects)
            {
                this.cost = cost;
                this.effects = effects;
            }
        }
        class TestServantDefine : ServantCardDefine
        {
            public override int cost { get; } = 0;
            public override int attack { get; } = 1;
            public override int life { get; } = 1;
            public override int category { get; } = 0;
            public override int id { get; set; } = 0;
            public override Effect[] effects { get; } = new Effect[0];
            public TestServantDefine(int cost, int attack, int life, int category = 0, params Effect[] effects)
            {
                this.cost = cost;
                this.attack = attack;
                this.life = life;
                this.category = category;
                this.effects = effects;
            }
        }
    }
}
