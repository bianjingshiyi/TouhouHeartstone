using NUnit.Framework;
using TouhouHeartstone;
using UI;
using UnityEngine;
namespace Tests
{
    public class UITests
    {
        [Test]
        public void getAnimTest()
        {
            Table table = new Table();
            table.initAnim();
            Assert.NotNull(table.getAnim(new THHGame.InitEventArg()));
            UI.Animation fatigueAnim = table.getAnim(new THHPlayer.FatigueEventArg());
            Assert.NotNull(fatigueAnim);
            Debug.Log(fatigueAnim);
        }
    }
}