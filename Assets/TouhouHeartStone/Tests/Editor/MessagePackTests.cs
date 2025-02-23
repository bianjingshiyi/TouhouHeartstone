using Controllers;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using NUnit.Framework;
using System;
using TouhouCardEngine;
using UnityEngine;

namespace Tests
{
    public class MessagePackTests
    {
        [Test]
        public void MessagePackRenameTest()
        {
            // 构造一个重命名前的对象
            BeforeRenameClass before = new BeforeRenameClass() { id = 1, name = "test" };
            // 使用默认方法序列化
            byte[] bytes = MessagePackSerializer.Serialize(before);
            // 使用重命名解码器反序列化
            CustomFormatterResolver resolver = new CustomFormatterResolver(new()
            {
                { typeof(BeforeRenameClass), new RenameFormatter() }
            });
            AfterRenameClass after = MessagePackSerializer.Deserialize<AfterRenameClass>(new ReadOnlyMemory<byte>(bytes), MessagePackSerializer.DefaultOptions.WithResolver(CompositeResolver.Create(
                StandardResolver.Instance,
                resolver)));
            // 预期得到重命名后的对象
            Assert.AreEqual(1, after.id);
            Assert.AreEqual("test", after.name);
        }
        [MessagePackObject]
        public class BeforeRenameClass
        {
            [Key(0)]
            public int id;
            [Key(1)]
            public string name;
        }
        [MessagePackObject]
        public class AfterRenameClass
        {
            [Key(0)]
            public int id;
            [Key(1)]
            public string name;
        }
        public class RenameFormatter : IMessagePackFormatter<AfterRenameClass>
        {
            public void Serialize(ref MessagePackWriter writer, AfterRenameClass value, MessagePackSerializerOptions options)
            {
                if (value == null)
                {
                    writer.WriteNil();
                }
                else
                {
                    writer.Write(value.id);
                    writer.Write(value.name);
                }
            }
            public AfterRenameClass Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
            {
                if (reader.TryReadNil())
                {
                    return null;
                }
                int id = reader.ReadInt32();
                string name = reader.ReadString();
                return new AfterRenameClass() { id = id, name = name };
            }
        }
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