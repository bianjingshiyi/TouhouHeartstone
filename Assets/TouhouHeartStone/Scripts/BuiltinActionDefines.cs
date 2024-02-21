using TouhouCardEngine;

namespace Game
{
    public static class ObsoletedActionDefineID
    {
        public const int DealSpecificDamageToSpecificNumberOfRandomTarget = -2;
        public const int DealSpecificSpellDamageToSpecificNumberOfRandomTarget = -3;
        public const int DealSpecificSpellDamageToTargets = -4;
        public const int GetCardsCount = -5;
        public const int GetCardsInDeckWithCondition = -6;
        public const int GetCardsWithCondition = -7;
        public const int GetRandomCardDefine = -8;
        public const int GetRandomCard = -9;
        public const int GetTarget = -10;
        public const int HealSpecificAmountToTargets = -11;
        public const int IsCardDefineCollectible = -12;
        public const int IsPlayerPileChanged = -13;
        public const int IsSpellCard = -14;
    }
    public static class BuiltinActionDefineID
    {
        public const int Type_Function = 100000;
        public const int Type_Action = 200000;
        public const int Category_Game = 0;
        public const int Category_Player = 1000;
        public const int Category_Card = 2000;
        public const int Category_CardDefine = 3000;
        public const int Category_CardSnapshot = 4000;
        public const int Category_Pile = 5000;
        public const int Category_Buff = 6000;
        public const int Category_Effect = 7000;
        public const int Category_Event = 8000;
        public const int Category_EventRecord = 9000;
        public const int Category_Action = 10000;
        public const int Category_Collection = 11000;
        public const int Category_Other = 12000;

        #region 函数
        // 玩家
        public const int GetPile_Obsolete = Type_Function + Category_Player + 1;    // 获取牌堆（过时）
        public const int GetPlayerProp = Type_Function + Category_Player + 2;    // 获取玩家属性
        public const int GetPlayerOpponent = Type_Function + Category_Player + 3;    // 获取玩家对手
        public const int GetPlayerMaster = Type_Function + Category_Player + 4;    // 获取玩家英雄
        public const int GetCurrentPlayer = Type_Function + Category_Player + 5;    // 获取当前回合玩家
        public const int GetPlayerHands = Type_Function + Category_Player + 6;    // 获取玩家的手牌
        public const int GetPlayerGraveCards = Type_Function + Category_Player + 7;    // 获取玩家的墓地卡牌
        public const int GetPlayerDeckCards = Type_Function + Category_Player + 8;    // 获取玩家牌库中的卡牌
        public const int GetPlayerHeadCards = Type_Function + Category_Player + 9;    // 获取玩家延迟区
        public const int GetPlayerSkill = Type_Function + Category_Player + 10;    // 获取玩家技能
        public const int GetPlayerItem = Type_Function + Category_Player + 11;    // 获取玩家道具
        public const int DoesPlayerHaveItem = Type_Function + Category_Player + 12;    // 玩家是否装备道具
        public const int GetPlayerWarpCards = Type_Function + Category_Player + 13;    // 获取玩家暂存区
        public const int GetPlayerGem = Type_Function + Category_Player + 14;    // 获取当前灵力值
        public const int GetPlayerMaxGem = Type_Function + Category_Player + 15;    // 获取灵力上限
        public const int GetPlayerSpellDamage = Type_Function + Category_Player + 16;    // 获取玩家法术伤害
        public const int GetPile = Type_Function + Category_Player + 17;    // 获取牌堆
        public const int GetPlayerTurnCount = Type_Function + Category_Player + 18;    // 获取玩家的回合数
        public const int IsOffensivePlayer = Type_Function + Category_Player + 19;    // 玩家是否是先手
        public const int GetAllPlayers = Type_Function + Category_Player + 20;    // 获取所有玩家
        public const int GetFatigue = Type_Function + Category_Player + 21; // 获取疲劳层数

