using System.Collections.Generic;
using UnityEngine;

public class CardLayoutManager : MonoBehaviour
{
    [Header("Card Layout Settings")]
    public List<CardInteraction> cards = new List<CardInteraction>();  //list of cards managed by this layout
    public float spacing = 150f;            //horizontal spacing between cards
    public float curveHeight = 30f;         //vertical curve height for card layout arc
    public float selectRaise = 20f;         //vertical raise amount for selected card

    private CardInteraction selectedCard;   //currently selected card

    private void Start()
    {
        LayoutCards();                      //initial layout of cards on start
    }

    private void Update()
    {
        //deselect card if clicked outside UI and a card is selected
        if (Input.GetMouseButtonDown(0))
        {
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() && selectedCard != null)
            {
                DeselectCard();
            }
        }
    }

    public void LayoutCards()
    {
        int count = cards.Count;
        if (count == 0) return;

        float centerIndex = (count - 1) / 2f;  //calculate center index for spacing

        for (int i = 0; i < count; i++)
        {
            float xPos = (i - centerIndex) * spacing;  //horizontal position based on index
            float normalizedX = (i - centerIndex) / centerIndex;  //normalized horizontal position
            float yPos = -Mathf.Pow(normalizedX, 2) * curveHeight + curveHeight;  //curve height based on normalized x

            Vector3 targetPos = new Vector3(xPos, yPos, 0);

            if (cards[i] == selectedCard)
            {
                targetPos += Vector3.up * selectRaise;  //raise selected card up visually
            }

            cards[i].MoveToLocalPosition(targetPos);  //move card smoothly to target position

            cards[i].SetOriginalSortingOrder(i);     //store original sorting order
            cards[i].SetSortingOrder(i);             //set sorting order to maintain proper draw order
        }

        if (selectedCard != null)
        {
            selectedCard.SetSortingOrder(1000);     //bring selected card visually on top
        }
    }

    public void SelectCard(CardInteraction card)
    {
        if (!cards.Contains(card)) return;
        if (selectedCard == card) return;

        if (selectedCard != null)
        {
            selectedCard.cardVisual.DeselectVisual();      //update visuals of previously selected card
            selectedCard.RestoreOriginalSortingOrder();   //restore sorting order of previously selected card
        }

        selectedCard = card;
        selectedCard.cardVisual.SelectVisual();            //update visuals of newly selected card
        LayoutCards();                                      //relayout all cards to reflect selection
    }

    public void DeselectCard()
    {
        if (selectedCard != null)
        {
            selectedCard.cardVisual.DeselectVisual();      //update visuals on deselect
            selectedCard.RestoreOriginalSortingOrder();   //restore original sorting order
            selectedCard = null;
            LayoutCards();                                 //relayout cards after deselection
        }
    }

    public void ReorderCard(CardInteraction draggedCard, float draggedX)
    {
        cards.Remove(draggedCard);  //temporarily remove dragged card from list

        int insertIndex = 0;
        for (int i = 0; i < cards.Count; i++)
        {
            if (draggedX > cards[i].transform.localPosition.x)
            {
                insertIndex = i + 1;  //determine new insert index based on dragged x position
            }
        }

        cards.Insert(insertIndex, draggedCard);  //insert dragged card back into list at new position
        LayoutCards();                           //relayout cards to update positions
    }

    public bool IsSelected(CardInteraction card)
    {
        return selectedCard == card;             //check if given card is currently selected
    }

    public void SimulateDrag(CardInteraction draggedCard, float draggedX)
    {
        if (!cards.Contains(draggedCard)) return;

        draggedCard.SetSortingOrder(1000);      //bring dragged card visually on top

        List<CardInteraction> simulated = new List<CardInteraction>(cards);
        simulated.Remove(draggedCard);           //remove dragged card from simulation list

        int insertIndex = 0;
        for (int i = 0; i < simulated.Count; i++)
        {
            if (draggedX > simulated[i].transform.localPosition.x)
            {
                insertIndex = i + 1;             //find insertion index based on dragged x position
            }
        }

        simulated.Insert(insertIndex, draggedCard);  //insert dragged card into simulated list

        float centerIndex = (simulated.Count - 1) / 2f;

        for (int i = 0; i < simulated.Count; i++)
        {
            float xPos = (i - centerIndex) * spacing;          //calculate horizontal position
            float normalizedX = (i - centerIndex) / centerIndex;  //normalized position for curve
            float yPos = -Mathf.Pow(normalizedX, 2) * curveHeight + curveHeight;  //curve height

            Vector3 pos = new Vector3(xPos, yPos, 0);

            if (simulated[i] == draggedCard) continue;         //skip moving dragged card here

            simulated[i].MoveToLocalPosition(pos);              //move other cards smoothly to their positions
        }
    }
}
