using System.Collections.Generic;
using UnityEngine;

public class CardDatabase : MonoBehaviour
{
    public static List<Card> GetAllCards()
    {
        return new List<Card>()
        {
            new Card("GBase Attack!", "Example of effect here", 1, new DamageEffect(2), UseCase.SLOT)
        };
    }

    public static Card GetRandomCard()
    {
        var cards = GetAllCards();
        return cards[Random.Range(0, cards.Count)];
    }
}
