using CustomBuilder;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NitoriNetwork.Common;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace IGensoukyo.Utilities
{
    public class AiFaDianAPI
    {
        string UserID { get; }
        string APIKey { get; }
        string BaseURL => "https://afdian.net/api";
        string OrderURL => BaseURL + "/open/query-order";
        string SponsorURL => BaseURL + "/open/query-sponsor";

        RestClient client { get; } = new RestClient();

        public AiFaDianAPI(string userID, string apiKey)
        {
            UserID = userID;
            APIKey = apiKey;

            client.ThrowOnDeserializationError = true;
            client.UseSerializer(
                () => new MongoDBJsonSerializer()
            );
        }

        public void GetOrder(int page = 0, int numPerPage = 100, int[] tradeNo = null)
        {

        }

        GenericResp<ItemList<SponsorItem>> getSponsors(int page = 0, int numPerPage = 100, int[] userID = null)
        {
            BsonDocument doc = new BsonDocument();
            if (page > 0) doc.Add("page", page);
            if (numPerPage > 0) doc.Add("per_page", numPerPage);
            if (userID != null) doc.Add("user_id", string.Join(",", userID.Select(i => i.ToString())));

            var result = queryAifadian<ItemList<SponsorItem>>(SponsorURL, doc.ToJson());
            if (!result.IsSuccessful)
                throw new NetClientException(result.ErrorMessage);

            return result.Data;
        }

        public SponsorItem[] GetSponsors()
        {
            int numPerPage = 100;
            var resp = getSponsors(0, numPerPage);
            List<SponsorItem> sponsorList = new List<SponsorItem>();
            sponsorList.AddRange(resp.Result.List);

            for (int i = 1; i < resp.Result.TotalPage; i++)
            {
                var obj = getSponsors(i + 1, numPerPage);
                sponsorList.AddRange(obj.Result.List);
            }

            return sponsorList.ToArray();
        }

        IRestResponse<GenericResp<T>> queryAifadian<T>(string url, string param)
        {
            IRestRequest request = new RestRequest(url, Method.POST);
            request.AddParameter("user_id", UserID);
            request.AddParameter("params", param);

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            request.AddParameter("ts", timestamp);

            var strToCalc = APIKey + "params" + param + "ts" + timestamp + "user_id" + UserID;
            var hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(strToCalc));
            var hashStr = string.Concat(hash.Select(b => b.ToString("x2")).ToArray());
            request.AddParameter("sign", hashStr);

            return client.Post<GenericResp<T>>(request);
        }

        [BsonIgnoreExtraElements]
        [Serializable]
        class GenericResp<T>
        {
            [BsonElement("ec")]
            public int ErrorCode { get; set; }

            [BsonElement("em")]
            public string Message { get; set; }

            [BsonElement("data")]
            public T Result { get; set; }
        }

        [BsonIgnoreExtraElements]
        [Serializable]
        class ItemList<T>
        {
            [BsonElement("total_count")]
            public int TotalCount { get; set; }

            [BsonElement("total_page")]
            public int TotalPage { get; set; }

            [BsonElement("list")]
            public T[] List { get; set; }
        }

        [BsonIgnoreExtraElements]
        [Serializable]
        public class SponsorItem
        {
            [BsonElement("sponsor_plans")]
            public Plan[] Plans { get; set; }

            [BsonElement("current_plan")]
            public Plan CurrentPlan { get; set; }

            [BsonElement("all_sum_amount")]
            public string AllSumAmount { get; set; }

            [BsonElement("first_pay_time")]
            public int FirstPayTime { get; set; }

            [BsonElement("last_pay_time")]
            public int LastPayTime { get; set; }

            [BsonElement("user")]
            public User User { get; set; }

        }

        [BsonIgnoreExtraElements]
        [Serializable]
        public class Plan
        {

        }

        [BsonIgnoreExtraElements]
        [Serializable]
        public class User
        {
            [BsonElement("user_id")]
            public string UserID { get; set; }

            [BsonElement("name")]
            public string Name { get; set; }

            [BsonElement("avatar")]
            public string Avatar { get; set; }
        }
    }

    [CreateAssetMenu(fileName = "AiFaDian.asset", menuName = "Custom/爱发电")]
    public class AiFaDianList : ScriptableObject
    {
        [SerializeField]
        string[] sponsorList = new string[0];

        public string[] SponsorList => sponsorList;

        public DateTime UpdateTime => DateTimeOffset.FromUnixTimeSeconds(updateTime).LocalDateTime;

        [SerializeField]
        long updateTime = 0;

        public void UpdateInfo(string userID, string apiKey)
        {
            var api = new AiFaDianAPI(userID, apiKey);
            var sponsors = api.GetSponsors();

            List<(string, int, int)> sponsorAmounts = new List<(string, int, int)>();
            foreach (var item in sponsors)
            {
                float amount = float.Parse(item.AllSumAmount);
                sponsorAmounts.Add((item.User.Name, (int)(amount * 100), item.FirstPayTime));
            }

            sponsorAmounts.Sort(((string, int, int) a, (string, int, int) b) =>
            {
                if (a.Item2 < b.Item2) return 1;
                if (a.Item2 == b.Item2)
                    return a.Item3.CompareTo(b.Item3);
                return -1;
            });

            sponsorList = sponsorAmounts.Select(x => x.Item1).ToArray();
            updateTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        }
    }


#if UNITY_EDITOR
    [BuilderStep(5)]
    public class AiFaDianBuildStep : IBuilderStep
    {
        void IBuilderStep.ActionAfter()
        {
        }

        void IBuilderStep.ActionBefore(object opt)
        {
            var userID = Environment.GetEnvironmentVariable("AIFADIAN_USR");
            var apiKey = Environment.GetEnvironmentVariable("AIFADIAN_PSW");
            if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(apiKey))
            {
                Debug.Log("爱发电APIKey不存在，不更新赞助名单");
                return;
            }

            var so = AssetDatabase.LoadAssetAtPath<AiFaDianList>("Assets/Resources/AiFaDian.asset");
            if (so == null)
            {
                Debug.Log("找不到爱发电赞助名单，跳过更新");
                return;
            }
            try
            {
                so.UpdateInfo(userID, apiKey);
                EditorUtility.SetDirty(so);
                AssetDatabase.SaveAssets();
                Debug.Log("爱发电赞助名单已更新");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }

        }
    }
#endif
}