        // 卡牌
        public const int ThisCard = Type_Function + Category_Card + 1; // 这张卡牌
        public const int IsCardOfType_Obs_CardType = Type_Function + Category_Card + 2;    // 卡牌类型判断（过时）
        public const int CardHasKeyword = Type_Function + Category_Card + 3;    // 卡牌关键词判断
        public const int GetCardProp_Obsolete = Type_Function + Category_Card + 4;    // 获取属性（过时）
        public const int GetCardCost = Type_Function + Category_Card + 5;    // 获取花费
        public const int GetCardAttack = Type_Function + Category_Card + 6;    // 获取攻击力
        public const int GetCardMaxLife = Type_Function + Category_Card + 7;    // 获取最大生命值
        public const int GetCardCurrentLife = Type_Function + Category_Card + 8;    // 获取当前生命值
        public const int GetUniquePropertyName = Type_Function + Category_Card + 9;    // 获取唯一属性名
        public const int GetCardOwner = Type_Function + Category_Card + 10;    // 获取卡牌拥有者
        public const int GetCardSpellDamage = Type_Function + Category_Card + 11;    // 获取卡牌法术伤害
        public const int GetAllServants = Type_Function + Category_Card + 12;    // 获取所有随从
        public const int GetAllCharacters = Type_Function + Category_Card + 13;    // 获取所有角色
        public const int GetFriendlyServants = Type_Function + Category_Card + 14;    // 获取友方随从
        public const int GetFriendlyCharacters = Type_Function + Category_Card + 15;    // 获取友方角色
        public const int GetEnemyServants = Type_Function + Category_Card + 16;    // 获取敌方随从
        public const int GetEnemyCharacters = Type_Function + Category_Card + 17;    // 获取敌方角色
        public const int IsDead = Type_Function + Category_Card + 18;    // 是否濒死
        public const int GetArmor = Type_Function + Category_Card + 19;    // 获取护甲值
        public const int GetNearbyCards = Type_Function + Category_Card + 20;    // 获取相邻卡牌
        public const int CardHasBattlecry = Type_Function + Category_Card + 21;    // 卡牌具有宣言
        public const int CardHasDeathrattle = Type_Function + Category_Card + 22;    // 卡牌具有遗言
        public const int GetValidCardTargets = Type_Function + Category_Card + 23;    // 获取合法卡牌使用目标
        public const int GetValidAttackTargets = Type_Function + Category_Card + 24;    // 获取合法攻击目标
        public const int IsCharged = Type_Function + Category_Card + 25;    // 是否已蓄力
        public const int GetChargedLevel = Type_Function + Category_Card + 26;    // 获取蓄力层数
        public const int GetCardProp = Type_Function + Category_Card + 27;    // 获取卡牌属性
        public const int CardIsTrap = Type_Function + Category_Card + 28;    // 卡牌是否是陷阱
        public const int GetCardPile = Type_Function + Category_Card + 29;    // 获取卡牌所在牌堆
        public const int IsCardPropVisibleTo = Type_Function + Category_Buff + 30; // 卡牌属性是否可见
        public const int IsCardVisibleTo = Type_Function + Category_Buff + 31; // 卡牌是否可见
        public const int IsCardOfType = Type_Function + Category_Card + 32;    // 卡牌类型判断


