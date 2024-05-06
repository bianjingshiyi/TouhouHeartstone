using MessagePack;
using NUnit.Framework;
using TouhouCardEngine;
using UnityEngine;

namespace Tests
{
    public class MessagePackTests
    {
        [Test]
        public void defineReferenceTest()
        {
            var objects = new object[] { 111, "aaa", new DefineReference(1, 114514) };
            var proxy = ObjectProxy.TryProxy(objects);
            var bin = MessagePackSerializer.Serialize(proxy);
            var converted = MessagePackSerializer.Deserialize<object>(bin);
            var convertedBack = ObjectProxy.TryConvertBack(converted);
            var defineRef = (convertedBack as object[])[2] as DefineReference;
            Assert.NotNull(defineRef);
            Assert.AreEqual(114514, defineRef.defineId);
        }
    }
}