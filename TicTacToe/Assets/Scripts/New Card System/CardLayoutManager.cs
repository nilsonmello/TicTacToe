using System.Collections.Generic;
using UnityEngine;

public class CardLayoutManager : MonoBehaviour
{
    [Header("card layout settings")]
    public List<CardInteraction> cards = new List<CardInteraction>(); //list of managed cards
    public float spacing = 150f;                                     //horizontal spacing between cards
    public float curveHeight = 30f;                                  //height of curved layout
    public float selectRaise = 20f;                                  //vertical offset for selected card
    private CardInteraction selectedCard;                            //currently selected card

    private void Start()
    {
        LayoutCards();                                               //initial layout setup
    }

    private void Update()
    {
        //deselect card if clicking outside ui
        if (Input.GetMouseButtonDown(0))
        {
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() && selectedCard != null)
            {
                DeselectCard();
            }
        }
    }

    //layout all cards in curve with spacing, adjusting for selection
    public void LayoutCards()
    {
        cards.RemoveAll(card => card == null); // remove null cards

        int count = cards.Count;
        if (count == 0) return;

        float minSpacing = -50f;
        float maxSpacing = spacing;
        float dynamicSpacing = Mathf.Clamp(maxSpacing - (count * 5f), minSpacing, maxSpacing);

        float centerIndex = (count - 1) / 2f;

        //if selected card is null, use first card as default
        for (int i = 0; i < count; i++)
        {
            if (cards[i].IsDragging)
                continue;
            //calculate position based on index and dynamic spacing
            float xPos = (i - centerIndex) * dynamicSpacing;
            float normalizedX = (i - centerIndex) / centerIndex;
            float yPos = -Mathf.Pow(normalizedX, 2) * curveHeight + curveHeight;

            Vector3 targetPos = new Vector3(xPos, yPos, 0);

            if (cards[i] == selectedCard)
                targetPos += Vector3.up * selectRaise;

            cards[i].MoveToLocalPosition(targetPos);
            cards[i].SetOriginalSortingOrder(i);
            cards[i].SetSortingOrder(i);
        }

        if (selectedCard != null && !selectedCard.IsDragging)
            selectedCard.SetSortingOrder(1000);
    }

    //select a card, deselect previous if any
    public void SelectCard(CardInteraction card)
    {
        if (!cards.Contains(card)) return;
        if (selectedCard == card) return;

        if (selectedCard != null)
        {
            selectedCard.cardVisual.DeselectVisual();
            selectedCard.RestoreOriginalSortingOrder();
        }

        selectedCard = card;
        selectedCard.cardVisual.SelectVisual();
        LayoutCards();                 //update layout with selection
    }

    //deselect current selected card
    public void DeselectCard()
    {
        if (selectedCard == null) return;

        Debug.Log($"deselecting card: {selectedCard.name}");
        selectedCard.cardVisual.DeselectVisual();
        selectedCard.RestoreOriginalSortingOrder();
        selectedCard = null;
        LayoutCards();                  //update layout without selection
    }

    //reorder card in list based on dragged x position
    public void ReorderCard(CardInteraction draggedCard, float draggedX)
    {
        cards.Remove(draggedCard);

        int insertIndex = 0;
        for (int i = 0; i < cards.Count; i++)
        {
            if (draggedX > cards[i].transform.localPosition.x)
                insertIndex = i + 1;
        }

        cards.Insert(insertIndex, draggedCard);
        LayoutCards();             //update layout after reorder
    }

    //check if card is selected
    public bool IsSelected(CardInteraction card)
    {
        return selectedCard == card;
    }

    //simulate drag layout without changing actual list order
    public void SimulateDrag(CardInteraction draggedCard, float draggedX)
    {
        if (!cards.Contains(draggedCard)) return;

        draggedCard.SetSortingOrder(1000);  // bring dragged card to front

        List<CardInteraction> simulated = new List<CardInteraction>(cards);
        simulated.Remove(draggedCard);

        int insertIndex = 0;
        for (int i = 0; i < simulated.Count; i++)
        {
            if (draggedX > simulated[i].transform.localPosition.x)
                insertIndex = i + 1;
        }

        simulated.Insert(insertIndex, draggedCard);

        int count = simulated.Count;
        float minSpacing = 80f;
        float maxSpacing = spacing;
        float dynamicSpacing = Mathf.Clamp(maxSpacing - (count * 5f), minSpacing, maxSpacing);

        float centerIndex = (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            float xPos = (i - centerIndex) * dynamicSpacing;
            float normalizedX = (i - centerIndex) / centerIndex;
            float yPos = -Mathf.Pow(normalizedX, 2) * curveHeight + curveHeight;

            Vector3 pos = new Vector3(xPos, yPos, 0);

            if (simulated[i] == draggedCard) continue;

            simulated[i].MoveToLocalPosition(pos);
        }
    }

    //deselect selected card unless it is exceptCard
    public void DeselectAllExcept(CardInteraction exceptCard)
    {
        if (selectedCard != null && selectedCard != exceptCard)
            DeselectCard();
    }

    //destroy a random card and remove it from layout
    public void RemoveRandomCard()
    {
        if (cards.Count == 0) return;

        int randomIndex = Random.Range(0, cards.Count);
        CardInteraction cardToRemove = cards[randomIndex];

        if (cardToRemove == selectedCard)
        {
            selectedCard = null;
        }

        cards.RemoveAt(randomIndex);
        Destroy(cardToRemove.gameObject);

        LayoutCards();
    }
}