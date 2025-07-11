using System.Collections.Generic;
using UnityEngine;
public class Player
{
    private string playerName;
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
} 