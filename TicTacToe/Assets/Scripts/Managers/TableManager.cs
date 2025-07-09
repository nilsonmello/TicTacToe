using UnityEngine;
using System.Collections.Generic;
public class TableManager : MonoBehaviour
{
    private GameObject tableObject;
    private Table table;
    private bool tableCreated = false;

    public GameObject tablePrefab;

    void Start()
    {

    }
    public void CreateTable(List<Slot> slotTypes, int slotQuantity)
    {
        if (tableCreated)
        {
            Debug.LogWarning("Tried creating Table but Table already created, try deleting it before creating.");
        };
        tableObject = Instantiate(tablePrefab);
        table = tableObject.GetComponent<Table>();
        table.GenerateTable(slotTypes);
        table.GenerateGraphic();

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