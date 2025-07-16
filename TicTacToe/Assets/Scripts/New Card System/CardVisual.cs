using UnityEngine;
using System.Collections;
using TMPro;

public class CardVisual : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI nameText;                //displays the card's name
    public TextMeshProUGUI descriptionText;         //displays the card's description
    public TextMeshProUGUI costText;                //displays the card's energy cost

    [Header("Card Data")]
    private Card card;                              //reference to the logical data of the card

    [Header("Scale Animation (to be Removed)")]
    private Vector3 originalScale;                  //default scale
    private Vector3 raisedScale;                    //enlarged scale for selected effect
    private float scaleAmount = 1.1f;               //scale multiplier when selected
    private float animationDuration = 0.2f;         //time for scale animation

    [Header("Movement Interpolation")]
    private Vector3 positionTarget;                //target position to move toward
    private Coroutine moveCoroutine;               //coroutine for movement interpolation
    public float moveDuration = 0.25f;             //time to reach target position
    [HideInInspector]
    public bool allowPositionInterpolation = true; //toggle to enable or disable smooth movement

    private void Awake()
    {
        //set initial scale and position
        originalScale = transform.localScale;
        raisedScale = originalScale * scaleAmount;
        positionTarget = transform.localPosition;
    }

    /// <summary>
    ///coroutine to interpolate position smoothly
    /// </summary>
    private IEnumerator MoveRoutine()
    {
        Vector3 startPos = transform.localPosition;
        float time = 0f;

        while (time < moveDuration)
        {
            transform.localPosition = Vector3.Lerp(startPos, positionTarget, time / moveDuration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = positionTarget;
    }

    /// <summary>
    ///moves the card to a new position with optional interpolation
    /// </summary>
    public void MoveToPosition(Vector3 newPosition)
    {
        if (!allowPositionInterpolation)
        {
            StopAllCoroutines();
            transform.localPosition = newPosition;
            positionTarget = newPosition;
            return;
        }

        positionTarget = newPosition;

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveRoutine());
    }

    /// <summary>
    ///set the visual text info based on Card data
    /// </summary>
    public void Setup(Card cardData)
    {
        card = cardData;
        nameText.text = card.Name;
        descriptionText.text = card.Description;
        costText.text = card.EnergyCost.ToString();
    }

    /// <summary>
    ///trigger visual scale-up effect (selected)
    /// </summary>
    public void SelectVisual()
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(raisedScale));
    }

    /// <summary>
    ///revert scale to original (deselected)
    /// </summary>
    public void DeselectVisual()
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(originalScale));
    }

    /// <summary>
    ///coroutine to animate scale change
    /// </summary>
    private IEnumerator ScaleTo(Vector3 targetScale)
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
