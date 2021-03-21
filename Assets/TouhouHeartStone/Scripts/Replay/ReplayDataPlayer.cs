using TouhouHeartstone;
using System.Linq;
using TouhouCardEngine.Interfaces;
using System;

namespace Game.Replays
{
    public class ReplayDataPlayer
    {
        public ReplayDataPlayer(THHGame game, ReplayData replay)
        {
            this.game = game;
            this.replay = replay;
        }
        public void playNextMove(IRequest request)
        {
            for (int i = 0; i < request.playersId.Length; i++)
            {
                IResponse response = replay.responses[currentResponseIndex];
                currentResponseIndex++;
                while (!request.isValidResponse(response) && !isReplayEnd)
                {
                    response = replay.responses[currentResponseIndex];
                    currentResponseIndex++;
                }
                if (request.isValidResponse(response))
                {
                    game.answers.answer(response.playerId, response);
                    if (request.isAny)
                        return;
                }
            }
        }
        public bool isReplayEnd
        {
            get { return currentResponseIndex >= replay.responses.Count; }
        }
        int currentResponseIndex { get; set; } = 0;
        THHGame game { get; }
        ReplayData replay { get; }
    }
}
