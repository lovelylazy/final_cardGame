// 卡牌类 基础类
using System.Collections.Generic;

// 卡牌类型枚举（建议单独文件：CardType.cs）
public enum CardType
{
    Monster,   // 怪兽卡
    Item,      // 道具卡
    Spell      // 法术卡
}
public class Card
{
    //卡牌编号
    public int id;
    public string cardName;
    // 新增：卡牌类型标识（核心解决报错的属性）
    public CardType cardType;
    public Card(int _id, string _cardName)
    {
        this.id = _id;
        this.cardName = _cardName;
    }

}

// 怪兽卡类
public class MonsterCard : Card
{

    // 怪兽攻击力
    public int attack;
    /// 生命值
    public int healthPoint;
    // 生命值上限
    public int healthPointMax;
    // 怪兽等级
    public int rank;
    // 怪兽属性
    public string type;
    // 怪兽效果
    public string effect;

    // 🔥 新增：装备数值加成
    public int equipAttackBuff = 0;    // 装备攻击力加成
    public int equipHealthBuff = 0;    // 装备生命值加成
    public List<ItemCard> equippedItems = new List<ItemCard>(); // 已装备的物品

    public MonsterCard(int _id, string _cardName, int _attack, int _healthPoint, string _type) : base(_id, _cardName)
    {
        this.attack = _attack;
        this.healthPointMax = _healthPoint;
        this.healthPoint = _healthPoint;
        this.type = _type;
        cardType = CardType.Monster;
    }
    // 🔥 新增：获取最终攻击力（基础+装备）
    public int GetFinalAttack()
    {
        return attack + equipAttackBuff;
    }

    // 🔥 新增：获取最终生命值上限（基础+装备）
    public int GetFinalHealthMax()
    {
        return healthPointMax + equipHealthBuff;
    }


    public void GetDamage(int _damagePoint)
    {
        if (healthPoint > _damagePoint)
        {
            healthPoint -= _damagePoint;
        }
        else
        {
            healthPoint = 0;
            damageDestroy();
        }
    }

    public void damageDestroy()
    {
        // do some thing...
        // 🔥 新增：怪兽被摧毁时，清空所有装备加成
        equipAttackBuff = 0;
        equipHealthBuff = 0;
        equippedItems.Clear();
    }
}

// 🔥 新增：目标类型枚举
public enum TargetType
{
    EnemyPlayer,    // 敌方玩家
    EnemyMonster,   // 敌方怪兽
    AllyMonster     // 我方怪兽
}
// 法术卡类 继承自卡牌类
public class SpellCard : Card
{
    public int rank;
    public string type;
    public string effect;
    public int damageValue; // 🔥 新增：法术伤害值（从effect解析）
    public TargetType targetType; // 🔥 新增：目标类型（敌方玩家/敌方怪兽/我方怪兽）

    public SpellCard(int _id, string _cardName, int _rank, string _type, string _effect) : base(_id, _cardName)
    {
        this.rank = _rank;
        this.type = _type;
        this.effect = _effect;
        cardType = CardType.Spell;
        if (effect.Contains("造成") && effect.Contains("点伤害"))
        {
            damageValue = int.Parse(System.Text.RegularExpressions.Regex.Match(effect, @"\d+").Value);
        }
        // 自动解析目标类型
        if (effect.Contains("敌方玩家")) targetType = TargetType.EnemyPlayer;
        else if (effect.Contains("敌方怪兽")) targetType = TargetType.EnemyMonster;
        else if (effect.Contains("我方怪兽")) targetType = TargetType.AllyMonster;
        else targetType = TargetType.EnemyPlayer; // 默认敌方玩家
    }
}

public class ItemCard : Card
{
    public string effect;
    public string type;
    public int attackBuff; // 🔥 新增：攻击力加成
    public int healthBuff; // 🔥 新增：生命值加成
    public TargetType targetType; // 🔥 新增：目标类型（我方怪兽）
    public ItemCard(int _id, string _cardName, string _type, string _effect) : base(_id, _cardName)
    {
        this.type = _type;
        this.effect = _effect;
        cardType = CardType.Item;
        // 自动解析加成数值
        if (effect.Contains("增加") && effect.Contains("点攻击力"))
        {
            attackBuff = int.Parse(System.Text.RegularExpressions.Regex.Match(effect, @"\d+").Value);
        }
        if (effect.Contains("恢复") && effect.Contains("点生命值"))
        {
            healthBuff = int.Parse(System.Text.RegularExpressions.Regex.Match(effect, @"\d+").Value);
        }
        // 装备卡默认目标为我方怪兽
        targetType = TargetType.AllyMonster;
    }
    
}