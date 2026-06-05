using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour
{
    public GameObject cardLabel;
    public GameObject panel;

    public GameObject playerData;

    private PlayerDataManager pdm;
    private CardData cardData;

    public Text numberText;

    private List<GameObject> cardPool = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        pdm = playerData.GetComponent<PlayerDataManager>();
        cardData = playerData.GetComponent<CardData>();
        UpdateDeck();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateDeck()
    {
        numberText.text = "DeckCards Number:" + GetTotalDeckCount();
        ClearPool();
        for (int i = 0; i < pdm.playerDeck.Length; i++)
        {
            if (pdm.playerDeck[i] != 0)
            {
                GameObject newCard = GameObject.Instantiate(cardLabel, panel.transform);
                cardPool.Add(newCard);
                newCard.GetComponent<CardDisplay>().card = cardData.CardList[i];
                newCard.GetComponent<UICardCounter>().counter.text = pdm.playerDeck[i].ToString();
            }
        }
    }

    public void ClearPool()
    {
        foreach (var card in cardPool)
        {
            Destroy(card);
        }
        cardPool.Clear();
    }

    public void OnClickSave()
    {
        pdm.SavePlayerData();
    }
    // 计算卡组总卡牌数（仅新增，不影响原有功能）
    public int GetTotalDeckCount()
    {
        int total = 0;
        for (int i = 0; i < pdm.playerDeck.Length; i++)
        {
            total += pdm.playerDeck[i];
        }
        return total;
    }
}
