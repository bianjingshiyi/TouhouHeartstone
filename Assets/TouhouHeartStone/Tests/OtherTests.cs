using NUnit.Framework;
using TouhouHeartstone;
using TouhouCardEngine;
using Controllers;
using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using System.Text.RegularExpressions;
using UnityEngine;
using BJSYGameCore;
using System;
using System.Security.Cryptography;
using System.IO;

namespace Tests
{
    public class OtherTests
    {
        [Test]
        public void getTextureHashTest()
        {
            Texture2D texture = Resources.Load<Texture2D>("TestPicture");
            Debug.Log(texture.imageContentsHash.ToString());
            string textureHash = calcHashString(texture.EncodeToJPG());
            Debug.Log(textureHash);
            string fileHash = calcHashString(texture.EncodeToJPG());
            Debug.Log(fileHash);
            Assert.AreEqual(textureHash, fileHash);
            string calcHashString(byte[] bytes)
            {
                byte[] hash = SHA256.Create().ComputeHash(bytes);
                string hashString = string.Join(string.Empty, Array.ConvertAll(hash, b => b.ToString("x2")));
                return hashString;
            }
        }
        [Test]
        public void regexWildcardTest()
        {
            Assert.True(Regex.IsMatch("ABC.txt", GetWildcardRegexString("*.txt")));
        }
        [Test]
        public void resoucesLoadAllTest()
        {
            var res = Resources.LoadAll<StreamingAssetsInfo>("");
            Assert.NotNull(res);
        }
        [Test]
        public void getArrayTypeInfoTest()
        {
            Type type = typeof(int[]);
            Debug.Log("IsArray:" + type.IsArray);
            Debug.Log("ElementType:" + type.GetElementType().FullName);
        }
        public static string GetWildcardRegexString(string wildcardStr)
        {
            return Regex.Replace(wildcardStr, "[.$^{\\[(|)*+?\\\\]", m =>
            {
                switch (m.Value)
                {
                    case "?":
                        return ".?";
                    case "*":
                        return ".*";
                    default:
                        return "\\" + m.Value;
                }
            }) + "$";
        }
    }
}

