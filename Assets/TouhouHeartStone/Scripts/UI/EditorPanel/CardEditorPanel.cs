using BJSYGameCore.UI;
using SFB;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using Game;
using UIHelper = Game.UIHelper;
using System.Threading.Tasks;

namespace UI
{
    public class CardEditorPanel : MonoBehaviour
    {
        public void setId(int id)
        {
            _idText.text = id.ToString();
        }
        /// <summary>
        /// 设置皮肤切换选项
        /// </summary>
        /// <param name="skinNames"></param>
        public void setSkinNames(string[] skinNames)
        {
            _skinDropdown.ClearOptions();
            List<string> optionList = new List<string>(skinNames);
            optionList.Add("新建皮肤");
            _skinDropdown.AddOptions(optionList);
        }
        /// <summary>
        /// 设置当前皮肤索引
        /// </summary>
        /// <param name="skinIndex"></param>
        public void setSkin(int skinIndex)
        {
            _disableUIEvent = true;
            _skinDropdown.value = skinIndex;
            _disableUIEvent = false;
        }
        public void setLanguages(string[] languages)
        {
            _languageDropdown.ClearOptions();
            _languageDropdown.AddOptions(new List<string>(languages));
        }
        /// <summary>
        /// 设置当前语言索引
        /// </summary>
        /// <param name="languageIndex"></param>
        public void setLanguage(int languageIndex)
        {
            _disableUIEvent = true;
            _languageDropdown.value = languageIndex;
            _disableUIEvent = false;
        }
        /// <summary>
        /// 设置可选的卡牌类型
        /// </summary>
        /// <param name="types"></param>
        public void setTypes(string[] types)
        {
            _typeDropdown.ClearOptions();
            _typeDropdown.AddOptions(new List<string>(types));
        }
        /// <summary>
        /// 设置卡牌的卡框类型索引
        /// </summary>
        /// <param name="typeIndex"></param>
        public void setType(int typeIndex)
        {
            _disableUIEvent = true;
            _typeDropdown.value = typeIndex;
            _card.setTypeByName(_typeDropdown.options[typeIndex].text);
            _disableUIEvent = false;
        }
        /// <summary>
        /// 设置费用
        /// </summary>
        /// <param name="cost"></param>
        public void setCost(string cost)
        {
            _card.setCost(cost);
        }
        /// <summary>
        /// 设置卡牌名称
        /// </summary>
        /// <param name="name"></param>
        public void setName(string name)
        {
            _card.setName(name);
        }
        /// <summary>
        /// 设置卡牌副名
        /// </summary>
        /// <param name="subName"></param>
        public void setSubName(string subName)
        {
            _card.setSubname(subName);
        }
        /// <summary>
        /// 设置卡牌的图片。
        /// </summary>
        /// <param name="sprite">卡牌图片</param>
        public void setImage(Sprite sprite)
        {
            _card.setImage(sprite);
        }
        /// <summary>
        /// 设置卡牌的描述
        /// </summary>
        /// <param name="text"></param>
        public void setDesc(string text)
        {
            _card.setDesc(text);
        }
        /// <summary>
        /// 设置卡牌的攻击力
        /// </summary>
        /// <param name="attack"></param>
        public void setAttack(string attack)
        {
            _card.setAttack(attack);
        }
        /// <summary>
        /// 设置卡牌的生命值
        /// </summary>
        /// <param name="life"></param>
        public void setLife(string life)
        {
            _card.setLife(life);
        }
        /// <summary>
        /// 设置卡牌可选的标签
        /// </summary>
        /// <param name="tags"></param>
        public void setTags(string[] tags)
        {
            _tagDropdown.ClearOptions();
            _tagDropdown.AddOptions(new List<string>(tags));
        }
        /// <summary>
        /// 设置卡牌的标签
        /// </summary>
        /// <param name="tag"></param>
        public void setTag(int index)
        {
            _disableUIEvent = true;
            _card.setTag(_tagDropdown.options[index].text);
            _tagDropdown.value = index;
            _disableUIEvent = false;
        }
        /// <summary>
        /// 设置效果列表
        /// </summary>
        /// <param name="effectNames"></param>
        public void setEffects(string[] effectNames)
        {
            BJSYGameCore.UI.UIHelper.updateList(_effectListRoot, _effectListItemTemplate, _effectList, effectNames.Length, (i, item) =>
            {
                CardEditorPanelEffectListItem effectListItem = item.GetComponent<CardEditorPanelEffectListItem>();
                effectListItem.update(effectNames[i]);
            }, itemRect =>
            {
                CardEditorPanelEffectListItem item = itemRect.GetComponent<CardEditorPanelEffectListItem>();
                item.onEditButtonClick += onClickEffectListItemEditButton;
                item.onRemoveButtonClick += onClickEffectListItemRemoveButton;
            }, itemRect =>
            {
                CardEditorPanelEffectListItem item = itemRect.GetComponent<CardEditorPanelEffectListItem>();
                item.onEditButtonClick -= onClickEffectListItemEditButton;
                item.onRemoveButtonClick -= onClickEffectListItemRemoveButton;
            });
        }
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            _typeDropdown.onValueChanged.AddListener(onTypeDropdownValueChanged);
            _skinDropdown.gameObject.SetActive(true);
            _skinDropdown.onValueChanged.AddListener(onSkinDropdownValueChanged);
            _changeSkinNameButton.gameObject.SetActive(true);
            _changeSkinNameButton.onClick.AddListener(onChangeSkinNameButtonClicked);
            _deleteSkinButton.gameObject.SetActive(true);
            _deleteSkinButton.onClick.AddListener(onDeleteSkinButtonClicked);
            _skinNameInputField.gameObject.SetActive(false);
            _skinNameInputField.onEndEdit.AddListener(onSkinNameInputFieldEndEdit);
            _languageDropdown.onValueChanged.AddListener(onLanguageDropdownValueChanged);
            _editNameButton.onClick.AddListener(onEditNameButtonClicked);
            _editSubnameButton.onClick.AddListener(onEditSubnameButtonClicked);
            _editDescButton.onClick.AddListener(onEditDescButtonClicked);
            _editImageButton.onClick.AddListener(onEditImageButtonClicked);
            _editTagButton.onClick.AddListener(onEditTagButtonClicked);
            _editCostButton.onClick.AddListener(onEditCostButtonClicked);
            _editAttackButton.onClick.AddListener(onEditAttackButtonClicked);
            _editLifeButton.onClick.AddListener(onEditLifeButtonClicked);
            _nameInputField.onEndEdit.AddListener(onNameInputFieldEndEdit);
            _subnameInputField.onEndEdit.AddListener(onSubNameInputFieldEndEdit);
            _descInputField.onEndEdit.AddListener(onDescInputFieldEndEdit);
            _tagDropdown.onValueChanged.AddListener(onTagDropdownValueChanged);
            EventTrigger trigger = _tagDropdown.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.Deselect;
            entry.callback.AddListener(e =>
            {
                if (_tagDropdown.template.gameObject.activeSelf)
                {
                    return;
                }
                onTagDropdownValueChanged(_tagDropdown.value);
            });
            trigger.triggers.Add(entry);
            _costInputField.onEndEdit.AddListener(onCostInputFieldEndEdit);
            _attackInputField.onEndEdit.AddListener(onAttackInputFieldEndEdit);
            _lifeInputField.onEndEdit.AddListener(onLifeInputFieldEndEdit);
            _addEffectButton.onClick.AddListener(onAddEffectButtonClicked);
            if (_effectListItemTemplate.transform.parent == _effectListRoot)
            {
                _effectListItemTemplate.gameObject.SetActive(false);
            }
            _backgroundButton.onClick.AddListener(onBackgroundButtonClicked);
        }
        #endregion
        #region 事件回调
        private void onTypeDropdownValueChanged(int value)
        {
            if (_disableUIEvent)
            {
                return;
            }
            onChangeType?.Invoke(value);
        }
        private void onSkinDropdownValueChanged(int value)
        {
            if (_disableUIEvent)
            {
                return;
            }
            onChangeSkin?.Invoke(value);
        }
        private void onChangeSkinNameButtonClicked()
        {
            _skinDropdown.gameObject.SetActive(false);
            _changeSkinNameButton.gameObject.SetActive(false);
            _deleteSkinButton.gameObject.SetActive(false);
            _skinNameInputField.gameObject.SetActive(true);
            _disableUIEvent = true;
            _skinNameInputField.text = null;
            _disableUIEvent = false;
            _skinNameInputField.Select();
        }
        private void onSkinNameInputFieldEndEdit(string text)
        {
            _disableUIEvent = true;
            _skinDropdown.gameObject.SetActive(true);
            _changeSkinNameButton.gameObject.SetActive(true);
            _deleteSkinButton.gameObject.SetActive(true);
            _skinNameInputField.gameObject.SetActive(false);
            onChangeSkinName?.Invoke(text);
            _disableUIEvent = false;
        }
        private void onDeleteSkinButtonClicked()
        {
            if (_disableUIEvent)
            {
                return;
            }
            onClickDeleteSkinButton?.Invoke();
        }
        private void onLanguageDropdownValueChanged(int value)
        {
            if (_disableUIEvent)
            {
                return;
            }
            onChangeLanguage?.Invoke(value);
        }
        private void onEditNameButtonClicked()
        {
            _editNameButton.gameObject.SetActive(false);
            _nameInputField.gameObject.SetActive(true);
            _disableUIEvent = true;
            _nameInputField.text = null;
            _disableUIEvent = false;
            _nameInputField.Select();
        }
        private void onEditSubnameButtonClicked()
        {
            _editSubnameButton.gameObject.SetActive(false);
            _subnameInputField.gameObject.SetActive(true);
            _disableUIEvent = true;
            _subnameInputField.text = null;
            _disableUIEvent = false;
            _subnameInputField.Select();
        }
        private void onEditDescButtonClicked()
        {
            _editDescButton.gameObject.SetActive(false);
            _descInputField.gameObject.SetActive(true);
            _disableUIEvent = true;
            _descInputField.text = null;
            _disableUIEvent = false;
            _descInputField.Select();
        }
        private async void onEditImageButtonClicked()
        {
            _editImageButton.gameObject.SetActive(false);
            Task<Sprite> task = onNeedSelectImage?.Invoke();
            Sprite sprite = task != null ? await task : null;
            if (sprite != null)
            {
                _card.setImage(sprite);
            }
            _editImageButton.gameObject.SetActive(true);
        }
        private void onEditTagButtonClicked()
        {
            _editTagButton.gameObject.SetActive(false);
            _tagDropdown.gameObject.SetActive(true);
            _tagDropdown.Show();
        }
        private void onEditCostButtonClicked()
        {
            _editCostButton.gameObject.SetActive(false);
            _costInputField.gameObject.SetActive(true);
        }
        private void onEditAttackButtonClicked()
        {
            _editAttackButton.gameObject.SetActive(false);
            _attackInputField.gameObject.SetActive(true);
        }
        private void onEditLifeButtonClicked()
        {
            _editLifeButton.gameObject.SetActive(false);
            _lifeInputField.gameObject.SetActive(true);
        }
        private void onNameInputFieldEndEdit(string text)
        {
            _nameInputField.gameObject.SetActive(false);
            _editNameButton.gameObject.SetActive(true);
            onChangeName?.Invoke(text);
        }
        private void onSubNameInputFieldEndEdit(string text)
        {
            _subnameInputField.gameObject.SetActive(false);
            _editSubnameButton.gameObject.SetActive(true);
            onChangeSubName?.Invoke(text);
        }
        private void onDescInputFieldEndEdit(string text)
        {
            _descInputField.gameObject.SetActive(false);
            _editDescButton.gameObject.SetActive(true);
            onChangeDesc?.Invoke(text);
        }
        private void onTagDropdownValueChanged(int value)
        {
            if (_disableUIEvent)
            {
                return;
            }
            _tagDropdown.gameObject.SetActive(false);
            _editTagButton.gameObject.SetActive(true);
            onChangeTag?.Invoke(value);
        }
        private void onCostInputFieldEndEdit(string text)
        {
            _costInputField.gameObject.SetActive(false);
            _editCostButton.gameObject.SetActive(true);
            onChangeCost?.Invoke(text);
        }
        private void onAttackInputFieldEndEdit(string text)
        {
            _attackInputField.gameObject.SetActive(false);
            _editAttackButton.gameObject.SetActive(true);
            onChangeAttack?.Invoke(text);
        }
        private void onLifeInputFieldEndEdit(string text)
        {
            _lifeInputField.gameObject.SetActive(false);
            _editLifeButton.gameObject.SetActive(true);
            onChangeLife?.Invoke(text);
        }
        private void onClickEffectListItemEditButton(CardEditorPanelEffectListItem item)
        {
            onClickEditEffectButton?.Invoke(_effectList.IndexOf(item.transform as RectTransform));
        }
        private void onClickEffectListItemRemoveButton(CardEditorPanelEffectListItem item)
        {
            onClickRemoveEffectButton?.Invoke(_effectList.IndexOf(item.transform as RectTransform));
        }
        private void onAddEffectButtonClicked()
        {
            onClickAddEffectButton?.Invoke();
        }
        private void onBackgroundButtonClicked()
        {
            onClickBackgroundButton?.Invoke();
        }
        #endregion
        #endregion
        /// <summary>
        /// 当更换了皮肤，参数为更换的皮肤索引。
        /// </summary>
        public event Action<int> onChangeSkin;
        /// <summary>
        /// 当更改当前皮肤名字，参数为更改名称。
        /// </summary>
        public event Action<string> onChangeSkinName;
        /// <summary>
        /// 当点击删除当前皮肤按钮
        /// </summary>
        public event Action onClickDeleteSkinButton;
        /// <summary>
        /// 当更换了语言，参数为更换的语言索引。
        /// </summary>
        public event Action<int> onChangeLanguage;
        /// <summary>
        /// 当更换卡牌类型，参数为更改类型索引。
        /// </summary>
        public event Action<int> onChangeType;
        /// <summary>
        /// 当卡牌费用值被更改，参数为用户输入费用。
        /// </summary>
        public event Action<string> onChangeCost;
        /// <summary>
        /// 当卡牌名称被更改，参数为更改后的名称。
        /// </summary>
        public event Action<string> onChangeName;
        /// <summary>
        /// 当卡牌副名被更改，参数为更改后的卡牌副名。
        /// </summary>
        public event Action<string> onChangeSubName;
        /// <summary>
        /// 当需要选择卡牌图片，需要返回一个被选择图片（可以为空）
        /// </summary>
        public event Func<Task<Sprite>> onNeedSelectImage;
        /// <summary>
        /// 当更改卡牌描述，参数为更改后的卡牌描述。
        /// </summary>
        public event Action<string> onChangeDesc;
        /// <summary>
        /// 当更改卡牌攻击力，参数为更改输入的攻击力。
        /// </summary>
        public event Action<string> onChangeAttack;
        /// <summary>
        /// 当更改卡牌生命值，参数为更改输入的生命值。
        /// </summary>
        public event Action<string> onChangeLife;
        /// <summary>
        /// 当更改卡牌标签，参数为更改后的卡牌标签。
        /// </summary>
        public event Action<int> onChangeTag;
        /// <summary>
        /// 当点击编辑效果按钮，参数为效果索引。
        /// </summary>
        public event Action<int> onClickEditEffectButton;
        /// <summary>
        /// 当点击移除效果按钮，参数为效果索引。
        /// </summary>
        public event Action<int> onClickRemoveEffectButton;
        /// <summary>
        /// 当点击添加效果按钮
        /// </summary>
        public event Action onClickAddEffectButton;
        /// <summary>
        /// 当点击背景返回
        /// </summary>
        public event Action onClickBackgroundButton;
        [SerializeField]
        Card _card;
        [SerializeField]
        Text _idText;
        [SerializeField]
        Dropdown _typeDropdown;
        [SerializeField]
        Dropdown _skinDropdown;
        [SerializeField]
        Button _changeSkinNameButton;
        [SerializeField]
        Button _deleteSkinButton;
        [SerializeField]
        InputField _skinNameInputField;
        [SerializeField]
        Dropdown _languageDropdown;
        [SerializeField]
        RectTransform _effectListRoot;
        [SerializeField]
        RectTransform _effectListItemTemplate;
        [SerializeField]
        List<RectTransform> _effectList = new List<RectTransform>();
        [SerializeField]
        Button _addEffectButton;
        [SerializeField]
        Button _editNameButton;
        [SerializeField]
        Button _editSubnameButton;
        [SerializeField]
        Button _editDescButton;
        [SerializeField]
        Button _editTagButton;
        [SerializeField]
        Button _editCostButton;
        [SerializeField]
        Button _editAttackButton;
        [SerializeField]
        Button _editLifeButton;
        [SerializeField]
        Button _editImageButton;
        [SerializeField]
        InputField _nameInputField;
        [SerializeField]
        InputField _subnameInputField;
        [SerializeField]
        InputField _descInputField;
        [SerializeField]
        Dropdown _tagDropdown;
        [SerializeField]
        InputField _costInputField;
        [SerializeField]
        InputField _attackInputField;
        [SerializeField]
        InputField _lifeInputField;
        [SerializeField]
        Button _backgroundButton;
        [SerializeField]
        bool _disableUIEvent = false;
    }
}