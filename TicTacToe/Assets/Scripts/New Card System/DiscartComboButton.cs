using UnityEngine;
using System.Collections.Generic;

public class DiscardComboButton : MonoBehaviour
{
    [Header("Panel references")]
    public CardLayoutManager comboPanel; //reference to combo panel
    public CardLayoutManager discardPanel; //reference to discard panel

    public void DiscardAllComboCards()
    {
        if (comboPanel == null || discardPanel == null)
        {
            Debug.LogWarning("Combo or Discard panel reference is missing."); //warn if panels not assigned
            return;
        }

        List<CardInteraction> cardsToDiscard = new List<CardInteraction>(comboPanel.cards); //copy cards from combo panel

        comboPanel.cards.Clear(); //clear combo panel cards

        float centerIndex = (cardsToDiscard.Count - 1) / 2f; //calculate center index for discard layout

        for (int i = 0; i < cardsToDiscard.Count; i++)
        {
            CardInteraction card = cardsToDiscard[i];

            card.layoutManager = discardPanel; //set card's layout manager to discard panel

            Vector3 worldPos = card.transform.position; //store world position
            Quaternion worldRot = card.transform.rotation; //store world rotation

            card.transform.SetParent(discardPanel.transform, false); //set parent to discard panel without changing local transform

            card.transform.position = worldPos; //restore world position to avoid snapping
            card.transform.rotation = worldRot; //restore rotation

            discardPanel.cards.Add(card); //add card to discard panel list

            float xPos = (i - centerIndex) * discardPanel.dynamicSpacing; //calculate local x position
            float normalizedX = centerIndex != 0 ? (i - centerIndex) / centerIndex : 0f; //normalize position
            float yPos = -Mathf.Pow(normalizedX, 2) * discardPanel.curveHeight + discardPanel.curveHeight; //calculate curved y position
            Vector3 targetLocalPos = new Vector3(xPos, yPos, 0f);

            card.MoveToLocalPosition(targetLocalPos); //move card smoothly to target position

            card.canInteract = false; //disable interaction on discarded cards

            discardPanel.panelData?.OnCardDropped(card); //notify panel of card drop if panelData exists
        }

        discardPanel.LayoutCards(); //refresh discard panel layout
    }
}
