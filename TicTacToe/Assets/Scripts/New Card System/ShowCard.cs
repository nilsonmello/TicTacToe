using UnityEngine;

public class ShowCard : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform cardParent;

    // Referência para o CardLayoutManager que deve estar no cardParent ou pai dele
    private CardLayoutManager layoutManager;

    void Start()
    {
        layoutManager = cardParent.GetComponent<CardLayoutManager>();
        if (layoutManager == null)
        {
            Debug.LogError("CardLayoutManager não encontrado no cardParent.");
            return;
        }

        for (int i = 0; i < 5; i++)
        {
            Card carta = CardDatabase.GetRandomCard();
            if (carta != null)
            {
                GameObject go = Instantiate(cardPrefab, cardParent);
                CardVisual cardVisual = go.GetComponent<CardVisual>();
                cardVisual.Setup(carta);

                // Adiciona a carta na lista do layout manager
                layoutManager.cards.Add(cardVisual);
            }
        }

        // Atualiza o layout após criar as cartas
        layoutManager.LayoutCards();
    }
}
