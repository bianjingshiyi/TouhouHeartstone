using UnityEngine;
namespace UI
{
    partial class RoomPanel
    {
        partial void onAwake()
        {
            RandomSeedToggle.onValueChanged.AddListener(value =>
            {
                RandomSeedInputField.interactable = value;
                RandomSeedInputField.image.color = value ? Color.white : Color.gray;
            });
            RandomSeedToggle.isOn = false;
            RandomSeedInputField.text = null;
            RandomSeedInputField.interactable = false;
            RandomSeedInputField.image.color = Color.gray;
        }
    }
}