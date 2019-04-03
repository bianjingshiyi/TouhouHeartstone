using TouhouHeartstone.Frontend.ViewModel;
using UnityEngine;

namespace TouhouHeartstone.Frontend.Controller
{
    /// <summary>
    /// 桌面的公共部分管理器
    /// </summary>
    public class DeckController : MonoBehaviour
    {
        [SerializeField]
        WeatherViewModel weather;

        [SerializeField]
        RoundEndViewModel roundEnd;

        private void Start()
        {
            roundEnd.RoundEndEvent += OnRoundend;
            roundEnd.TimeRemain = 1;
        }

        private void OnRoundend()
        {
            throw new System.NotImplementedException();
        }
    }
}