        // 卡牌定义
        public const int GetAllCardDefines = Type_Function + Category_CardDefine + 1;    // 获取所有卡牌定义
        public const int GetCardDefine = Type_Function + Category_CardDefine + 2;    // 获取卡牌定义
        public const int GetCardDefineArray = Type_Function + Category_CardDefine + 3;    // 获取卡牌定义列表
        public const int CardDefineHasTag = Type_Function + Category_CardDefine + 4;    // 卡牌定义是否具有标签
        public const int IsCardDefineOfType_Obs_CardType = Type_Function + Category_CardDefine + 5;    // 卡牌定义类型判断（过时）
        public const int CardDefineHasKeyword = Type_Function + Category_CardDefine + 6;    // 卡牌定义关键词判断
        public const int GetCardDefinesOfCardArray = Type_Function + Category_CardDefine + 7;    // 获取目标卡牌的定义列表
        public const int GetCardDefineOfCard = Type_Function + Category_CardDefine + 8;    // 获取单张卡牌的定义
        public const int GetCardDefineProp_Obsolete = Type_Function + Category_CardDefine + 9;    // 获取卡牌定义属性（过时）
        public const int GetCardDefineCost = Type_Function + Category_CardDefine + 10;    // 获取卡牌定义花费
        public const int GetCardDefineAttack = Type_Function + Category_CardDefine + 11;    // 获取卡牌定义攻击力
        public const int GetCardDefineMaxLife = Type_Function + Category_CardDefine + 12;    // 获取卡牌定义生命值
        public const int GetCardDefineSpellDamage = Type_Function + Category_CardDefine + 13;    // 获取卡牌定义法术伤害
        public const int IsCardDefineToken = Type_Function + Category_CardDefine + 14;    // 是否为衍生牌
        public const int GetCardDefineSkill = Type_Function + Category_CardDefine + 15;    // 获取英雄牌技能
        public const int GetCardDefineCharacter = Type_Function + Category_CardDefine + 16;    // 获取卡牌定义对应角色
        public const int CardDefineHasBattlecry = Type_Function + Category_CardDefine + 17;    // 卡牌定义具有宣言
        public const int CardDefineHasDeathrattle = Type_Function + Category_CardDefine + 18;    // 卡牌定义具有遗言
        public const int GetCardDefineTagList = Type_Function + Category_CardDefine + 19;    // 获取卡牌定义标签列表
        public const int GetCardDefineProp = Type_Function + Category_CardDefine + 20;    // 获取卡牌定义属性
        public const int CardDefineIsTrap = Type_Function + Category_CardDefine + 21;    // 卡牌定义是否是陷阱
        public const int GetValidCardDefineTargets = Type_Function + Category_CardDefine + 22;    // 获取卡牌定义的合法使用目标
        public const int GetCardDefineType = Type_Function + Category_CardDefine + 23; // 获取卡牌定义类型
        public const int IsCardDefineOfType = Type_Function + Category_CardDefine + 24;    // 卡牌定义类型判断


        // 卡牌快照
        public const int GetCardOfCardSnapshot = Type_Function + Category_CardSnapshot + 1;    // 获取卡牌快照对应卡牌
        public const int GetCardDefineOfCardSnapshot = Type_Function + Category_CardSnapshot + 2;    // 获取卡牌快照卡牌定义
        public const int CheckCardSnapshotType_Obs_CardType = Type_Function + Category_CardSnapshot + 3;    // 卡牌快照类型判断（过时）
        public const int CardSnapshotHasKeywords = Type_Function + Category_CardSnapshot + 4;    // 卡牌快照关键词判断
        public const int GetCardSnapshotProp = Type_Function + Category_CardSnapshot + 5;    // 获取卡牌快照属性
        public const int GetCardSnapshotCost = Type_Function + Category_CardSnapshot + 6;    // 获取卡牌快照花费
        public const int GetCardSnapshotAttack = Type_Function + Category_CardSnapshot + 7;    // 获取卡牌快照攻击力
        public const int GetCardSnapshotMaxLife = Type_Function + Category_CardSnapshot + 8;    // 获取卡牌快照最大生命值
        public const int GetCardSnapshotLife = Type_Function + Category_CardSnapshot + 9;    // 获取卡牌快照当前生命值
        public const int GetCardSnapshotOwner = Type_Function + Category_CardSnapshot + 10;    // 获取卡牌快照拥有者
        public const int GetCardSnapshotArmor = Type_Function + Category_CardSnapshot + 11;    // 获取卡牌快照护甲值
        public const int IsCardSnapshotCharged = Type_Function + Category_CardSnapshot + 12;    // 卡牌快照是否已蓄力
        public const int GetCardSnapshotChargedLevel = Type_Function + Category_CardSnapshot + 13;    // 获取卡牌快照蓄力层数
        public const int DoesCardSnapshotHaveBattlecry = Type_Function + Category_CardSnapshot + 14;    // 卡牌快照具有宣言
        public const int DoesCardSnapshotHaveDeathrattle = Type_Function + Category_CardSnapshot + 15;    // 卡牌快照具有遗言
        public const int CardSnapshotIsTrap = Type_Function + Category_CardSnapshot + 16;    // 卡牌快照是否是陷阱
        public const int CheckCardSnapshotType = Type_Function + Category_CardSnapshot + 17;    // 卡牌快照类型判断


