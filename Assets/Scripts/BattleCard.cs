using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// 原有枚举 全部保留
public enum CardOwner
{
    Player = 0,
    Enemy = 1
}

public enum CardState //位置和所属状态
{
    inPlayerHand,
    inPlayerBlock,
    inEnemyHand,
    inEnemyBlock
}

// 🔥 新增：卡牌类型枚举（必须加，用于区分怪兽/法术/装备）


public class BattleCard : MonoBehaviour, IPointerDownHandler
{
    // 原有组件缓存 全部保留
    private CardDisplay _cardDisplay;
    private BattleManager _battleManager;

    public CardState cardState = CardState.inPlayerHand;
    public bool hasAttacked;

    void Start()
    {
        _cardDisplay = GetComponent<CardDisplay>();
        _battleManager = BattleManager.Instance;

        if (_cardDisplay == null)
            Debug.LogError($"[{gameObject.name}] 缺少CardDisplay组件！");
        if (_battleManager == null)
            Debug.LogError("BattleManager单例未初始化！");
    }

    // 核心：融合原有逻辑 + 新增UseCard使用卡牌逻辑
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_battleManager == null || _cardDisplay == null) return;
        Card currentCard = _cardDisplay.card;
        if (currentCard == null) return;

        // ==============================================
        // 原有逻辑：战场怪兽卡 → 攻击（完全保留）
        // ==============================================
        if (cardState == CardState.inPlayerBlock || cardState == CardState.inEnemyBlock)
        {
            if (!(currentCard is MonsterCard)) return;
            MonsterCard monster = currentCard as MonsterCard;

            if (cardState == CardState.inPlayerBlock)
            {
                if (_battleManager.currentPhase == GamePhase.playerAction && !hasAttacked)
                {
                    _battleManager.AttackRequst(transform.position, (int)CardOwner.Player, gameObject);
                    hasAttacked = true;
                }
            }
            else if (cardState == CardState.inEnemyBlock)
            {
                if (_battleManager.currentPhase == GamePhase.enemyAction && !hasAttacked)
                {
                    _battleManager.AttackRequst(transform.position, (int)CardOwner.Enemy, gameObject);
                    hasAttacked = true;
                }
            }
            return;
        }

        // ==============================================
        // 新增逻辑：手牌卡牌 → 区分怪兽/法术/装备
        // ==============================================
        if (cardState == CardState.inPlayerHand && _battleManager.currentPhase == GamePhase.playerAction)
        {
            // 怪兽卡 → 原有召唤逻辑
            if (currentCard.cardType == CardType.Monster)
            {
                _battleManager.SummonRequst(transform.position, (int)CardOwner.Player, gameObject);
            }
            // 法术/装备卡 → 调用UseCard使用
            else if (currentCard.cardType == CardType.Spell || currentCard.cardType == CardType.Item)
            {
                _battleManager.UseCard((int)CardOwner.Player, gameObject);
            }
        }

        // AI手牌逻辑（原有保留，AI自动处理，玩家不可点击）
        else if (cardState == CardState.inEnemyHand && _battleManager.currentPhase == GamePhase.enemyAction)
        {
            if (currentCard.cardType == CardType.Monster)
            {
                _battleManager.SummonRequst(transform.position, (int)CardOwner.Enemy, gameObject);
            }
            else if (currentCard.cardType == CardType.Spell || currentCard.cardType == CardType.Item)
            {
                _battleManager.UseCard((int)CardOwner.Enemy, gameObject);
            }
        }
    }

    // 原有方法 完全保留
    public void ResetAttackState()
    {
        hasAttacked = false;
    }
}