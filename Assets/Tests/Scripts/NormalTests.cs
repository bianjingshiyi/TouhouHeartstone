using System.Threading;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class NormalTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void NormalTestsSimplePasses()
        {
            Task.Run(() =>
            {
                Thread.Sleep(3000);
                Debug.Log("Async");
            }).Wait();
        }
    }
}