        // 牌堆
        public const int CheckCardPile_Obsolete = Type_Function + Category_Pile + 1;    // 卡牌所在牌堆判断（过时）
        public const int GetCardsInPile = Type_Function + Category_Pile + 2;    // 获取牌堆中的卡牌
        public const int GetPileName = Type_Function + Category_Pile + 3;    // 获取牌堆名
        public const int GetOwnerOfPile = Type_Function + Category_Pile + 4;    // 获取牌堆所属玩家
        public const int CheckCardPile = Type_Function + Category_Pile + 5;    // 卡牌所在牌堆判断

        // 增益
        public const int ThisBuff = Type_Function + Category_Buff + 1; // 该增益
        public const int GetBuffDefine = Type_Function + Category_Buff + 2;    // 获取增益定义
        public const int HasBuff = Type_Function + Category_Buff + 3;    // 是否具有增益
        public const int GetBuffProp = Type_Function + Category_Buff + 4;    // 获取增益属性
        public const int GetBuffDefineOfBuff = Type_Function + Category_Buff + 5;    // 获取单个增益的定义
        public const int GetAllBuffsOnCard = Type_Function + Category_Buff + 6;    // 获取卡牌上的所有增益
        public const int IsBuffPropVisibleTo = Type_Function + Category_Buff + 7; // 增益属性是否可见

        // 效果
        public const int ThisEffect = Type_Function + Category_Effect + 1; // 该效果
        public const int EffectHasTag = Type_Function + Category_Effect + 2;    // 效果具有标签
        public const int GetEffectProp = Type_Function + Category_Effect + 3;    // 获取效果属性
        public const int GetAllCardEffects = Type_Function + Category_Effect + 4;    // 获取卡牌的所有效果
        public const int GetAllBuffEffects = Type_Function + Category_Effect + 5;    // 获取增益的所有效果
        public const int GetAllCardDefineEffects = Type_Function + Category_Effect + 6;    // 获取卡牌定义的所有效果
        public const int GetValidEffectTargets = Type_Function + Category_Effect + 7;    // 获取效果的合法使用目标
        public const int CheckEffectType = Type_Function + Category_Effect + 8; // 效果类型判断
        public const int GetEffectCardDefineOrBuffDefine = Type_Function + Category_Effect + 9; // 获取效果所属卡牌定义或增益定义
        public const int CheckEventEffectType = Type_Function + Category_Effect + 10; // 事件效果类型判断

        // 事件
        public const int ThisEvent = Type_Function + Category_Event + 1;    // 当前事件
        public const int GetEventVariable = Type_Function + Category_Event + 2;    // 获取变量
        public const int CastEventType = Type_Function + Category_Event + 3;    // 转换事件类型
        public const int CheckEventType = Type_Function + Category_Event + 4;    // 事件类型判断
        public const int GetEventsThisGame = Type_Function + Category_Event + 5;    // 获取本局游戏事件
        public const int GetEventsThisTurn = Type_Function + Category_Event + 6;    // 获取本回合事件
        public const int GetEventsInTurnsBefore = Type_Function + Category_Event + 7;    // 获取前x-y回合内的事件
        public const int GetCardSnapshotBeforeEvent = Type_Function + Category_Event + 8;    // 获取卡牌在某事件前的快照
        public const int GetCardSnapshotAfterEvent = Type_Function + Category_Event + 9;    // 获取卡牌在某事件后的快照
        public const int GetEventRepeatTimes = Type_Function + Category_Event + 10;    // 获取事件重复次数

        // 事件记录
        public const int GetTurnCount_Obsolete = Type_Function + Category_EventRecord + 1;    // 获得当前回合数（过时）
        public const int GetEventRecordVariable = Type_Function + Category_EventRecord + 2;    // 获取事件记录变量
        public const int CheckEventRecordType = Type_Function + Category_EventRecord + 3;    // 事件记录类型判断
        public const int GetEventRecordsThisGame = Type_Function + Category_EventRecord + 4;    // 获取本局游戏事件记录
        public const int GetEventRecordsThisTurn = Type_Function + Category_EventRecord + 5;    // 获取本回合事件记录
        public const int GetEventRecordsInTurnsBefore = Type_Function + Category_EventRecord + 6;    // 获取前x-y回合内的事件记录
        public const int GetCardSnapshotOfEvent = Type_Function + Category_EventRecord + 7;    // 获取卡牌在某事件时的快照
        public const int GetTurnCount = Type_Function + Category_EventRecord + 8;    // 获得当前回合数
        public const int CastEventRecordType = Type_Function + Category_EventRecord + 9;    // 转换事件记录类型

