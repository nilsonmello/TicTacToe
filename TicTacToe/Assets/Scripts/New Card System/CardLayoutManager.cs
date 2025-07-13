using System.Collections.Generic;
using UnityEngine;

public class CardLayoutManager : MonoBehaviour
{
    public List<CardVisual> cards = new List<CardVisual>();
    public float spacing = 150f;       // Espaçamento horizontal entre cartas
    public float curveHeight = 30f;    // Altura da curva (montanha)
    public float selectRaise = 20f;    // Quanto a carta selecionada sobe no eixo Y
    public float scaleAmount = 1.1f;   // Escala para seleção

    private CardVisual selectedCard;

    void Start()
    {
        LayoutCards();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Se clicou em algo que não é UI e há uma carta selecionada
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

        float centerIndex = (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            // Calcula posição x
            float xPos = (i - centerIndex) * spacing;

            // Calcula y com curva parabólica (para formar a "montanha")
            float normalizedX = (i - centerIndex) / centerIndex; // vai de -1 a 1
            float yPos = -Mathf.Pow(normalizedX, 2) * curveHeight + curveHeight;

            Vector3 targetPos = new Vector3(xPos, yPos, 0);
            cards[i].transform.localPosition = targetPos;

            // Reseta rotação e escala
            cards[i].transform.localRotation = Quaternion.identity;
            cards[i].transform.localScale = Vector3.one;

            // Ajusta elevação e escala se for carta selecionada
            if (cards[i] == selectedCard)
            {
                cards[i].transform.localPosition += Vector3.up * selectRaise;
                cards[i].transform.localScale = Vector3.one * scaleAmount;
            }
        }
    }

    public void SelectCard(CardVisual card)
    {
        if (!cards.Contains(card)) return; // evita selecionar carta que não está na lista
        if (selectedCard == card) return;

        if (selectedCard != null)
            selectedCard.DeselectVisual();

        selectedCard = card;
        selectedCard.SelectVisual();

        LayoutCards();
    }

    public void DeselectCard()
    {
        if (selectedCard != null)
        {
            selectedCard.DeselectVisual();
            selectedCard = null;
            LayoutCards();
        }
    }

    public void ReorderCard(CardVisual draggedCard, float draggedX)
    {
        // Remove o card temporariamente da lista
        cards.Remove(draggedCard);

        // Encontra o índice mais próximo baseado no X
        int insertIndex = 0;
        for (int i = 0; i < cards.Count; i++)
        {
            if (draggedX > cards[i].transform.localPosition.x)
            {
                insertIndex = i + 1;
            }
        }

        // Reinsere a carta na nova posição
        cards.Insert(insertIndex, draggedCard);

        // Atualiza layout visual
        LayoutCards();
    }

    public bool IsSelected(CardVisual card)
    {
        return selectedCard == card;
    }

    public void SimulateDrag(CardVisual draggedCard, float draggedX)
    {
        if (!cards.Contains(draggedCard)) return;

        // Cria uma cópia da lista
        List<CardVisual> simulated = new List<CardVisual>(cards);
        simulated.Remove(draggedCard);

        // Encontra o índice ideal com base na posição X atual do drag
        int insertIndex = 0;
        for (int i = 0; i < simulated.Count; i++)
        {
            if (draggedX > simulated[i].transform.localPosition.x)
            {
                insertIndex = i + 1;
            }
        }

        simulated.Insert(insertIndex, draggedCard);

        // Posiciona visualmente a lista simulada
        float centerIndex = (simulated.Count - 1) / 2f;

        for (int i = 0; i < simulated.Count; i++)
        {
            Vector3 pos;
            float xPos = (i - centerIndex) * spacing;
            float normalizedX = (i - centerIndex) / centerIndex;
            float yPos = -Mathf.Pow(normalizedX, 2) * curveHeight + curveHeight;

            pos = new Vector3(xPos, yPos, 0);

            // Não mover a carta que está sendo arrastada
            if (simulated[i] != draggedCard)
            {
                simulated[i].transform.localPosition = pos;
                simulated[i].transform.localRotation = Quaternion.identity;
                simulated[i].transform.localScale = Vector3.one;
            }
        }
    }
}
