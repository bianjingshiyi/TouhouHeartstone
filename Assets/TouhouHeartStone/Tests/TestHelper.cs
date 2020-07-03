using System.Threading.Tasks;
using UnityEngine;
namespace Tests
{
    static class TestHelper
    {
        public static WaitUntil waitTask(Task task)
        {
            return new WaitUntil(() => task.IsCompleted || task.IsFaulted || task.IsCanceled);
        }
    }
}