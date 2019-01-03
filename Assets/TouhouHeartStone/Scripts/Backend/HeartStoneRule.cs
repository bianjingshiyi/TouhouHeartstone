namespace TouhouHeartstone.Backend
{
    /// <summary>
    /// 这个炉石规则是测试用的。
    /// </summary>
    class HeartStoneRule : Rule
    {
        public override void beforeEvent(Game game, Event e)
        {
        }
        public override void afterEvent(Game game, Event e)
        {
            if (e.name == "Init")
                onInit(game);
        }
        private void onInit(Game game)
        {
            //确定先攻顺序
            Player[] players = game.getPlayers();
            firstPlayer = players[game.randomInt(0, players.Length - 1)];
            //初始化卡组并抽牌
            for (int i = 0; i < players.Length; i++)
            {
                initPlayer(players[i]);
                //获取玩家的卡组
                int[] deck = players[i].getDeck();
                //创建这些卡并洗牌
                players[i]["Library"].createCard(game, players[i], deck, 0, Visibility.none);
                players[i]["Library"].shuffle(game);
                //抽三张牌，如果不是第一个玩家则多抽一张牌。
                if (players[i] == firstPlayer)
                    draw(game, players[i], 3);
                else
                    draw(game, players[i], 4);
            }
            //游戏开始
            start(game);
        }
        private void start(Game game)
        {
            Event @event = new Event("Start");
            game.doEvent(@event, (g, e) =>
            {

            });
        }
        private void draw(Game game, Player[] players)
        {
            Event @event = new Event("Draw");
            @event["players"] = players;
            game.doEvent(@event, (g, e) =>
            {
                players = e.getVar<Player[]>("players");
                if (players == null || players.Length == 0)
                    return;
                foreach (Player player in players)
                    player["Library"].moveCardTo(g, player["Library"][player["Library"].count - 1], player["Hand"], player["Hand"].count, Visibility.self);
            });
        }
        private void draw(Game game, Player[] players, int count)
        {
            for (int i = 0; i < count; i++)
                draw(game, players);
        }
        private void initPlayer(Player player)
        {
            player.createPile("Library");
            player.createPile("Hand");
            player.createPile("Field");
            player.createPile("Grave");
        }
        Player firstPlayer { get; set; } = null;
    }
}