using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    public float actionTimeStart = 1f;
    private float actionTime;
    private bool hasActed = false;

    void Start()
    {
        actionTime = actionTimeStart;
    }

    void Update()
    {
        if (BattleManager.Instance.currentPhase == GamePhase.enemyAction && !hasActed)
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
        hasActed = true;
        var bm = BattleManager.Instance;

        // 1. 优先召唤
        if (bm.enemySummonCount >= 1)
        {
            Transform emptyBlock = null;
            foreach (var block in bm.enemyBlocks)
            {
                if (block.GetComponent<CardBlock>().monsterCard == null)
                {
                    emptyBlock = block.transform;
                    break;
                }
            }

            GameObject monsterInHand = null;
            foreach (Transform child in bm.enemyHands.transform)
            {
                if (child.GetComponent<CardDisplay>().card is MonsterCard)
                {
                    monsterInHand = child.gameObject;
                    break;
                }
            }

            if (emptyBlock != null && monsterInHand != null)
            {
                bm.Summon(monsterInHand, 1, emptyBlock);
                hasActed = false;
                return;
            }
        }

        // 2. 再攻击
        GameObject myMonster = null;
        foreach (var block in bm.enemyBlocks)
        {
            var card = block.GetComponent<CardBlock>().monsterCard;
            if (card != null && !card.GetComponent<BattleCard>().hasAttacked)
            {
                myMonster = card;
                break;
            }
        }

        GameObject playerMonster = null;
        foreach (var block in bm.playerBlocks)
        {
            var card = block.GetComponent<CardBlock>().monsterCard;
            if (card != null)
            {
                playerMonster = card;
                break;
            }
        }

        if (myMonster != null && playerMonster != null)
        {
            bm.Attack(myMonster, 1, playerMonster);
        }

        // 3. 最后结束回合（只调用1次）
        bm.TurnEnd();
        hasActed = false;
    }
}