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
        //initial layout of cards on start
        LayoutCards();
    }

    private void Update()
    {
        //deselect card if user clicks outside any UI element and a card is selected
        if (Input.GetMouseButtonDown(0))
        {
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() && selectedCard != null)
            {
                DeselectCard();
            }
        }
    }

    /// <summary>
    ///positions all cards in a curved layout, applying selected card raise and sorting order
    /// </summary>
    public void LayoutCards()
    {
        int count = cards.Count;
        if (count == 0) return;

        float centerIndex = (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            float xPos = (i - centerIndex) * spacing;
            float normalizedX = (i - centerIndex) / centerIndex;
            float yPos = -Mathf.Pow(normalizedX, 2) * curveHeight + curveHeight;

            Vector3 targetPos = new Vector3(xPos, yPos, 0);

            if (cards[i] == selectedCard)
            {
                targetPos += Vector3.up * selectRaise;
            }

            //move card smoothly to target position
            cards[i].MoveToLocalPosition(targetPos);
            cards[i].transform.localRotation = Quaternion.identity;

            //set base sorting order according to card index (leftmost lowest)
            cards[i].SetOriginalSortingOrder(i);
            cards[i].SetSortingOrder(i);
        }

        //bring selected card to the front visually by setting high sorting order
        if (selectedCard != null)
        {
            selectedCard.SetSortingOrder(1000);
        }
    }

    /// <summary>
    ///select a card and update layout
    /// </summary>
    public void SelectCard(CardInteraction card)
    {
        if (!cards.Contains(card)) return;
        if (selectedCard == card) return;

        if (selectedCard != null)
        {
            selectedCard.RestoreOriginalSortingOrder();
        }

        selectedCard = card;
        LayoutCards();
    }

    /// <summary>
    ///deselect currently selected card and update layout
    /// </summary>
    public void DeselectCard()
    {
        if (selectedCard != null)
        {
            selectedCard.RestoreOriginalSortingOrder();
            selectedCard = null;
            LayoutCards();
        }
    }

    /// <summary>
    ///reorders the dragged card within the list based on its horizontal dragged position
    /// </summary>
    public void ReorderCard(CardInteraction draggedCard, float draggedX)
    {
        cards.Remove(draggedCard);

        int insertIndex = 0;
        for (int i = 0; i < cards.Count; i++)
        {
            if (draggedX > cards[i].transform.localPosition.x)
            {
                insertIndex = i + 1;
            }
        }

        cards.Insert(insertIndex, draggedCard);
        LayoutCards();
    }

    /// <summary>
    ///check if a card is currently selected
    /// </summary>
    public bool IsSelected(CardInteraction card)
    {
        return selectedCard == card;
    }

    /// <summary>
    ///simulates the layout while dragging a card, updating positions of other cards smoothly
    /// </summary>
    public void SimulateDrag(CardInteraction draggedCard, float draggedX)
    {
        if (!cards.Contains(draggedCard)) return;

        List<CardInteraction> simulated = new List<CardInteraction>(cards);
        simulated.Remove(draggedCard);

        int insertIndex = 0;
        for (int i = 0; i < simulated.Count; i++)
        {
            if (draggedX > simulated[i].transform.localPosition.x)
            {
                insertIndex = i + 1;
            }
        }

        simulated.Insert(insertIndex, draggedCard);

        float centerIndex = (simulated.Count - 1) / 2f;

        for (int i = 0; i < simulated.Count; i++)
        {
            float xPos = (i - centerIndex) * spacing;
            float normalizedX = (i - centerIndex) / centerIndex;
            float yPos = -Mathf.Pow(normalizedX, 2) * curveHeight + curveHeight;

            Vector3 pos = new Vector3(xPos, yPos, 0);

            if (simulated[i] == draggedCard) continue;

            simulated[i].MoveToLocalPosition(pos);
            simulated[i].transform.localRotation = Quaternion.identity;
        }
    }
}
