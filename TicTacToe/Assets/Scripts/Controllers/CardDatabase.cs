using System.Collections.Generic;
using UnityEngine;

public class CardDatabase : MonoBehaviour
{
    //list containing all available cards in the game
    public static List<Card> GetAllCards()
    {
        return new List<Card>()
        {
            new BasicDamageCard(),
            new BasicSuportCard(),
            //etc...
        };
    }
    
    /// <summary>
    ///returns a random card from the database.
    /// </summary>
    public static Card GetRandomCard()
    {
        var cards = GetAllCards();
        return cards[Random.Range(0, cards.Count)];
    }
}