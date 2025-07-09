using UnityEngine;
using System.Collections.Generic;
public class Table : MonoBehaviour
{
    private List<List<Slot>> slots;

    private List<GameObject> generated_slots;

    public GameObject slotPrefab;
    private int gridSize;
    
    public void GenerateTable(List<System.Type> slotTypes, Slot defaultSlot)
        {
            Dictionary<System.Type, int> slotUsage = new Dictionary<System.Type, int>();
            foreach (var slotType in slotTypes)
            {
                slotUsage[slotType] = 0;
            }

            List<List<Slot>> table = new List<List<Slot>>();

            for (int i = 0; i < gridSize; i++)
            {
                List<Slot> row = new List<Slot>();
                for (int j = 0; j < gridSize; j++)
                {
                    List<System.Type> available = new List<System.Type>();
                    foreach (var slotType in slotTypes)
                    {
                        // Create a temp instance to check the limit
                        Slot tempSlot = (Slot)System.Activator.CreateInstance(slotType);
                        // Debug.Log($"{slotType.Name} limit: {tempSlot.GetUseLimit()} usage: {slotUsage[slotType]}");
                        if (slotUsage[slotType] < tempSlot.GetUseLimit())
                            available.Add(slotType);
                    }

                    Slot chosenSlot;

                    if (available.Count > 0)
                    {
                        var chosenType = available[UnityEngine.Random.Range(0, available.Count)];
                        chosenSlot = (Slot)System.Activator.CreateInstance(chosenType);
                        slotUsage[chosenType]++;
                        //Debug.Log($"slots[{i}][{j}] is {chosenSlot}");
                    }
                    else
                    {
                        chosenSlot = defaultSlot;
                        //Debug.Log($"slots[{i}][{j}] is {chosenSlot} (default)");
                    }
                    row.Add(chosenSlot);
                }
                table.Add(row);
            }
            slots = table;
        }
    public void GenerateGraphic(float spacing)
    {
        generated_slots = new List<GameObject>();
        Transform parentTransform = this.transform;

        float offset = (gridSize - 1) * spacing * 0.5f;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector2 localPosition = new Vector2(x * spacing - offset, y * spacing - offset);
                GameObject slotObj = GameObject.Instantiate(
                    slotPrefab,
                    parentTransform.position + (Vector3)localPosition,
                    Quaternion.identity,
                    parentTransform
                );
                slotObj.name = $"Slot_{x}_{y}";
                slotObj.transform.localPosition = localPosition;
                var controller = slotObj.GetComponent<SlotController>();
                    if (controller != null)
                        controller.SetSlot(slots[x][y]);
                    else
                        Debug.LogError("Slot prefab missing SlotController!");

                generated_slots.Add(slotObj);
            }
        }
    }

    public Slot GetSlot(int slotIndex)
    {
        return generated_slots[slotIndex].GetComponent<Slot>();
    }
    public void SetSlotQuantity(int quantity)
    {
        gridSize = quantity;
    }
}
