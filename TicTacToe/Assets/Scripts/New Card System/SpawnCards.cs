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
            Card card = CardDatabase.GetRandomCard();
            if (card != null)
            {
                GameObject go = Instantiate(cardPrefab, cardParent);
                go.name = card.Name;

                CardInteraction interaction = go.GetComponent<CardInteraction>();
                interaction.Initialize(card);

                layoutManager.cards.Add(interaction);
            }
        }

        layoutManager.LayoutCards();
    }

    public void SpawnSingleCard()
    {
        if (!canSpawn) return;

        StartCoroutine(SpawnCardWithCooldown());
    }

    private IEnumerator SpawnCardWithCooldown()
    {
        canSpawn = false;

        SpawnCard();

        yield return new WaitForSeconds(spawnCooldown);
        canSpawn = true;
    }

    private void SpawnCard()
    {
        Card card = CardDatabase.GetRandomCard();
        if (card == null) return;

        GameObject go = Instantiate(cardPrefab, cardParent);
        go.name = card.Name;

        CardInteraction interaction = go.GetComponent<CardInteraction>();
        interaction.Initialize(card);

        layoutManager.cards.Add(interaction);
        layoutManager.LayoutCards();

    }
}
