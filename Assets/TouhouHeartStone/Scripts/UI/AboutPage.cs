using UnityEngine;
using IGensoukyo.Utilities;
using System.Text;

namespace UI
{
    public partial class AboutPage
    {
        [SerializeField]
        LicenseFinder license;

        [SerializeField]
        TextAsset staff;

        [SerializeField]
        AiFaDianList aifadian;

        partial void onAwake()
        {
            BackButtonBlack.asButton.onClick.AddListener(() =>
            {
                parent.display(parent.MainMenu);
            });

            LicenseText.text = "";
            foreach (var item in license.Licenses)
            {
                LicenseText.text += $"{item.Title}\n";
                LicenseText.text += $"{item.Content}\n\n";
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("（更新日期：");
            sb.Append(aifadian.UpdateTime.ToString());
            sb.Append("）\n");
            for (int i = 0; i < aifadian.SponsorList.Length; i += 4)
            {
                for (int j = 0; j < 4 && j + i < aifadian.SponsorList.Length; j++)
                {
                    if (j > 0) sb.Append("    ");
                    sb.Append(aifadian.SponsorList[j + i]);
                }
                sb.Append('\n');
            }

            StaffText.text = staff.text.Replace("{{AiFaDian List}}", sb.ToString());
        }
    }
}
