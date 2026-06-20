using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GamePhase
{
    playerDraw, playerAction, enemyDraw, enemyAction, gameStart
}
/*public enum GameEvent
{
    phaseChange, monsterDestroy
}*/
public class BattleManager : MonoSingleton<BattleManager>
{
    public GameObject playerData; // 数据
    public GameObject enemyData;
    public GameObject playerHands; // 手牌
    public GameObject enemyHands;
    public GameObject[] playerBlocks; // 怪兽区
    public GameObject[] enemyBlocks;
    public List<Card> playerDeckList = new List<Card>(); // 卡组
    public List<Card> enemyDeckList = new List<Card>();

    public GameObject cardPrefab;

    public GameObject arrowPrefab;//召唤指示箭头
    public GameObject attackPrefab;//攻击指示箭头
    private GameObject arrow;


    // 生命值
    public int playerHealthPoint;
    public int enemyHealthPoint;

    // 血量UI（拖头像下的Text）
    public Text playerHpText;
    public Text enemyHpText;
    // 新增：对局结束后跳转的场景名（在Inspector可修改）
    public string backToScene = "BuildDeck";

    public GameObject playerIcon;
    public GameObject enemyIcon;


    // 召唤次数
    public int maxPlayerSummonCount;
    public int playerSummonCount;
    public int maxEnemySummonCount;
    public int enemySummonCount;


    public GamePhase currentPhase = GamePhase.playerDraw;


    protected CardData CardDate;

    public Transform canvas;


    private GameObject waitingMonster;
    private int waitingID;
    public GameObject attackingMonster;
    private int attackingID;

    // private Dictionary<string, UnityEvent> eventDic = new Dictionary<string, UnityEvent>();
    public UnityEvent phaseChangeEvent;

    // 🔥【仅新增这1行】标记是否可以直接攻击玩家
    private bool canStraightAttack;

    public Transform playerDiscardPile; // 玩家弃牌堆（空物体即可）
    public Transform enemyDiscardPile;  // 敌方弃牌堆
    public LayerMask monsterLayer;       // 怪兽碰撞层（用于目标选择）

    public string victoryScene = "VictoryScene";       // 胜利界面
    public string defeatScene = "DefeatScene";         // 失败界面