        // 集合运算
        public const int CreateArray = Type_Function + Category_Collection + 1;    // 创建集合
        public const int AddElementToArray = Type_Function + Category_Collection + 2;    // 向集合添加元素
        public const int Where_Obsolete = Type_Function + Category_Collection + 3;    // 筛选（过时）
        public const int GetCount = Type_Function + Category_Collection + 4;    // 获取元素数量
        public const int Contains = Type_Function + Category_Collection + 5;    // 包含
        public const int OrderBy_Obsolete = Type_Function + Category_Collection + 6;    // 排序（过时）
        public const int GetElementByIndex = Type_Function + Category_Collection + 7;    // 获取第X个元素
        public const int GetRandomElement = Type_Function + Category_Collection + 8;    // 获取集合中的随机元素
        public const int ReverseCollection = Type_Function + Category_Collection + 9;    // 反转集合
        public const int Distinct = Type_Function + Category_Collection + 10;    // 集合去重
        public const int Except = Type_Function + Category_Collection + 11;    // 集合相减
        public const int Where = Type_Function + Category_Collection + 12;    // 筛选
        public const int OrderBy = Type_Function + Category_Collection + 13;    // 排序
        public const int Sum = Type_Function + Category_Collection + 14; // 求和
        public const int Min = Type_Function + Category_Collection + 15; // 获取最小值
        public const int Max = Type_Function + Category_Collection + 16; // 获取最大值
        public const int Average = Type_Function + Category_Collection + 17; // 获取平均值
        public const int Concat = Type_Function + Category_Collection + 18; // 集合相加
        public const int Union = Type_Function + Category_Collection + 19; // 获取并集
        public const int Intersect = Type_Function + Category_Collection + 20; // 获取交集
        public const int IsSubset = Type_Function + Category_Collection + 21; // 是否是某集合的子集
        public const int All = Type_Function + Category_Collection + 22; // 是否所有元素都满足条件
        public const int Any = Type_Function + Category_Collection + 23; // 是否有任意元素满足条件
        public const int First = Type_Function + Category_Collection + 24; // 获取第一个元素
        public const int Last = Type_Function + Category_Collection + 25; // 获取最后一个元素
        public const int Take = Type_Function + Category_Collection + 26; // 获取集合内前X个元素
        public const int Skip = Type_Function + Category_Collection + 27; // 获取集合内前X个元素之外的元素
        public const int RandomTake = Type_Function + Category_Collection + 28; // 获取随机X个元素
        public const int TakeWhile = Type_Function + Category_Collection + 29; // 获取集合内结果直到条件不成立
        public const int SkipWhile = Type_Function + Category_Collection + 30; // 条件不成立后获取剩余结果
        public const int Select = Type_Function + Category_Collection + 31; // 获取集合内所有元素的某项属性
        public const int Randomize = Type_Function + Category_Collection + 32; // 打乱集合内元素的顺序
        public const int ContentEquals = Type_Function + Category_Collection + 33; // 集合内容是否相同
        public const int IndexOf = Type_Function + Category_Collection + 34; // 获取元素在集合中的位置
        public const int Merge = Type_Function + Category_Collection + 35; // 合并多个集合
        public const int CastCollectionType = Type_Function + Category_Collection + 36; // 转换集合类型

