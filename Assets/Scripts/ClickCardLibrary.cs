using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickCardLibrary : MonoBehaviour
{
    public DeckManager deckManger;
    public LibraryManager LibraryManger;
    private PlayerDataManager pdm;

    private const int MAX_CARD_COUNT = 3;  // 单卡上限
    private const int MAX_DECK_TOTAL = 40; // 卡组上限
    // Start is called before the first frame update
    void Start()
    {
        LibraryManger = GameObject.Find("LibraryManager").GetComponent<LibraryManager>();
        deckManger = GameObject.Find("DeckManager").GetComponent<DeckManager>();
        pdm = GameObject.Find("PlayerData").GetComponent<PlayerDataManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnBuildClick()
    {
        int id = GetComponent<CardDisplay>().card.id;
        pdm.playerCards[id] += 1;
        LibraryManger.UpdateLibrary();
    }
    public void OnRemoveClick()
    {
        int id = GetComponent<CardDisplay>().card.id;
        if (pdm.playerDeck[id] < MAX_CARD_COUNT && deckManger.GetTotalDeckCount() <= MAX_DECK_TOTAL)
        {
            pdm.playerCards[id] -= 1;
            pdm.playerDeck[id] += 1;
            LibraryManger.UpdateLibrary();
            deckManger.UpdateDeck();
        }
        else
        {
            Debug.Log("卡组只能一类牌最多三张,卡组最多40张牌");
        }
    }
    public void OnBattleClick()
    {

    }
}
