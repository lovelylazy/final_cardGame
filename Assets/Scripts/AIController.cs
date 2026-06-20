using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    /// <summary>
    /// 行动间隔时间
    /// </summary>
    public float actionTimeStart;
    private float actionTime;
    bool haveBlock;
    bool haveMonster;
    GameObject currentMonster;
    Transform currentBlock;

    // Start is called before the first frame update
    void Start()
    {
        actionTime = actionTimeStart;
    }

    // Update is called once per frame
    void Update()
    {
        // 修复：统一用BattleManager判断阶段，修正笔误
        if (BattleManager.Instance.currentPhase == GamePhase.enemyDraw
            || BattleManager.Instance.currentPhase == GamePhase.enemyAction)
        {
            if (actionTime <= 0)
            {
                NextStep();
                actionTime = actionTimeStart;
            }
            else
            {
                actionTime -= Time.deltaTime;
            }
        }
    }

    private void NextStep()
    {
        // 每次执行先重置所有标记和引用
        haveBlock = false;
        haveMonster = false;
        currentMonster = null;
        currentBlock = null;

        // 1. 抽卡阶段
        if (BattleManager.Instance.currentPhase == GamePhase.enemyDraw)
        {
            BattleManager.Instance.DrawCard(1, 1, true);
            return;
        }

        // 2. 行动阶段
        if (BattleManager.Instance.currentPhase == GamePhase.enemyAction)
        {
            // 优先尝试召唤
            if (BattleManager.Instance.enemySummonCount >= 1)
            {
                // 找空格子
                foreach (var block in BattleManager.Instance.enemyBlocks)
                {
                    if (block.GetComponent<CardBlock>().monsterCard == null)
                    {
                        haveBlock = true;
                        currentBlock = block.transform;
                        break;
                    }
                }
                // 找手牌里的怪兽卡
                foreach (Transform child in BattleManager.Instance.enemyHands.transform)
                {
                    CardDisplay display = child.GetComponent<CardDisplay>();
                    if (display != null && display.card != null && display.card is MonsterCard)
                    {
                        haveMonster = true;
                        currentMonster = child.gameObject;
                        break;
                    }
                }

                // 能召唤就召唤
                if (haveBlock && haveMonster && currentMonster != null && currentBlock != null)
                {
                    Debug.Log("AI开始召唤");
                    BattleManager.Instance.Summon(currentMonster, 1, currentBlock);
                    return; // 召唤完本步结束，下一步再判断
                }
                else
                {
                    // 修复：召唤不了 → 直接清空召唤次数，强制进入攻击阶段，避免卡死
                    Debug.Log("AI无法召唤，跳过召唤阶段");
                    BattleManager.Instance.enemySummonCount = 0;
                    return;
                }
            }
            // 召唤次数用完 → 执行攻击
            else
            {
                bool hasAttacked = false;

                foreach (var block in BattleManager.Instance.enemyBlocks)
                {
                    CardBlock blockScript = block.GetComponent<CardBlock>();
                    // 找到未攻击的怪兽
                    if (blockScript.monsterCard != null
                        && !blockScript.monsterCard.GetComponent<BattleCard>().hasAttacked)
                    {
                        currentMonster = blockScript.monsterCard;
                        GameObject attackTarget = null;

                        // 搜索玩家场上的怪兽
                        foreach (var playerBlock in BattleManager.Instance.playerBlocks)
                        {
                            if (playerBlock.GetComponent<CardBlock>().monsterCard != null)
                            {
                                attackTarget = playerBlock.GetComponent<CardBlock>().monsterCard;
                                break;
                            }
                        }

                        Debug.Log(attackTarget == null ? "AI直接攻击玩家角色！" : "AI攻击玩家怪兽！");
                        BattleManager.Instance.Attack(currentMonster, 1, attackTarget);
                        currentMonster.GetComponent<BattleCard>().hasAttacked = true;
                        hasAttacked = true;
                        break; // 每次只攻击一只，下一轮再继续
                    }
                }

                // 修复：没有可攻击的怪兽 / 攻击完所有怪兽 → 统一结束回合
                // 只有当本次没攻击时，才结束回合，避免攻击一次就结束
                if (!hasAttacked)
                {
                    Debug.Log("AI回合结束");
                    BattleManager.Instance.TurnEnd();
                }
            }
        }
    }
}