        // 其他
        public const int BooleanConst = Type_Function + Category_Other + 1;       // 固定真值
        public const int Compare = Type_Function + Category_Other + 2;            // 比较
        public const int IntegerConst = Type_Function + Category_Other + 3;    // 固定整数
        public const int IntegerOperation = Type_Function + Category_Other + 4;    // 整数运算
        public const int LogicOperation = Type_Function + Category_Other + 5;    // 逻辑运算
        public const int StringConst = Type_Function + Category_Other + 6;    // 固定字符串
        public const int GetTempVar = Type_Function + Category_Other + 7;    // 获取临时变量
        public const int RandomInt = Type_Function + Category_Other + 8;    // x到y之间的随机整数
        public const int IsNotExists = Type_Function + Category_Other + 9;    // 是否不存在
        public const int ChooseIf = Type_Function + Category_Other + 10; // 根据条件选择值

        #endregion 函数

        #region 动作
        // 游戏
        public const int MakePlayersWin = Type_Action + Category_Game + 1;      // 使玩家获胜
        public const int MakePlayersLose = Type_Action + Category_Game + 2;      // 使玩家失败
        public const int MakePlayersDraw = Type_Action + Category_Game + 3;      // 使玩家平局
        public const int CreatePile = Type_Action + Category_Game + 4;      // 创建牌堆
        public const int RemovePile = Type_Action + Category_Game + 5;      // 删除牌堆
        public const int MoveCardFromPileToPile = Type_Action + Category_Game + 6;      // 从牌堆将卡牌移动到目标牌堆

        // 玩家
        public const int SetPlayerGem = Type_Action + Category_Player + 1;    // 设置当前灵力值
        public const int SetPlayerProp = Type_Action + Category_Player + 2;    // 设置玩家属性
        public const int DrawTargetCard = Type_Action + Category_Player + 3;    // 抽目标卡牌
        public const int SelectOneFromCards_Obs_Rule = Type_Action + Category_Player + 4;    // 从一定数量的卡牌中选择一张（自定义规则过时）
        public const int SelectOneFromCardDefines_Obs_Rule = Type_Action + Category_Player + 5;    // 从一定数量的卡牌定义中选择一张（自定义规则过时）
        public const int ResetSkill = Type_Action + Category_Player + 6;    // 复原技能
        public const int DestroyItem = Type_Action + Category_Player + 7;    // 摧毁道具
        public const int AddPlayerGem = Type_Action + Category_Player + 8;    // 增加当前灵力值
        public const int SetPlayerMaxGem = Type_Action + Category_Player + 9;    // 设置灵力上限
        public const int AddPlayerMaxGem = Type_Action + Category_Player + 10;    // 增加灵力上限
        public const int FaithDuel_Obsolete = Type_Action + Category_Player + 11;    // 信仰对决（过时）
        public const int FaithDuel = Type_Action + Category_Player + 12;    // 信仰对决
        public const int SetFatigue = Type_Action + Category_Player + 13; // 设置疲劳层数
        public const int AddFatigue = Type_Action + Category_Player + 14; // 增加疲劳层数
        public const int TakeFatigueDamage = Type_Action + Category_Player + 15; // 受到疲劳伤害
        public const int SelectOneFromCards = Type_Action + Category_Player + 16;    // 从一定数量的卡牌中选择一张
        public const int SelectOneFromCardDefines = Type_Action + Category_Player + 17;    // 从一定数量的卡牌定义中选择一张

