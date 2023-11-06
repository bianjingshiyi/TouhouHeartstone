using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace Game.Replays
{
    public class ReplayDataPlayer
    {
        public ReplayDataPlayer(CardEngine game, ReplayData replay)
        {
            this.game = game;
            this.replay = replay;
        }
        public void update(IRequest[] requests, float deltaTime)
        {
            _currentTime += deltaTime;
            foreach (var request in requests)
            {
                if (request == null)
                    continue;
                var time = replay.responseTimes[currentResponseIndex];
                if (_currentTime < time)
                    continue;
                IResponse nextResponse = replay.responses[currentResponseIndex];
                if (request.isValidResponse(nextResponse))
                {
                    game.answers.answer(nextResponse.playerId, nextResponse);
                    currentResponseIndex++;
                }
            }
        }
        public bool isReplayEnd
        {
            get { return currentResponseIndex >= replay.responses.Count; }
        }
        int currentResponseIndex { get; set; } = 0;
        float _currentTime = 0;
        CardEngine game { get; }
        ReplayData replay { get; }
    }
}
