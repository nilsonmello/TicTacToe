using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TableManager))]
public class GameManager : MonoBehaviour
{
    private TableManager tableManager;

    public List<System.Type> activeSlotTypes = new List<System.Type>
    {
        typeof(ToxicSlot),
        typeof(BrightSlot)
    };
    public static GameManager Instance { get; private set; }
    public Slot defaultSlot = new DefaultSlot();
    private GameState gameState = GameState.INITIALIZING;

    public List<Sprite> playerSprites = new List<Sprite>();
    private List<Player> players = new List<Player>();

    private int currentPlayer = 0;

    public Player GetCurrentPlayer()
    {
        return players[currentPlayer];
    }
    public void ChangeTurn()
    {
        gameState = GameState.CHANGING_TURN;   
    }

    public void SwitchPlayer()
    {
        currentPlayer = (currentPlayer + 1) % players.Count;
        Debug.Log("Switched to player: " + GetCurrentPlayer().GetName());
    }

    public Player GetPlayer(int playerIndex)
    {
        return players[playerIndex];
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        tableManager = GetComponent<TableManager>();
        for (int i = 0; i < playerSprites.Count; i++)
        {
            players.Add(new Player($"Player {i + 1}", i + 1, playerSprites[i]));
        }
    }


    void Update()
    {
        Debug.Log($"Current Game State: {gameState}");
        switch (gameState)
        {
            case GameState.INITIALIZING:
                tableManager.Initialize();
                gameState = GameState.PLAYING;
                break;
            case GameState.START_TURN:
                GetCurrentPlayer().AddEnergyPoints();
                gameState = GameState.PLAYING;
                break;
            case GameState.CHANGING_TURN:
            string winner = tableManager.CheckVictory().GetWinner()?.GetName();
                Debug.Log($"State: {gameState}, CurrentPlayer: {GetCurrentPlayer().GetName()}");
                Debug.Log(winner);
                if (winner != null)
                {
                    gameState = GameState.GAME_OVER;
                    Debug.Log($"Game Over! Winner: {winner}");
                    return;
                }
                SwitchPlayer();
                gameState = GameState.START_TURN;
                break;
            case GameState.PLAYING:
                break;
            case GameState.GAME_OVER:
                Debug.Log("Game Over! Resetting game state.");
                // gameState = GameState.RESTARTING;
                break;
            case GameState.RESTARTING:
                gameState = GameState.INITIALIZING;
                break;
            default:
                Debug.LogError("Unknown game state: " + gameState);
                break;
        }
    }
}