    // Start is called before the first frame update
    void Start()
    {      
        GameStart();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void CheckGameOver()
    {
        // 敌方血量<=0 → 玩家获胜
        if (enemyHealthPoint <= 0)
        {
            Debug.Log("✅ 敌方血量归零，玩家胜利，返回构筑界面");
            // 🔥 新增：胜利加100金币
            int currentCoins = PlayerPrefs.GetInt("PlayerCoins", 0); // 读取当前金币，默认0
            currentCoins += 100; // 加100
            PlayerPrefs.SetInt("PlayerCoins", currentCoins); // 保存
            PlayerPrefs.Save(); // 立即写入磁盘
            Debug.Log($"🎁 获得胜利奖励100金币！当前金币：{currentCoins}");
            SceneManager.LoadScene(victoryScene);
            return;
        }
        // 我方血量<=0 → 玩家战败
        if (playerHealthPoint <= 0)
        {
            Debug.Log("❌ 我方血量归零，对局失败，返回构筑界面");
            SceneManager.LoadScene(defeatScene);
            return;
        }
    }
    // 刷新血量UI
    public void RefreshHpUI()
    {
        if (playerHpText != null)
            playerHpText.text = playerHealthPoint.ToString();

        if (enemyHpText != null)
            enemyHpText.text = enemyHealthPoint.ToString();
    }

    public void OnPlayerDrawCard()
    {
        if (currentPhase == GamePhase.playerDraw)
        {
            DrawCard(0, 1);           
        }
    }
    public void OnEnemyDrawCard()
    {
        if (currentPhase == GamePhase.enemyDraw)
        {
            DrawCard(1, 1);  
        }
    }
    public void DrawCard(int _player, int _number, bool _back = false, bool _state=true)
    {
        // 🔥 新增：手牌上限6张，超过不能抽
        int handCount = _player == 0 ? playerHands.transform.childCount : enemyHands.transform.childCount;
        if (handCount >= 6)
        {
            Debug.Log("手牌已满，无法抽第7张！直接进入战斗阶段");
            if (_player == 0)
                currentPhase = GamePhase.playerAction;
            else
                currentPhase = GamePhase.enemyAction;
            phaseChangeEvent.Invoke();
            return;
        }
        if (_player == 0)
        {
            if (playerDeckList.Count <= 0)
            {
                Debug.Log("玩家卡组已空，无法抽卡，直接进入玩家行动阶段");
                currentPhase = GamePhase.playerAction;
                phaseChangeEvent.Invoke();
                return;
            }
            for (int i = 0; i < _number; i++)
            {
                GameObject newCard = GameObject.Instantiate(cardPrefab, playerHands.transform);
                newCard.GetComponent<CardDisplay>().card = playerDeckList[0];
                playerDeckList.RemoveAt(0);
                newCard.GetComponent<BattleCard>().cardState = CardState.inPlayerHand;
                // 显示卡背
                if (_back)
                {
                    newCard.GetComponent<CardDisplay>().back = true;
                }
                if (_state)
                {
                    currentPhase = GamePhase.playerAction;
                    phaseChangeEvent.Invoke();
                }
            }

        }
        else if (_player == 1)
        {
            // 🔥 加：卡组为空直接返回
            if (enemyDeckList.Count <= 0)
            {
                Debug.Log("敌方卡组已空，无法抽卡，直接进入敌方行动阶段");
                currentPhase = GamePhase.enemyAction;
                phaseChangeEvent.Invoke();
                return;
            }
            for (int i = 0; i < _number; i++)
            {
                GameObject newCard = GameObject.Instantiate(cardPrefab, enemyHands.transform);
                newCard.GetComponent<CardDisplay>().card = enemyDeckList[0];
                enemyDeckList.RemoveAt(0);
                newCard.GetComponent<BattleCard>().cardState = CardState.inEnemyHand;
                // 显示卡背
                if (_back)
                {
                    newCard.GetComponent<CardDisplay>().back = true;
                }
                if (_state)
                {
                    currentPhase = GamePhase.enemyAction;
                    phaseChangeEvent.Invoke();
                }
            }
        }     
    }

    public virtual void OnClickTurnEnd()
    {
        TurnEnd();
    }
    public void TurnEnd()
    {
        
        if (arrow != null)
        {
            Destroy(arrow);
        }
        if (currentPhase == GamePhase.playerAction)
        {
            currentPhase = GamePhase.enemyDraw;
            enemySummonCount = maxEnemySummonCount;

            playerIcon.GetComponent<AttackTarget>().attackable = true;
            enemyIcon.GetComponent<AttackTarget>().attackable = false;
            foreach (var block in playerBlocks)
            {
                if (block.GetComponent<CardBlock>().monsterCard != null)
                {
                    block.GetComponent<CardBlock>().monsterCard.GetComponent<AttackTarget>().attackable = true;
                }
            }
            foreach (var block in enemyBlocks)
            {
                if (block.GetComponent<CardBlock>().monsterCard != null)
                {
                    block.GetComponent<CardBlock>().monsterCard.GetComponent<AttackTarget>().attackable = false;
                    block.GetComponent<CardBlock>().monsterCard.GetComponent<BattleCard>().hasAttacked = false;
                }
            }
        }
        else if (currentPhase == GamePhase.enemyAction)
        {
            currentPhase = GamePhase.playerDraw;
            playerSummonCount = maxPlayerSummonCount;

            playerIcon.GetComponent<AttackTarget>().attackable = false;
            enemyIcon.GetComponent<AttackTarget>().attackable = true;
            foreach (var block in playerBlocks)
            {
                if (block.GetComponent<CardBlock>().monsterCard != null)
                {
                    block.GetComponent<CardBlock>().monsterCard.GetComponent<AttackTarget>().attackable = false;
                    block.GetComponent<CardBlock>().monsterCard.GetComponent<BattleCard>().hasAttacked = false;
                }
            }
            foreach (var block in enemyBlocks)
            {
                if (block.GetComponent<CardBlock>().monsterCard != null)
                {
                    block.GetComponent<CardBlock>().monsterCard.GetComponent<AttackTarget>().attackable = true;
                }
            }
        }
        phaseChangeEvent.Invoke();
    }

    /// <summary>
    /// 统一使用卡牌入口（法术/装备/怪兽卡）
    /// </summary>
    /// <param name="_player">使用者编号（0=玩家，1=AI）</param>
    /// <param name="_cardObject">要使用的卡牌物体</param>
    public void UseCard(int _player, GameObject _cardObject)
    {
        CardDisplay cardDisplay = _cardObject.GetComponent<CardDisplay>();
        if (cardDisplay == null || cardDisplay.card == null) return;

        Card usedCard = cardDisplay.card;
        Transform handTransform = _player == 0 ? playerHands.transform : enemyHands.transform;
        Transform discardTransform = _player == 0 ? playerDiscardPile : enemyDiscardPile;

        // 按卡牌类型处理逻辑
        switch (usedCard.cardType)
        {
            case CardType.Monster: // 怪兽卡，走原有召唤逻辑
                Debug.Log("怪兽卡请使用召唤功能");
                return;
            case CardType.Spell: // 法术卡，走攻击判定流程
                HandleSpellCard(_player, usedCard as SpellCard, _cardObject);
                break;
            case CardType.Item: // 装备卡，修改怪兽数值
                HandleItemCard(_player, usedCard as ItemCard, _cardObject);
                break;
        }

        // 处理弃牌（装备卡除外）
        if (usedCard.cardType != CardType.Item || (usedCard as ItemCard).type != "装备")
        {
            _cardObject.transform.SetParent(discardTransform);
            _cardObject.SetActive(false);
        }

        // 刷新回合状态
        if (_player == 0)
        {
            currentPhase = GamePhase.playerAction;
        }
        else
        {
            currentPhase = GamePhase.enemyAction;
        }
        phaseChangeEvent.Invoke();
    }

    /// <summary>
    /// 处理法术卡逻辑（和怪兽攻击走一样的判定流程）
    /// </summary>
    private void HandleSpellCard(int _player, SpellCard _spellCard, GameObject _cardObject)
    {
        Debug.Log($"使用法术卡：{_spellCard.cardName}，效果：{_spellCard.effect}");

        // 1. 确定攻击目标
        GameObject target = null;
        if (_spellCard.targetType == TargetType.EnemyPlayer)
        {
            // 目标为敌方玩家，直接造成伤害
            if (_player == 0)
            {
                enemyHealthPoint -= _spellCard.damageValue;
                enemyHealthPoint = Mathf.Max(enemyHealthPoint, 0);
                Debug.Log($"对敌方玩家造成{_spellCard.damageValue}点伤害，剩余血量：{enemyHealthPoint}");
                RefreshHpUI();
                CheckGameOver();
            }
            else
            {
                playerHealthPoint -= _spellCard.damageValue;
                playerHealthPoint = Mathf.Max(playerHealthPoint, 0);
                Debug.Log($"对我方玩家造成{_spellCard.damageValue}点伤害，剩余血量：{playerHealthPoint}");
                RefreshHpUI();
                CheckGameOver();
            }
            // 生成箭头（从手牌指向敌方玩家）
            GenerateAttackArrow(_cardObject.transform.position, _player == 0 ? enemyIcon.transform.position : playerIcon.transform.position);
            return;
        }
        else if (_spellCard.targetType == TargetType.EnemyMonster)
        {
            // 目标为敌方怪兽，自动选择第一个敌方怪兽
            GameObject[] targetBlocks = _player == 0 ? enemyBlocks : playerBlocks;
            foreach (var block in targetBlocks)
            {
                GameObject monsterCard = block.GetComponent<CardBlock>().monsterCard;
                if (monsterCard != null)
                {
                    target = monsterCard;
                    break;
                }
            }
        }
        else if (_spellCard.targetType == TargetType.AllyMonster)
        {
            // 目标为我方怪兽，自动选择第一个我方怪兽
            GameObject[] allyBlocks = _player == 0 ? playerBlocks : enemyBlocks;
            foreach (var block in allyBlocks)
            {
                GameObject monsterCard = block.GetComponent<CardBlock>().monsterCard;
                if (monsterCard != null)
                {
                    target = monsterCard;
                    break;
                }
            }
        }

        // 2. 对目标怪兽造成伤害（复用原有攻击逻辑）
        if (target != null)
        {
            MonsterCard targetMonster = target.GetComponent<CardDisplay>().card as MonsterCard;
            if (targetMonster != null)
            {
                targetMonster.GetDamage(_spellCard.damageValue);
                Debug.Log($"对{targetMonster.cardName}造成{_spellCard.damageValue}点伤害，剩余血量：{targetMonster.healthPoint}");
                // 生成箭头（从手牌指向目标怪兽）
                GenerateAttackArrow(_cardObject.transform.position, target.transform.position);
                // 刷新怪兽血量显示
                target.GetComponent<CardDisplay>().ShowCard();
                // 检查怪兽是否被摧毁
                if (targetMonster.healthPoint <= 0)
                {
                    Destroy(target);
                }
            }
        }
    }

    /// <summary>
    /// 处理装备卡逻辑（修改怪兽数值）
    /// </summary>
    private void HandleItemCard(int _player, ItemCard _itemCard, GameObject _cardObject)
    {
        Debug.Log($"使用装备卡：{_itemCard.cardName}，效果：{_itemCard.effect}");

        // 1. 找到目标我方怪兽
        GameObject targetMonster = null;
        GameObject[] allyBlocks = _player == 0 ? playerBlocks : enemyBlocks;
        foreach (var block in allyBlocks)
        {
            GameObject monsterCard = block.GetComponent<CardBlock>().monsterCard;
            if (monsterCard != null)
            {
                targetMonster = monsterCard;
                break;
            }
        }

        if (targetMonster == null)
        {
            Debug.Log("没有可装备的怪兽，装备卡进入弃牌堆");
            _cardObject.transform.SetParent(_player == 0 ? playerDiscardPile : enemyDiscardPile);
            _cardObject.SetActive(false);
            return;
        }

        // 2. 给怪兽添加装备数值
        MonsterCard monster = targetMonster.GetComponent<CardDisplay>().card as MonsterCard;
        if (monster != null)
        {
            monster.equipAttackBuff += _itemCard.attackBuff;
            monster.equipHealthBuff += _itemCard.healthBuff;
            monster.equippedItems.Add(_itemCard);
            // 新增：使用药水立刻恢复对应数值的当前生命值，不超过上限
            monster.healthPoint = Mathf.Min(
                monster.healthPoint + _itemCard.healthBuff,
                monster.GetFinalHealthMax()
            );

            Debug.Log($"给{monster.cardName}装备{_itemCard.cardName}，攻击力+{_itemCard.attackBuff}，生命值+{_itemCard.healthBuff}");
            Debug.Log($"最终攻击力：{monster.GetFinalAttack()}，最终生命上限：{monster.GetFinalHealthMax()}");

            // 生成箭头（从手牌指向装备的怪兽）
            GenerateAttackArrow(_cardObject.transform.position, targetMonster.transform.position);
            // 刷新怪兽UI显示
            targetMonster.GetComponent<CardDisplay>().ShowCard();
            // 装备卡隐藏，不进入弃牌堆
            _cardObject.SetActive(false);
        }
    }

    /// <summary>
    /// 生成攻击箭头（和怪兽攻击用同一个逻辑）
    /// </summary>
    private void GenerateAttackArrow(Vector3 startPos, Vector3 endPos)
    {
        if (arrowPrefab == null) return;

        // 实例化箭头
        GameObject arrow = Instantiate(arrowPrefab, startPos, Quaternion.identity);
        // 计算箭头方向
        Vector3 direction = endPos - startPos;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        arrow.transform.rotation = Quaternion.Euler(0, 0, angle);
        // 拉伸箭头长度
        float distance = Vector3.Distance(startPos, endPos);
        arrow.transform.localScale = new Vector3(distance / 100f, 1f, 1f);
        // 箭头自动销毁
        Destroy(arrow, 0.5f);
    }

    public void SummonRequst(Vector2 _startPoint, int _player, GameObject _monster) // 召唤请求，点击手牌时触发
    {
        if (arrow != null)
        {
            Destroy(arrow);
        }
        if (_player == 0 && playerSummonCount >= 1)
        {
            arrow = GameObject.Instantiate(arrowPrefab, canvas);
            arrow.GetComponent<ArrowFollow>().SetStartPoint(_startPoint);
            foreach (var block in playerBlocks)
            {
                if (block.GetComponent<CardBlock>().monsterCard == null)
                {
                    block.GetComponent<CardBlock>().SetSummon();
                }
            }
            waitingMonster = _monster;
            waitingID = _player;
        }
        else if (_player == 1 && enemySummonCount >= 1)
        {
            arrow = GameObject.Instantiate(arrowPrefab, canvas);
            arrow.GetComponent<ArrowFollow>().SetStartPoint(_startPoint);
            foreach (var block in enemyBlocks)
            {
                if (block.GetComponent<CardBlock>().monsterCard == null)
                {
                    block.GetComponent<CardBlock>().SetSummon();
                }
            }
            waitingMonster = _monster;
            waitingID = _player;
        }
    }
    public void SummonCofirm(Transform _block) // 召唤确认，点击格子时触发
    {
        Summon(waitingMonster, waitingID, _block);
        waitingMonster = null;
    }

    /// <summary>
    /// 召唤怪兽
    /// </summary>
    /// <param name="_monster">要召唤的怪兽卡物体</param>
    /// <param name="_id">召唤者编号</param>
    /// <param name="_block">要召唤到的格子节点</param>
    public void Summon(GameObject _monster, int _id, Transform _block)
    {
        // ========== 入口空值校验 ==========
        if (_monster == null || _block == null)
        {
            Debug.LogWarning("召唤失败：怪兽或格子对象为空/已销毁");
            if (arrow != null) Destroy(arrow);
            return;
        }

        CardBlock blockScript = _block.GetComponent<CardBlock>();
        if (blockScript == null)
        {
            Debug.LogWarning("召唤失败：格子缺少CardBlock组件");
            return;
        }
        _monster.transform.SetParent(_block);
        _monster.GetComponent<CardDisplay>().ShowCard();
        _block.GetComponent<CardBlock>().monsterCard = _monster;
        //_block.GetComponent<CardBlock>().hasMonster = true;
        _monster.transform.localPosition = Vector3.zero;
        if (_id == 0)
        {
            _monster.GetComponent<BattleCard>().cardState = CardState.inPlayerBlock;
            playerSummonCount--;
            foreach (var block in playerBlocks)
            {
                block.GetComponent<CardBlock>().CloseAll();
            }
        }
        else if (_id == 1)
        {
            _monster.GetComponent<BattleCard>().cardState = CardState.inEnemyBlock;
            enemySummonCount--;
            foreach (var block in enemyBlocks)
            {
                block.GetComponent<CardBlock>().CloseAll();
            }
        }

        if (arrow != null)
        {
            Destroy(arrow);
        }
    }


    public void AttackRequst(Vector2 _startPoint, int _player, GameObject _monster)
    {
        if (arrow == null)
        {
            arrow = GameObject.Instantiate(attackPrefab, canvas);
        }

        arrow.GetComponent<ArrowFollow>().SetStartPoint(_startPoint);

        // 直接攻击条件
        bool strightAttack = true;
        if (_player == 0)
        {
            foreach (var block in enemyBlocks)
            {
                if (block.GetComponent<CardBlock>().monsterCard != null)
                {
                    block.GetComponent<CardBlock>().SetAttack();
                    strightAttack = false;
                }
            }

             // 新增：自动设置敌方头像Tag
            enemyIcon.tag = strightAttack ? "Attackable" : "Unattackable";

            //if (strightAttack)
            //{
            // 可以直接攻击对手玩家-+
            //}
        }
        if (_player == 1)
        {
            foreach (var block in playerBlocks)
            {
                if (block.GetComponent<CardBlock>().monsterCard != null)
                {
                    block.GetComponent<CardBlock>().SetAttack();
                    strightAttack = false;
                }
            }
            // 新增：自动设置玩家头像Tag
            playerIcon.tag = strightAttack ? "Attackable" : "Unattackable";

            // if (strightAttack)
            //{
            // 可以直接攻击对手玩家

            // }
        }

        attackingMonster = _monster;
        attackingID = _player;
        // 🔥【仅新增这1行】把直接攻击状态存起来
        canStraightAttack = strightAttack;
    }
    public void AttackCofirm(GameObject _target)
    {
        Attack(attackingMonster, attackingID, _target);
        attackingMonster = null;
    }
    public void Attack(GameObject _monster, int _id, GameObject _target)
    {
        // 1. 攻击怪兽自身为空，直接拦截
        if (_monster == null)
        {
            Debug.LogWarning("攻击怪兽为空，跳过本次攻击");
            if (arrow != null) Destroy(arrow);
            CloseAllBlockState();
            return;
        }

        if (arrow != null)
        {
            Destroy(arrow);
        }
        _monster.GetComponent<BattleCard>().hasAttacked = true;
        Debug.Log("攻击成立");

        // 2. 目标为空 → 直接攻击玩家（原有逻辑）
        if (_target == null)
        {
            MonsterCard attacker = _monster.GetComponent<CardDisplay>()?.card as MonsterCard;
            if (attacker == null)
            {
                Debug.LogWarning("攻击方卡牌数据为空，攻击无效");
                CloseAllBlockState();
                return;
            }

            if (_id == 0)
            {
                enemyHealthPoint -= attacker.GetFinalAttack();
                enemyHealthPoint = Mathf.Max(enemyHealthPoint, 0);
                Debug.Log("敌方玩家受到伤害！剩余血量：" + enemyHealthPoint);
                RefreshHpUI();
                CheckGameOver();
            }
            else
            {
                playerHealthPoint -= attacker.GetFinalAttack();
                playerHealthPoint = Mathf.Max(playerHealthPoint, 0);
                Debug.Log("我方玩家受到伤害！剩余血量：" + playerHealthPoint);
                RefreshHpUI();
                CheckGameOver();
            }
            CloseAllBlockState();
            return;
        }

        // 3. 目标是玩家头像（不可攻击的Tag）→ 拦截并返回
        if (_target.CompareTag("Unattackable"))
        {
            Debug.Log("场上有怪兽，禁止直接攻击玩家");
            CloseAllBlockState();
            return;
        }

        // 4. 目标是怪兽 → 校验组件和数据，再结算伤害
        var attackMonster = _monster.GetComponent<CardDisplay>()?.card as MonsterCard;
        var targetMonster = _target.GetComponent<CardDisplay>()?.card as MonsterCard;

        // 任意一方卡牌数据为空，都不执行伤害
        if (attackMonster == null || targetMonster == null)
        {
            Debug.LogWarning("攻击方或目标方卡牌数据异常，跳过伤害结算");
            CloseAllBlockState();
            return;
        }

        targetMonster.GetDamage(attackMonster.GetFinalAttack());
        if (targetMonster.healthPoint > 0)
        {
            _target.GetComponent<CardDisplay>().ShowCard();
        }
        else
        {
            // 先清空格子引用，再销毁对象，避免残留引用
            _target.GetComponentInParent<CardBlock>().monsterCard = null;
            Destroy(_target);
        }

        CloseAllBlockState();
    }

    // 封装关闭所有格子状态的方法，避免重复代码
    private void CloseAllBlockState()
    {
        foreach (var block in playerBlocks)
        {
            block.GetComponent<CardBlock>().CloseAll();
        }
        foreach (var block in enemyBlocks)
        {
            block.GetComponent<CardBlock>().CloseAll();
        }
    }

    public virtual void GameStart() // 游戏开始，读取卡组，抽五张手牌
    {
        playerSummonCount = maxPlayerSummonCount;
        enemySummonCount = maxEnemySummonCount;
        CardDate = playerData.GetComponent<CardData>();

        currentPhase = GamePhase.gameStart;
        ReadDeck();
        //Debug.Log(currentPhase);
        DrawCard(0, 5);
        DrawCard(1, 5);
        currentPhase = GamePhase.playerDraw;
        //Debug.Log(currentPhase);
    }

    public void ReadDeck() // 从数据中读取卡组
    {
        PlayerDataManager pdm = playerData.GetComponent<PlayerDataManager>();
        for (int i = 0; i < pdm.playerDeck.Length; i++)
        {
            if (pdm.playerDeck[i] != 0)
            {
                int counter = pdm.playerDeck[i];
                for (int j = 0; j < counter; j++)
                {
                    playerDeckList.Add(CardDate.CopyCard(i));
                }
            }
        }
        // 读取敌人的卡组
        PlayerDataManager edm = enemyData.GetComponent<PlayerDataManager>();
        for (int i = 0; i < edm.playerDeck.Length; i++)
        {
            if (edm.playerDeck[i] != 0)
            {
                int counter = edm.playerDeck[i];
                for (int j = 0; j < counter; j++)
                {
                    enemyDeckList.Add(CardDate.CopyCard(i));
                }
            }
        }
        ShuffletDeck(0);
        ShuffletDeck(1);
        foreach (var item in playerDeckList)
        {
            //Debug.Log(item.cardName);
        }
    }

    public void ShuffletDeck(int _player) // 将卡组洗牌，输入玩家编号，0代表player，1代表Enemy
    {
        // 洗牌算法的基本思路是遍历整个卡组，对于每一张牌，都和随机的一张牌调换位置。
        switch (_player)
        {
            case 0:
                for (int i = 0; i < playerDeckList.Count; i++)
                {
                    int rad = Random.Range(0, playerDeckList.Count);
                    Card temp = playerDeckList[i];
                    playerDeckList[i] = playerDeckList[rad];
                    playerDeckList[rad] = temp;
                }
                break;
            case 1:
                for (int i = 0; i < enemyDeckList.Count; i++)
                {
                    int rad = Random.Range(0, enemyDeckList.Count);
                    Card temp = enemyDeckList[i];
                    enemyDeckList[i] = enemyDeckList[rad];
                    enemyDeckList[rad] = temp;
                }
                break;
        }
    }
}
