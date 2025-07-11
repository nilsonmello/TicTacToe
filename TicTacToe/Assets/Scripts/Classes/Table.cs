using UnityEngine;
using System.Collections.Generic;
public class Table : MonoBehaviour
{
    private List<List<Slot>> slotsMatrix;

    private List<List<GameObject>> slotObjectGrid;

    private readonly Dictionary<Vector2Int, GameObject> slotObjects = new Dictionary<Vector2Int, GameObject>();
    public GameObject slotPrefab;
    private int xSize;
    private int ySize;

    public void GenerateTable(List<System.Type> slotTypes, Slot defaultSlot)
    {
        Dictionary<System.Type, int> slotUsage = new Dictionary<System.Type, int>();
        foreach (var slotType in slotTypes)
        {
            slotUsage[slotType] = 0;
        }

        List<List<Slot>> table = new List<List<Slot>>();

        for (int i = 0; i < xSize; i++)
        {
            List<Slot> row = new List<Slot>();
            for (int j = 0; j < ySize; j++)
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
                    chosenSlot = (Slot)System.Activator.CreateInstance(defaultSlot.GetType());
                    //Debug.Log($"slots[{i}][{j}] is {chosenSlot} (default)");
                }
                row.Add(chosenSlot);
            }
            table.Add(row);
        }
        slotsMatrix = table;
    }
    public void GenerateGraphic(float spacing)
    {
        slotObjectGrid = new List<List<GameObject>>();
        Transform parentTransform = this.transform;

        float offsetX = (xSize - 1) * spacing * 0.5f;
        float offsetY = (ySize - 1) * spacing * 0.5f;

        for (int x = 0; x < xSize; x++)
        {
            if (slotObjectGrid.Count <= x)
                slotObjectGrid.Add(new List<GameObject>());
            for (int y = 0; y < ySize; y++)
            {
                Vector2 localPosition = new Vector2(x * spacing - offsetX, y * spacing - offsetY);
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
                    controller.SetSlot(slotsMatrix[x][y]);
                else
                    Debug.LogError("Slot prefab missing SlotController!");

                if (slotObjectGrid.Count <= x)
                    slotObjectGrid.Add(new List<GameObject>());

                Vector2Int gridPos = new Vector2Int(x, y);
                slotObjects[gridPos] = slotObj;
                slotObjectGrid[x].Add(slotObj);
            }
        }
    }
    public void SetSlotQuantity(int quantityX, int quantityY)
    {
        xSize = quantityX;
        ySize = quantityY;
    }

    public GameObject GetOneSlotObject(int x, int y)
    {
        Vector2Int position = new Vector2Int(x, y);
        if (slotObjects.TryGetValue(position, out GameObject slotObject))
        {
            return slotObject;
        }
        Debug.LogError($"No slot object found at position {position}");
        return null;
    }
    public List<List<GameObject>> GetSlotObjectGrid()
    {
        return slotObjectGrid;
    }
    public List<List<Slot>> GetSlots()
    {
        return slotsMatrix;
    }

    public int GetXSize()
    {
        return xSize;
    }
    public int GetYSize()
    {
        return ySize;
    }
}
