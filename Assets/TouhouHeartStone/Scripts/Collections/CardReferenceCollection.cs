using System;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;

namespace TouhouCardEngine
{
    [Serializable]
    [MessagePackFormatter(typeof(CardReferenceCollectionFormatter))]
    public class CardReferenceCollection : List<DefineReference>, ICloneable
    {
        public CardReferenceCollection() : base()
        {
        }
        public CardReferenceCollection(IEnumerable<DefineReference> defRefs) : base(defRefs)
        {
        }
        public object Clone()
        {
            return new CardReferenceCollection(this);
        }
    }

    public sealed class CardReferenceCollectionFormatter : IMessagePackFormatter<CardReferenceCollection>
    {
        public void Serialize(ref MessagePackWriter writer, CardReferenceCollection value, MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
            }
            else
            {
                IMessagePackFormatter<DefineReference> formatter = options.Resolver.GetFormatterWithVerify<DefineReference>();

                var c = value.Count;
                writer.WriteArrayHeader(c);

                for (int i = 0; i < c; i++)
                {
                    writer.CancellationToken.ThrowIfCancellationRequested();
                    formatter.Serialize(ref writer, value[i], options);
                }
            }
        }

        public CardReferenceCollection Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
                return default;

            IMessagePackFormatter<DefineReference> formatter = options.Resolver.GetFormatterWithVerify<DefineReference>();

            var len = reader.ReadArrayHeader();
            var list = new CardReferenceCollection();
            options.Security.DepthStep(ref reader);
            try
            {
                for (int i = 0; i < len; i++)
                {
                    reader.CancellationToken.ThrowIfCancellationRequested();
                    list.Add(formatter.Deserialize(ref reader, options));
                }
            }
            finally
            {
                reader.Depth--;
            }

            return list;
        }
    }
}