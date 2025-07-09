using UnityEngine;
using System.Collections.Generic;
public class TableManager : MonoBehaviour
{
    private GameObject tableObject;
    private Table table;
    private bool tableCreated = false;
    public GameObject tablePrefab;

    // There is no actual limit but 2 is the minimum
    [Range(2, 20)]
    public int gridSize = 3; // Default grid size, can be changed in inspector
    public float spacing = 1.1f; // Default spacing, can be changed in inspector
    private Slot defaultSlot = new DefaultSlot();
    private List<System.Type> slotTypes  = new List<System.Type> { typeof(ToxicSlot), typeof(BrightSlot) };
    void Start()
    {
        CreateTable(slotTypes, defaultSlot, gridSize, spacing);
    }
    public void CreateTable(List<System.Type> slotTypes, Slot defaultSlot, int slotQuantity, float spacing)
    {
        if (tableCreated)
        {
            Debug.LogWarning("Tried creating Table but Table already created, try deleting it before creating.");
        };
        tableObject = GameObject.Instantiate(tablePrefab);
        tableObject.name = "GameTable";
        table = tableObject.GetComponent<Table>();
        table.SetSlotQuantity(slotQuantity);
        table.GenerateTable(slotTypes, defaultSlot);
        table.GenerateGraphic(spacing);
        tableCreated = true;
    }

    public void DeleteTable()
    {
        Destroy(tableObject);   
    }

    public void TriggerSlotEffect(int slotIndex)
    {
        if (!tableCreated) Debug.LogError("No Table, create it dumbass [TriggerSlotEffect]");
        table.GetSlot(slotIndex).UseSlotEffect();
    }

    public void UseEffectOnSlot(int slotIndex, Card card)
    {
        if (!tableCreated) Debug.LogError("No Table, create it dumbass [EffectOnSlot]");
        if (card.getUseCase() == UseCase.SLOT)
            table.GetSlot(slotIndex).UseCard(card);
        else Debug.LogWarning($"Can't use the Card [{card.getName()}] on Slots");
    }
}