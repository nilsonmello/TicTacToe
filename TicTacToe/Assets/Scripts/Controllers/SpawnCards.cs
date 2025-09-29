using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnCards : MonoBehaviour
{
    [Header("Prefabs & References")]
    public GameObject cardPrefab;
    public Transform cardParent;
    private CardLayoutManager layoutManager;

    [SerializeField] private int cardTotal = 5;
    [SerializeField] private float spawnCooldown = 0f;
    [SerializeField] private bool spawnInPullPanel = false; // NOVO toggle

    private bool canSpawn = true;

    private void Start()
    {
        layoutManager = cardParent.GetComponent<CardLayoutManager>();
        if (layoutManager == null)
        {
            Debug.LogError("CardLayoutManager not found on cardParent.");
            return;
        }

        for (int i = 0; i < cardTotal; i++)
        {
            SpawnCard(initialSpawn: true);
        }

        StartCoroutine(DelayedInitialLayout());
    }

    private IEnumerator DelayedInitialLayout()
    {
        yield return null;
        layoutManager.LayoutCards();

        for (int i = 0; i < layoutManager.cards.Count; i++)
        {
            layoutManager.cards[i].UpdateVisualSortingOrder(i * 10);
        }
    }

    public void SpawnSingleCard()
    {
        if (!canSpawn) return;
        StartCoroutine(SpawnCardWithCooldown());
    }

    private IEnumerator SpawnCardWithCooldown()
    {
        canSpawn = false;
        SpawnCard(initialSpawn: false);
        yield return new WaitForSeconds(spawnCooldown);
        canSpawn = true;
    }

    private void SpawnCard(bool initialSpawn)
    {
        Card card = CardDatabase.GetRandomCard();
        if (card == null) return;

        GameObject go = Instantiate(cardPrefab, cardParent);
        go.name = card.Name;

        CardInteraction interaction = go.GetComponent<CardInteraction>();
        interaction.Initialize(card);

        layoutManager.cards.Add(interaction);


        if (layoutManager.panelData.panelType == PanelType.Pull)
        {
            int order = layoutManager.cards.Count * 10;
            interaction.SetLocalPositionInstant(Vector3.zero);
            interaction.UpdateVisualSortingOrder(order);
            return;
        }

        if (!initialSpawn)
        {
            layoutManager.LayoutCards();
            int order = layoutManager.GetCardOrder(interaction) * 10;
            StartCoroutine(DelayedSorting(interaction, order));
        }
    }

    private IEnumerator DelayedSorting(CardInteraction interaction, int order)
    {
        yield return new WaitUntil(() => interaction.cardVisualInstance != null);
        interaction.UpdateVisualSortingOrder(order);
    }
}
