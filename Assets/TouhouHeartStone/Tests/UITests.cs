using NUnit.Framework;
using TouhouHeartstone;
using UI;
namespace Tests
{
    public class UITests
    {
        [Test]
        public void getAnimTest()
        {
            Table table = new Table();
            table.initAnim();
            var anim = table.getAnim(new THHGame.InitEventArg());
            Assert.NotNull(anim);
        }
    }
}