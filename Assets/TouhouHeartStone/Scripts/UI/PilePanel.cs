using BJSYGameCore.UI;
namespace UI
{
    partial class PilePanel
    {
        partial void onAwake()
        {
            ButtonBlack.asButton.onClick.set(() =>
            {
                hide();
            });
        }
        public void display(string title)
        {
            display();
            TitleText.text = title;
        }
    }
}