        // 卡牌
        public const int Damage = Type_Action + Category_Card + 1;    // 造成伤害
        public const int Discard_Obs_Mass = Type_Action + Category_Card + 2;    // 丢弃卡牌（群体事件过时）
        public const int CreateTokenToField = Type_Action + Category_Card + 3;    // 创建衍生卡并置入战场
        public const int CreateTokenToHand = Type_Action + Category_Card + 4;    // 创建衍生卡并置入手牌
        public const int CreateTokenToDeck = Type_Action + Category_Card + 5;    // 创建衍生卡并洗入牌库
        public const int CreateTokenToItem = Type_Action + Category_Card + 6;    // 创建衍生卡并装备到道具栏
        public const int SetCardProp_Obsolete = Type_Action + Category_Card + 7;    // 设置属性（过时）
        public const int DealDamageOrSpellDamage_Obsolete = Type_Action + Category_Card + 8;    // 造成伤害或法伤（过时）
        public const int DealDamageSplitAmong_Obsolete = Type_Action + Category_Card + 9;    // 造成伤害或法伤并分配给目标（过时）
        public const int DealDamageToRandomTargets_Obsolete = Type_Action + Category_Card + 10;    // 对固定数量的随机目标造成伤害或法伤（过时）
        public const int DealSpellDamageToTargets = Type_Action + Category_Card + 11;    // 造成法术伤害
        public const int DealSpecificSpellDamageToRandomTargets = Type_Action + Category_Card + 12;    // 造成固定法术伤害并分配给随机目标
        public const int HealTargets_Obs_Mass = Type_Action + Category_Card + 13;    // 治疗目标卡牌（群体事件过时）
        public const int Shackle = Type_Action + Category_Card + 14;    // 禁锢
        public const int Seal_Obs_Mass = Type_Action + Category_Card + 15;    // 沉默（群体事件过时）
        public const int Kill = Type_Action + Category_Card + 16;    // 消灭
        public const int ForceUpdateCardsState = Type_Action + Category_Card + 17;    // 强制更新所有卡牌状态
        public const int AddAmountOfArmor = Type_Action + Category_Card + 18;    // 增加固定数量护甲
        public const int AddArmor = Type_Action + Category_Card + 19;    // 增加护甲
        public const int LoseArmor = Type_Action + Category_Card + 20;    // 失去护甲
        public const int TriggerCardBattlecries = Type_Action + Category_Card + 21;    // 触发卡牌宣言
        public const int TriggerCardDeathrattles = Type_Action + Category_Card + 22;    // 触发卡牌遗言
        public const int TriggerCardActiveEffect = Type_Action + Category_Card + 23;    // 触发法术或卡牌效果
        public const int ResetCard = Type_Action + Category_Card + 24;    // 重置卡牌
        public const int ForceAttack = Type_Action + Category_Card + 25;    // 强制攻击目标
        public const int AddAttackChance = Type_Action + Category_Card + 26;    // 本回合攻击次数增加一次
        public const int RevealCard = Type_Action + Category_Card + 27;    // 揭示卡牌
        public const int TakeControl = Type_Action + Category_Card + 28;    // 获得控制权
        public const int SetCardDefineOfCard = Type_Action + Category_Card + 29;    // 变形为卡牌定义
        public const int TriggerCardDefineBattlecries = Type_Action + Category_Card + 30;    // 触发卡牌定义宣言
        public const int TriggerCardDefineDeathrattles = Type_Action + Category_Card + 31;    // 触发卡牌定义遗言
        public const int TriggerCardDefineActiveEffect = Type_Action + Category_Card + 32;    // 触发法术或技能卡牌定义效果
        public const int SetChargedLevel = Type_Action + Category_Card + 33;    // 设置蓄力层数
        public const int DealDamageOrSpellDamage_Obs_Mass = Type_Action + Category_Card + 34;    // 造成伤害或法伤（群体事件过时）
        public const int DealDamageSplitAmong_Obs_Mass = Type_Action + Category_Card + 35;    // 造成伤害或法伤兵分配给目标（群体事件过时）
        public const int DealDamageToRandomTargets_Obs_Mass = Type_Action + Category_Card + 36;    // 对固定数量的随机目标造成伤害或法伤（群体事件过时）
        public const int SetCardProp = Type_Action + Category_Card + 37;    // 设置卡牌属性
        public const int Discard = Type_Action + Category_Card + 38;    // 丢弃卡牌
        public const int HealTargets_Obs_Rule = Type_Action + Category_Card + 39;    // 治疗目标卡牌（自定义规则过时）
        public const int Seal = Type_Action + Category_Card + 40;    // 封印
        public const int DealDamageOrSpellDamage = Type_Action + Category_Card + 41;    // 造成伤害或法伤
        public const int DealDamageSplitAmong = Type_Action + Category_Card + 42;    // 造成伤害或法伤并分配给目标
        public const int DealDamageToRandomTargets = Type_Action + Category_Card + 43;    // 对固定数量的随机目标造成伤害或法伤
        public const int CloneCard = Type_Action + Category_Card + 44;    // 复制卡牌
        public const int CreateTokenToHead = Type_Action + Category_Card + 45;    // 创建衍生卡并置入延迟区
        public const int CreateTokenToPile = Type_Action + Category_Card + 46;    // 创建衍生卡并置入牌堆
        public const int HealTargets = Type_Action + Category_Card + 47;    // 治疗目标卡牌
        public const int SetCardPropVisibility = Type_Action + Category_Buff + 48; // 设置卡牌属性可见性
        public const int SetCardVisibility = Type_Action + Category_Buff + 49; // 设置卡牌可见性

