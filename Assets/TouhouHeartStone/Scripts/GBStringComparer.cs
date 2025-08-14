using System;
using System.Text;

namespace Game
{
    public class GBStringComparer : StringComparer
    {
        public override int Compare(string x, string y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            int len = Math.Min(x.Length, y.Length);
            char[] buffer = new char[1];
            for (int i = 0; i < len; i++)
            {
                char cx = x[i];
                char cy = y[i];

                if (cx == cy) continue;

                if (IsInGBK(cx) && IsInGBK(cy))
                {
                    // 中文字符：使用国标码排序
                    buffer[0] = cx;
                    byte[] xb = gbEncoding.GetBytes(buffer);
                    buffer[0] = cy;
                    byte[] yb = gbEncoding.GetBytes(buffer);

                    int cmp = CompareBytes(xb, yb);
                    if (cmp != 0) return cmp;
                }
                else
                {
                    // 其他字符：使用 Unicode 顺序
                    int cmp = cx.CompareTo(cy);
                    if (cmp != 0) return cmp;
                }
            }

            return x.Length.CompareTo(y.Length);
        }

        public override bool Equals(string x, string y) => Compare(x, y) == 0;

        public override int GetHashCode(string obj)
        {
            if (obj == null) return 0;
            var bytes = gbEncoding.GetBytes(obj);
            int hash = 17;
            foreach (var b in bytes)
                hash = hash * 31 + b;
            return hash;
        }

        /// <summary>
        /// 判断字符是否属于 GB18030 双字节（GBK）编码范围
        /// 这些范围是根据 GBK 映射表反查得出的，不含四字节补充区
        /// </summary>
        public static bool IsInGBK(char c)
        {
            // ASCII 范围
            if (c <= 0x007F) return true;

            // CJK 基本汉字区
            if (c >= 0x4E00 && c <= 0x9FA5) return true;

            // CJK 扩展 A（GB18030 双字节部分覆盖部分汉字）
            if (c >= 0x3400 && c <= 0x4DB5) return true;

            // 常用符号、标点
            if (c >= 0x2010 && c <= 0x2027) return true;
            if (c >= 0x3000 && c <= 0x303F) return true; // 全角标点
            if (c >= 0xFF01 && c <= 0xFF5E) return true; // 全角英数

            // 部分希腊字母、俄文字母（GBK 收录）
            if (c >= 0x0391 && c <= 0x0451) return true;

            // 常用拼音符号
            if (c >= 0x0100 && c <= 0x017F) return true;
            if (c >= 0x02C7 && c <= 0x02C7) return true; // ˇ
            if (c >= 0x02D8 && c <= 0x02D9) return true; // ˘ ˙
            if (c >= 0x2014 && c <= 0x2014) return true; // —
            if (c >= 0x2018 && c <= 0x2019) return true; // ‘ ’
            if (c >= 0x201C && c <= 0x201D) return true; // “ ”
            if (c >= 0x2026 && c <= 0x2026) return true; // …
            if (c >= 0x2215 && c <= 0x2215) return true; // ∕
            if (c >= 0x2225 && c <= 0x2225) return true; // ∥

            return false;
        }

        private static int CompareBytes(byte[] a, byte[] b)
        {
            int len = Math.Min(a.Length, b.Length);
            for (int i = 0; i < len; i++)
            {
                int diff = a[i].CompareTo(b[i]);
                if (diff != 0) return diff;
            }
            return a.Length.CompareTo(b.Length);
        }

        public static readonly GBStringComparer Default = new GBStringComparer();
        private readonly Encoding gbEncoding = Encoding.GetEncoding("GB18030");
    }
}
