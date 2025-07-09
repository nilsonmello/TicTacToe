using UnityEngine;
using System.Collections.Generic;
public class Table
{
    private List<Slot> slots;

    private List<GameObject> generated_slots;
    private int tableSize;

    // TODO , remember that we need to generate the table, based on SlotTypes,
    // these have limits of use, so if we can only use 4 Slots of type (Greed),
    // we DON'T USE MORE SLOTS OF THAT TYPE when generating the table.

    // tableSize can determine the size of the table, in a manner like X times X
    // so a grid would be of size tableSize times tableSize so a 10x10
    // would be a 100 itens grid.

    // The GenerateGraphic should look at the grid we have and when generating
    // cut the lines and collums based on tableSize, so a 10x10 would break at 
    // the 9th item of the list, it also should add the generated Objects in ORDER
    // to generated_slots list. 

    public void GenerateTable(List<Slot> SlotTypes) //
    {

    }   
    public void GenerateGraphic()
    {

    }
    public Slot GetSlot(int slotIndex)
    {
        return generated_slots[slotIndex].GetComponent<Slot>();
    }
    
}
