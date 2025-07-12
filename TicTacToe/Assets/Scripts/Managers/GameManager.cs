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
    public Slot defaultSlot = new DefaultSlot();
    private GameState gameState = GameState.INITIALIZING;

    public Sprite player1Sprite;
    public Sprite player2Sprite;

    bool tested = false;
    void Awake()
    {
        tableManager = GetComponent<TableManager>();
    }

    void Start()
    {

    }

    void Update()
    {
        switch (gameState)
        {
            case GameState.INITIALIZING:
                tableManager.Initialize();
                gameState = GameState.PLAYING;
                break;
            case GameState.PLAYER_TURN:
                break;
            case GameState.CHANGING_TURN:
                break;
            case GameState.WAITING_FOR_PLAYER:
                break;
            case GameState.WAITING_FOR_AI:
                break;
            case GameState.PLAYING:
                if (!tested)
                {
                    TestWinCondition();
                }
                break;
            case GameState.GAME_OVER:
                break;
            case GameState.RESTARTING:
                gameState = GameState.INITIALIZING;
                break;
            default:
                Debug.LogError("Unknown game state: " + gameState);
                break;
        }
    }
    void TestWinCondition()
    {

        var slots = tableManager.GetTable().GetSlots();
        var testOwner = new Player("TestPlayer", 1, player1Sprite);
        int xSize = tableManager.GetTable().GetXSize();
        int ySize = tableManager.GetTable().GetYSize();
        int diagSize = Mathf.Min(xSize, ySize);
        for (int x = 0; x < xSize; x++)
            for (int y = 0; y < ySize; y++)
            {
                slots[x][y].SetState(SlotStates.EMPTY);
                slots[x][y].SetOwner(null);
            }
        Debug.Log(diagSize);
        for (int i = 0; i < diagSize; i++)
        {
            Debug.Log($"Setting slot ({i},{ySize - 1 - i})");
            slots[xSize - 1 - i][i].SetState(SlotStates.OCCUPIED);
            slots[xSize - 1 - i][i].SetOwner(testOwner);
        }
        string winner = tableManager.CheckVictory().GetWinner()?.GetName();
        WinState wincon = tableManager.CheckVictory().GetWinCondition();
        Debug.Log("Winner: " + winner + " Win Condition: " + wincon);
        tested = true;
    }
}
