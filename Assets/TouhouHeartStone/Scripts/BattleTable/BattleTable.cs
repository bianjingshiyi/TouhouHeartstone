using System;
using Game.Animations;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BattleTable
{
    public class BattleTable : MonoBehaviour
    {
        public void update(float deltaTime)
        {
            _selfField.updateField(deltaTime);
            _enemyField.updateField(deltaTime);
            _selfHandList.updateHand(deltaTime);
            _enemyHandList.updateHand(deltaTime);
            _selfMaster.updateMaster(deltaTime);
            _enemyMaster.updateMaster(deltaTime);
        }
        public Vector3 WorldToTablePosition(Vector3 worldPos)
        {
            return worldPos * _worldToTableScale;
        }
        public Vector3 WorldToTableScale(Vector3 worldScale)
        {
            return worldScale * _worldToTableScale;
        }
        public float WorldToTableDistance(float worldDis)
        {
            return worldDis * _worldToTableScale;
        }
        public Vector3 TableToWorldPosition(Vector3 tablePos)
        {
            return tablePos / _worldToTableScale;
        }
        public Vector3 TableToWorldScale(Vector3 tableScale)
        {
            return tableScale / _worldToTableScale;
        }
        public float TableToWorldDistance(float tableDis)
        {
            return tableDis / _worldToTableScale;
        }
        private void Awake()
        {
            _tableBackground.onPointerDown += OnClickNoWhere;
            _tableBackground.onPointerUp += OnClickNoWhere;
        }
        private void OnClickNoWhere(PointerEventData eventData)
        {
            onClickNoWhere?.Invoke(this, eventData);
        }

        public event Action<BattleTable, PointerEventData> onClickNoWhere;

        #region 属性字段
        public Camera Camera => _camera;
        public GameObject CardTemplate => _cardTemplate;
        public Master SelfMaster => _selfMaster;
        public Master EnemyMaster => _enemyMaster;
        public Gem SelfGem => _selfGem;
        public Gem EnemyGem => _enemyGem;
        public Deck SelfDeck => _selfDeck;
        public Deck EnemyDeck => _enemyDeck;
        public Deck SelfGrave => _selfGrave;
        public Deck EnemyGrave => _enemyGrave;
        public HandList SelfHandList => _selfHandList;
        public HandList EnemyHandList => _enemyHandList;
        public Field SelfField => _selfField;
        public Field EnemyField => _enemyField;
        public Skill SelfSkill => _selfSkill;
        public Skill EnemySkill => _enemySkill;
        public Item SelfItem => _selfItem;
        public Item EnemyItem => _enemyItem;
        public TurnEndButton TurnEndButton => _turnEndButton;
        public Transform InvokeDisplayParent => _invokeDisplayParent;
        public Transform SpellDisplayTransform => _spellDisplayTransform;
        public Transform LeftDisplayTransform => _leftDisplayTransform;
        public Transform CenterDisplayTransform => _centerDisplayTransform;
        public Transform RightDisplayTransform => _rightDisplayTransform;
        public Transform DisplayerCardsParent => _displayerCardsParent;
        public Transform EffectsParent => _effectsParent;
        public Fatigue Fatigue => _fatigue;
        public DiscoverDialog Discover => _discover;
        public AttackArrow AttackArrowImage => _attackArrow;

        public AudioClip playCardSound => _playCardSound;

        [SerializeField]
        private Camera _camera;
        [SerializeField]
        private TableBackground _tableBackground;
        [SerializeField]
        private GameObject _cardTemplate;
        [SerializeField]
        private Master _selfMaster;
        [SerializeField]
        private Master _enemyMaster;
        [SerializeField]
        private Gem _selfGem;
        [SerializeField]
        private Gem _enemyGem;
        [SerializeField]
        private Deck _selfDeck;
        [SerializeField]
        private Deck _enemyDeck;
        [SerializeField]
        private Deck _selfGrave;
        [SerializeField]
        private Deck _enemyGrave;
        [SerializeField]
        private HandList _selfHandList;
        [SerializeField]
        private HandList _enemyHandList;
        [SerializeField]
        private Field _selfField;
        [SerializeField]
        private Field _enemyField;
        [SerializeField]
        private Skill _selfSkill;
        [SerializeField]
        private Skill _enemySkill;
        [SerializeField]
        private Item _selfItem;
        [SerializeField]
        private Item _enemyItem;
        [SerializeField]
        private TurnEndButton _turnEndButton;
        [SerializeField]
        private Transform _invokeDisplayParent;
        [SerializeField]
        private Transform _spellDisplayTransform;
        [SerializeField]
        private Transform _leftDisplayTransform;
        [SerializeField]
        private Transform _centerDisplayTransform;
        [SerializeField]
        private Transform _rightDisplayTransform;
        [SerializeField]
        private Transform _displayerCardsParent;
        [SerializeField]
        private Transform _effectsParent;
        [SerializeField]
        private Fatigue _fatigue;
        [SerializeField]
        private DiscoverDialog _discover;
        [SerializeField]
        private AttackArrow _attackArrow;

        [SerializeField]
        private AudioClip _playCardSound;

        [SerializeField]
        private float _worldToTableScale = 100;
        #endregion
    }
}