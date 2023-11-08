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
        public void update(IAnswerManager anwserManager)
        {
            if (isReplayEnd)
                return;
            var requests = anwserManager.getAllRequests();
            foreach (var request in requests)
            {
                if (isReplayEnd)
                    return;
                if (request == null)
                    continue;
                IResponse nextResponse = replay.responses[currentResponseIndex];
                if (!anwserManager.isCorrectResponse(request, nextResponse))
                    continue;
                game.answers.answer(nextResponse.playerId, nextResponse);
                currentResponseIndex++;
            }
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
