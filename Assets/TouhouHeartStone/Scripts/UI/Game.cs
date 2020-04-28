namespace UI
{
    partial class Game
    {
        partial void onAwake()
        {
            QuitButton.onClick.AddListener(() =>
            {
                parent.game.game.Dispose();
                parent.display(parent.MainMenu);
            });
        }
    }
}