        // 卡牌定义
        public const int RevealCardDefine = Type_Action + Category_CardDefine + 1; // 展示卡牌定义

        // 增益
        public const int AddBuff = Type_Action + Category_Buff + 1;    // 添加增益
        public const int RemoveBuff = Type_Action + Category_Buff + 2;    // 移除增益
        public const int SetBuffProp = Type_Action + Category_Buff + 3;    // 设置增益属性
        public const int SetBuffPropVisibility = Type_Action + Category_Buff + 4; // 设置增益属性可见性

        // 效果
        public const int TriggerEffect = Type_Action + Category_Effect + 1; // 发动效果

        // 事件
        public const int SetEventVariable = Type_Action + Category_Event + 1;    // 设置变量
        public const int StopEvent = Type_Action + Category_Event + 2;    // 阻止事件
        public const int ChangeAttackTarget = Type_Action + Category_Event + 3;    // 更改攻击目标
        public const int ChangeUseTarget = Type_Action + Category_Event + 4;    // 更改使用目标
        public const int DamageEvent_ChangeTarget = Type_Action + Category_Event + 5;    // 伤害/更改受伤卡牌
        public const int DamageEvent_ChangeSource = Type_Action + Category_Event + 6;    // 伤害/更改伤害源
        public const int DamageEvent_ChangeValue = Type_Action + Category_Event + 7;    // 伤害/更改伤害值
        public const int HealEvent_ChangeValue = Type_Action + Category_Event + 8;    // 治疗/更改治疗值
        public const int SetEventRepeatTimes = Type_Action + Category_Event + 9;    // 设置事件重复次数

        // 行动
        public const int SimpleDraw = Type_Action + Category_Action + 1;    // 简单抽牌
        public const int MoveToField = Type_Action + Category_Action + 2;    // 卡牌置入战场
        public const int MoveToHand = Type_Action + Category_Action + 3;    // 卡牌移回手牌
        public const int MoveToDeck = Type_Action + Category_Action + 4;    // 卡牌洗入牌库
        public const int MoveToGrave = Type_Action + Category_Action + 5;    // 卡牌置入墓地
        public const int MoveToHead = Type_Action + Category_Action + 6;    // 卡牌置入延迟区
        public const int MoveToItem = Type_Action + Category_Action + 7;    // 卡牌装备到道具栏
        public const int MoveToWarp = Type_Action + Category_Action + 8;    // 卡牌移动到暂存区

        // 集合运算
        public const int ForeachAction = Type_Action + Category_Collection + 1;    // 遍历

        // 其他
        public const int BranchAction = Type_Action + Category_Other + 1;    // 分支动作
        public const int ForAction = Type_Action + Category_Other + 2;    // 重复动作
        public const int DoUntilAction_Obsolete = Type_Action + Category_Other + 3;    // 重复动作直到（过时）
        public const int SetTempVar = Type_Action + Category_Other + 4;    // 设置临时变量
        public const int DoUntilAction = Type_Action + Category_Other + 5;    // 重复动作直到
        public const int Break = Type_Action + Category_Other + 6;    // 跳出循环
        public const int Continue = Type_Action + Category_Other + 7;    // 跳过本次循环

        #endregion

        public static NodeDefineType getNodeDefineType(this int id)
        {
            var typeFlag = id / 100000 * 100000;
            if (typeFlag == Type_Function)
            {
                return NodeDefineType.Function;
            }
            return NodeDefineType.Action;
        }
    }
    public static class ActionReferenceHelper
    {
        public static bool isBuiltinReference(this ActionReference actRef, int id)
        {
            if (actRef == null)
                return false;
            return actRef.cardPoolId == 0 && actRef.defineId == id;
        }
    }
}
