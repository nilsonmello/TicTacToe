using System.Collections.Generic;
using UnityEngine;

public class CardDatabase : MonoBehaviour
{
    public static List<Card> GetAllCards()
    {
        return new List<Card>()
        {
            new BasicDamageCard(),
            //etc...
        };
    }

    public static Card GetRandomCard()
    {
        var cards = GetAllCards();
        return cards[Random.Range(0, cards.Count)];
    }
}
