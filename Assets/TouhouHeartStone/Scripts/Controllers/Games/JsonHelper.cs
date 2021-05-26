using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using TouhouCardEngine;

namespace Controllers
{
    public static class JsonHelper
    {
        public static string toJson(this object obj)
        {
            ConventionRegistry.Register(nameof(FieldOnlyClassMapConvention), new ConventionPack()
            {
                new FieldOnlyClassMapConvention()
            }, t => true);
            string json = obj.ToJson();
            ConventionRegistry.Remove(nameof(FieldOnlyClassMapConvention));
            return json;
        }
    }
}
