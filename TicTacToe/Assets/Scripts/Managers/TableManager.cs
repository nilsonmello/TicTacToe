using UnityEngine;
using System.Collections.Generic;
public class TableManager : MonoBehaviour
{
    private GameObject tableObject;
    private Table table;
    private bool tableCreated = false;
    public GameObject tablePrefab;

    [Header("Table Settings")]
    // There is no actual limit but 2 is the minimum
    [Range(2, 30)]
    public int xSize = 3; // Default grid size, can be changed in inspector
    [Range(2, 30)]
    public int ySize = 3; // Default grid size, can be changed in inspector
    public float spacing = 1.1f;

    public Quaternion rotation = Quaternion.identity;

    private Slot defaultSlot = new DefaultSlot();
    private List<System.Type> slotTypes = new List<System.Type> { typeof(ToxicSlot), typeof(BrightSlot) };
    public void Initialize()
    {
        CreateTable(slotTypes, defaultSlot, xSize, ySize, spacing);
    }
    public void CreateTable(List<System.Type> slotTypes, Slot defaultSlot, int xSize, int ySize, float spacing)
    {
        if (tableCreated || tableObject != null)
        {
            Debug.LogWarning("Tried creating Table but Table already created, try deleting it before creating.");
            return;
        }
        tableObject = GameObject.Instantiate(tablePrefab);
        tableObject.name = "GameTable";
        tableObject.transform.rotation = rotation;
        table = tableObject.GetComponent<Table>();
        table.SetSlotQuantity(xSize, ySize);
        table.GenerateTable(slotTypes, defaultSlot);
        table.GenerateGraphic(spacing);
        tableCreated = true;
    }

    public void SetParameters(List<System.Type> slotTypes, Slot defaultSlot, int xSize, int ySize, float spacing)
    {
        this.slotTypes = slotTypes;
        this.defaultSlot = defaultSlot;
        this.xSize = xSize;
        this.ySize = ySize;
        this.spacing = spacing;
    }
    public Table GetTable()
    {
        return table;
    }
    public void DeleteTable()
    {
        Debug.Log("Deleting Table", tableObject);
        Destroy(tableObject);
        tableCreated = false;
    }

    public void TriggerSlotEffect(int slotIndex)
    {
        if (!tableCreated) Debug.LogError("No Table, create it dumbass [TriggerSlotEffect]");
        // table.GetSlot(slotIndex).UseSlotEffect();
    }

    public void UseAbilityOnSlot(int slotIndex, Card card)
    {
        if (!tableCreated) Debug.LogError("No Table, create it dumbass [EffectOnSlot]");
        // if (card.getUseCase() == UseCase.SLOT)
        // table.GetSlot(slotIndex).UseCard(card);
        else Debug.LogWarning($"Can't use the Card [{card.Name}] on Slots");
    }

    public IGameResult CheckVictory()
    {
        var slots = table.GetSlots();
        int xSize = slots.Count;
        if (xSize == 0) return new GameResult(null, WinState.NONE);
        int ySize = slots[0].Count;

        // Check rows
        for (int y = 0; y < ySize; y++)
        {
            var first = slots[0][y];
            if (first.GetState() != SlotStates.OCCUPIED) continue;
            string owner = first.GetOwner().GetName();
            bool win = true;
            for (int x = 1; x < xSize; x++)
            {
                if (slots[x][y].GetState() != SlotStates.OCCUPIED || slots[x][y].GetOwner().GetName() != owner)
                {
                    win = false;
                    break;
                }
            }
            if (win) return new GameResult(first.GetOwner(), WinState.COLUMN);
        }

        // Check columns
        for (int x = 0; x < xSize; x++)
        {
            var first = slots[x][0];
            if (first.GetState() != SlotStates.OCCUPIED) continue;
            string owner = first.GetOwner().GetName();
            bool win = true;
            for (int y = 1; y < ySize; y++)
            {
                if (slots[x][y].GetState() != SlotStates.OCCUPIED || slots[x][y].GetOwner().GetName() != owner)
                {
                    win = false;
                    break;
                }
            }
            if (win) return new GameResult(first.GetOwner(), WinState.COLUMN);
        }
        if (xSize == ySize)
        {
            // Check main diagonal
            var diagFirst = slots[0][0];
            if (diagFirst.GetState() == SlotStates.OCCUPIED)
            {
                string owner = diagFirst.GetOwner().GetName();
                bool win = true;
                for (int i = 1; i < xSize; i++)
                {
                    if (slots[i][i].GetState() != SlotStates.OCCUPIED || slots[i][i].GetOwner().GetName() != owner)
                    {
                        win = false;
                        break;
                    }
                }
                if (win) return new GameResult(diagFirst.GetOwner(), WinState.DIAGONAL);
            }

            // Check anti-diagonal
            var antiDiagFirst = slots[xSize - 1][0];
            if (antiDiagFirst.GetState() == SlotStates.OCCUPIED)
            {
                string owner = antiDiagFirst.GetOwner().GetName();
                bool win = true;
                for (int i = 1; i < xSize; i++)
                {
                    if (slots[xSize - 1 - i][i].GetState() != SlotStates.OCCUPIED || slots[xSize - 1 - i][i].GetOwner().GetName() != owner)
                    {
                        win = false;
                        break;
                    }
                }
                if (win) return new GameResult(antiDiagFirst.GetOwner(), WinState.DIAGONAL);
            }
        }

        // No winner
        return new GameResult(null, WinState.NONE);
    }
}