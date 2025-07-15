using TMPro;
using UnityEngine;

public class CardVisual : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;

    private Card card;
    private Vector3 originalScale;
    private Vector3 raisedScale;
    private float scaleAmount = 1.1f;
    private float animationDuration = 0.2f;

    private void Awake()
    {
        originalScale = transform.localScale;
        raisedScale = originalScale * scaleAmount;
    }

    public void Setup(Card cardData)
    {
        card = cardData;
        nameText.text = card.Name;
        descriptionText.text = card.Description;
        costText.text = card.EnergyCost.ToString();
    }

    public void SelectVisual()
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(raisedScale));
    }

    public void DeselectVisual()
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(originalScale));
    }

    private System.Collections.IEnumerator ScaleTo(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float time = 0f;

        while (time < animationDuration)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, time / animationDuration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;
    }
}
