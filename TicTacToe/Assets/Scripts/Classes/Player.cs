using System.Collections.Generic;
using UnityEngine;
public class Player
{
    private string playerName;
    private int HealthPoints = 30;
    private int EnergyPoints = 50;
    private int maxEnergy = 50;
    private int playerID;
    private int playerScore;
    private Sprite playerIcon;

    private List<Card> playerCards = new List<Card>();
    private List<Card> playerDeck = new List<Card>();
    private List<Card> playerDiscardPile = new List<Card>();

    public Player(string name, int id, Sprite icon = null)
    {
        playerName = name;
        playerID = id;
        playerScore = 0;
        playerIcon = icon;
    }

    public string GetName()
    {
        return playerName;
    }

    public int GetID()
    {
        return playerID;
    }

    public int GetScore()
    {
        return playerScore;
    }

    public void AddScore(int score)
    {
        playerScore += score;
    }

    public void SetIcon(Sprite icon)
    {
        playerIcon = icon;
    }
    public Sprite GetIcon()
    {
        return playerIcon;
    }
    public void AddCardToHand(Card card)
    {
        playerCards.Add(card);
    }
    public void RemoveCardFromHand(Card card)
    {
        playerCards.Remove(card);
    }
    public List<Card> GetHand()
    {
        return playerCards;
    }
    public void AddCardToDeck(Card card)
    {
        playerDeck.Add(card);
    }
    public void RemoveCardFromDeck(Card card)
    {
        playerDeck.Remove(card);
    }
    public List<Card> GetDeck()
    {
        return playerDeck;
    }
    public void AddCardToDiscardPile(Card card)
    {
        playerDiscardPile.Add(card);
    }
    public void RemoveCardFromDiscardPile(Card card)
    {
        playerDiscardPile.Remove(card);
    }
    public List<Card> GetDiscardPile()
    {
        return playerDiscardPile;
    }
    public void AddEnergyPoints(int amount = 1)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Cannot add negative energy points.");
            Debug.Log($"{playerName} gained no energy points. Total: {EnergyPoints}");
            return;
        }
        if (EnergyPoints + amount > maxEnergy)
        {
            Debug.LogWarning($"Cannot exceed maximum energy points of {maxEnergy}.");
            Debug.Log($"{playerName} gained no energy points. Total: {EnergyPoints}");
            return;
        }
        EnergyPoints += amount;
        Debug.Log($"{playerName} gained {amount} energy points. Total: {EnergyPoints}");
    }
    public int GetEnergyPoints()
    {
        return EnergyPoints;
    }
    public void SetHealthPoints(int health)
    {
        HealthPoints = health;
        Debug.Log($"{playerName} health set to {HealthPoints}");
    }
    public int GetHealthPoints()
    {
        return HealthPoints;
    }
} 