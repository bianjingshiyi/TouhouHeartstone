using MukioI18n;

namespace TouhouHeartstone.Builtin
{
    public class TAG
    {
        public const string FAIRY = "Fairy";
        public const string MACHINE = "Machine";
        public const string DEMON = "Demon";
        public const string ELEMENT = "Element";
        public const string BEAST = "Beast";
        public const string BUG = "Bug";
        public const string GOD = "God";
        public const string GHOST = "Ghost";
    }

    public static class TranslateTags
    {
        [TranslateMsg]
        public const string Fairy = "妖精";
        [TranslateMsg]
        public const string Machine = "机械";
        [TranslateMsg]
        public const string Demon = "恶魔";
        [TranslateMsg]
        public const string Element = "元素";
        [TranslateMsg]
        public const string Beast = "野兽";
        [TranslateMsg]
        public const string Bug = "昆虫";
        [TranslateMsg]
        public const string God = "神明";
        [TranslateMsg]
        public const string Ghost = "幽灵";
    }
}