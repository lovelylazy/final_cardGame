using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

/* 用于保存玩家的数据
可以从 csv 文件中读取玩家的资料，并且自动保存
可以保存玩家的金币数量，仓库中卡牌的数量，以及卡组。
【商店场景】玩家在商店中购买卡包的时候会先查看数据里的金币数是否充足。
【编辑卡组】卡组的数据格式与仓库类似
【对战场景】。。。
*/
public class PlayerDataManager : MonoBehaviour
{
    public TextAsset playerData;

    public Text coinsText;
    public Text cardsText;
    public Text DeckcardsText;

    public int totalCoins;

    private CardData cardData;
    public int[] playerCards;
    public int[] playerDeck;

    // Start is called before the first frame update
    void Start()
    {

    }
    private void Awake()
    {
        cardData = GetComponent<CardData>();
        cardData.LordCardList();
        LordPlayerData();
        LoadPlayerData();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LordPlayerData()
    {
        playerCards = new int[cardData.CardList.Count];
        playerDeck = new int[cardData.CardList.Count];
        string[] dataArray = playerData.text.Split('\n');
        foreach (var row in dataArray)
        {
            string[] rowArray = row.Split(',');
            if (rowArray[0] == "#")
            {
                continue;
            }
            else if (rowArray[0] == "coins")
            {
                totalCoins = int.Parse(rowArray[1]);
            }
            else if (rowArray[0] == "card")
            {
                int id = int.Parse(rowArray[1]);
                int num = int.Parse(rowArray[2]);
                playerCards[id] = num;
            }
            else if (rowArray[0] == "deck")
            {
                int id = int.Parse(rowArray[1]);
                int num = int.Parse(rowArray[2]);
                playerDeck[id] = num;
            }
        }
        updateText();
    }
    public void updateText()
    {
        coinsText.text = "Total Coins:" + totalCoins.ToString();
        cardsText.text = "Cards Number:" + Sum(playerCards).ToString();
        DeckcardsText.text = "DeckcardNumber:" + Sum(playerDeck).ToString();
    }

    public int Sum(int[] _cards)
    {
        int sum = 0;
        foreach (int item in _cards)
        {
            sum += item;
        }
        return sum;
    }

    public void SavePlayerData()
    {
        // 1. 使用持久化路径，打包后也能正常读写
        string directory = Path.Combine(Application.persistentDataPath, "Datas");
        string path = Path.Combine(directory, "playerdata.csv");

        try
        {
            // 2. 自动创建目录，避免文件夹不存在报错
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            List<string> datas = new List<string>();
            datas.Add("coins," + totalCoins.ToString());

            for (int i = 0; i < playerCards.Length; i++)
            {
                if (playerCards[i] != 0)
                {
                    datas.Add("card," + i.ToString() + "," + playerCards[i].ToString());
                }
            }

            for (int i = 0; i < playerDeck.Length; i++)
            {
                if (playerDeck[i] != 0)
                {
                    datas.Add("deck," + i.ToString() + "," + playerDeck[i].ToString());
                }
            }

            File.WriteAllLines(path, datas);
            Debug.Log($"✅ 存档保存成功！路径：{path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 存档保存失败：{e.Message}");
        }
    }
    public void LoadPlayerData()
    {
        string directory = Path.Combine(Application.persistentDataPath, "Datas");
        string path = Path.Combine(directory, "playerdata.csv");

        // 文件不存在就跳过，保持默认值
        if (!File.Exists(path))
        {
            Debug.Log("📂 无存档文件，使用默认数据");
            return;
        }

        try
        {
            // 先把数组全部初始化为0
            for (int i = 0; i < playerCards.Length; i++) playerCards[i] = 0;
            for (int i = 0; i < playerDeck.Length; i++) playerDeck[i] = 0;

            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line)) continue;

                string[] parts = line.Split(',');
                if (parts.Length < 2) continue;

                switch (parts[0])
                {
                    case "coins":
                        int.TryParse(parts[1], out totalCoins);
                        break;

                    case "card":
                        if (parts.Length >= 3)
                        {
                            int id = int.Parse(parts[1]);
                            int count = int.Parse(parts[2]);
                            // 防止索引越界
                            if (id >= 0 && id < playerCards.Length)
                                playerCards[id] = count;
                        }
                        break;

                    case "deck":
                        if (parts.Length >= 3)
                        {
                            int id = int.Parse(parts[1]);
                            int count = int.Parse(parts[2]);
                            if (id >= 0 && id < playerDeck.Length)
                                playerDeck[id] = count;
                        }
                        break;
                }
            }
            Debug.Log("✅ 存档加载成功");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 存档加载失败：{e.Message}");
        }
    }
}
