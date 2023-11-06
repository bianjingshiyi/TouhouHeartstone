using System.Linq;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;

namespace Game.Replays
{
    public class ReplayDataPlayer
    {
        public ReplayDataPlayer(CardEngine game, ReplayData replay)
        {
            this.game = game;
            this.replay = replay;
        }
        public IResponse getNextResponse()
        {
            return replay.responses[currentResponseIndex];
        }
        public void Next()
        {
            currentResponseIndex++;
        }
        public bool isReplayEnd
        {
            get { return currentResponseIndex >= replay.responses.Count; }
        }
        int currentResponseIndex { get; set; } = 0;
        CardEngine game { get; }
        ReplayData replay { get; }
    }
}
