using UnityEngine;
using System.Collections.Generic;
public class Table : MonoBehaviour
{
    private List<List<Slot>> slotsMatrix;

    private List<List<GameObject>> slotObjectGrid;

    private readonly Dictionary<Vector2Int, GameObject> slotObjects = new Dictionary<Vector2Int, GameObject>();
    private Queue<GameObject> slotPool = new Queue<GameObject>();
    public GameObject slotPrefab;
    private int xSize;
    private int ySize;
    private Dictionary<System.Type, int> slotUsage = new();
    private List<System.Type> slotTypes = new List<System.Type>();
    private Slot defaultSlot;

    private float spacing;

    /// <summary>
    /// Generates the table structure with the specified slot types and default slot.
    /// </summary>
    public void GenerateTable(List<System.Type> slotTypes, Slot defaultSlot)
    {
        if (slotTypes == null || slotTypes.Count == 0)
        {
            Debug.LogError("No slot types provided, cannot generate table.");
            return;
        }
        if (defaultSlot == null)
        {
            Debug.LogError("Default slot cannot be null, using DefaultSlot instead.");
            defaultSlot = new DefaultSlot();
        }
        this.slotTypes = slotTypes;
        this.defaultSlot = defaultSlot;

        foreach (var slotType in slotTypes)
            slotUsage[slotType] = 0;

        List<List<Slot>> table = new();

        for (int x = 0; x < xSize; x++)
        {
            List<Slot> column = new();
            for (int y = 0; y < ySize; y++)
            {
                List<System.Type> available = new();
                foreach (var slotType in slotTypes)
                {
                    Slot tempSlot = (Slot)System.Activator.CreateInstance(slotType);
                    if (slotUsage[slotType] < tempSlot.GetUseLimit())
                        available.Add(slotType);
                }

                Slot chosenSlot;
                if (available.Count > 0)
                {
                    var chosenType = available[UnityEngine.Random.Range(0, available.Count)];
                    chosenSlot = (Slot)System.Activator.CreateInstance(chosenType);
                    slotUsage[chosenType]++;
                }
                else
                {
                    chosenSlot = (Slot)System.Activator.CreateInstance(defaultSlot.GetType());
                }
                column.Add(chosenSlot);
            }
            table.Add(column);
        }
        slotsMatrix = table;
    }
    /// <summary>
    /// Regenerates the table structure with the already defined slot types and default slot.
    /// </summary>
    public void RegenerateTable()
    {
        foreach (var slotType in slotTypes)
            slotUsage[slotType] = 0;

        GenerateTable(slotTypes, defaultSlot);
    }
    public void GenerateGraphic(float spacing)
    {
        this.spacing = spacing;
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
                GameObject slotObj;
                 Vector2 localPosition = new Vector2(x * spacing - offsetX, y * spacing - offsetY);
                if (slotPool.Count > 0)
                {
                    slotObj = slotPool.Dequeue();
                    slotObj.SetActive(true);
                }
                else
                {
                    slotObj = GameObject.Instantiate(
                       slotPrefab,
                       parentTransform.position + (Vector3)localPosition,
                       Quaternion.identity,
                       parentTransform
                   );
                }
                slotObj.name = $"Slot_{x}_{y}";
                slotObj.transform.localPosition = localPosition;
                if (slotObj.TryGetComponent<SlotController>(out var controller))
                    controller.SetSlot(slotsMatrix[x][y]);
                else
                    Debug.LogError("Slot prefab missing SlotController!");

                Vector2Int gridPos = new(x, y);
                slotObjects[gridPos] = slotObj;
                slotObjectGrid[x].Add(slotObj);
            }
        }
    }
    public void RegenerateGraphic()
    {
        foreach (var slotObject in slotObjects.Values)
        {
            slotObject.SetActive(false);
            slotPool.Enqueue(slotObject);
        }
        slotObjects.Clear();
        slotObjectGrid.Clear();
        GenerateGraphic(spacing);
    }
    public GameObject GetOneSlotObject(int x, int y)
    {
        Vector2Int position = new(x, y);
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
    public void SetSlotQuantity(int quantityX, int quantityY)
    {
        xSize = quantityX;
        ySize = quantityY;
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
