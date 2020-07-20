using UnityEngine;
using IGensoukyo.Utilities;

namespace UI
{
    public partial class AboutPage
    {
        [SerializeField]
        LicenseFinder license;

        [SerializeField]
        TextAsset staff;

        partial void onAwake()
        {
            BackButtonBlack.asButton.onClick.AddListener(() =>
            {
                parent.display(parent.MainMenu);
            });

            LicenseText.text = "";
            foreach (var item in license.Licenses)
            {
                LicenseText.text += item.Title + "\n";
                LicenseText.text += item.Content + "\n\n";
            }

            StaffText.text = staff.text;
        }
    }
}
