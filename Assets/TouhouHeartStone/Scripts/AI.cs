using System.Threading.Tasks;
using TouhouHeartstone;
using TouhouCardEngine.Interfaces;
namespace Game
{
    class AI
    {
        THHGame game { get; }
        THHPlayer player { get; }
        public AI(THHGame game, THHPlayer player)
        {
            this.game = game;
            this.player = player;
            game.answers.onRequest += Answers_onRequest;
        }
        private async void Answers_onRequest(IRequest request)
        {
            if (game.answers == null)
                return;
            await Task.Delay(1000);
            switch (request)
            {
                case InitReplaceRequest _:
                    _ = game.answers.answer(player.id, new InitReplaceResponse()
                    {
                        cardsId = new int[0]
                    });
                    break;
                case FreeActRequest _:
                    _ = game.answers.answer(player.id, new TurnEndResponse()
                    {
                    });
                    break;
                default:
                    UberDebug.LogChannel("AI", "AI未处理的询问：" + request);
                    break;
            }
        }
    }
}
