using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class CardVisual : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Text References")]
    public TextMeshProUGUI nameText; //text field for card name
    public TextMeshProUGUI descriptionText; //text field for card description
    public TextMeshProUGUI costText; //text field for card energy cost

    [Header("Movement")]
    public float moveDuration = 0.25f; //duration for card movement tween
    [HideInInspector] public bool allowPositionInterpolation = true; //flag to allow smooth move

    [Header("Hover Effect")]
    public Transform scaleTarget; //transform to apply scale effects
    public float hoverScale = 1.05f; //scale amount on hover
    public float hoverDuration = 0.15f; //duration of hover scale tween

    [Header("Shake Settings")]
    public float hoverShakeDuration = 0.25f; //duration of shake on hover
    public float hoverShakeStrength = 10f; //strength of shake effect
    public int hoverShakeVibrato = 10; //number of shakes
    public float hoverShakeRandomness = 90f; //randomness angle of shake

    [Header("Punch Settings")]
    public float punchDuration = 0.3f; //duration of punch effect
    public float punchStrength = 20f; //strength of punch movement
    public int punchVibrato = 10; //vibrato for punch tween
    public float punchElasticity = 1f; //elasticity of punch tween

    [Header("Drag Effect")]
    public float dragScale = 1.2f; //scale when dragging card
    public float dragScaleDuration = 1.0f; //duration to return scale after drag
    public float maxRotationZ = 10f; //max z rotation during drag

    [Header("Card State")]
    private Card card; //reference to card data
    private Vector3 positionTarget; //target local position for smooth movement
    private Tween moveTween; //tween for position movement
    private Tween scaleTween; //tween for scaling effects
    private Tween shakeTween; //tween for shake effect
    private Tween punchTween; //tween for punch effect
    private float previousX; //previous x position for rotation calculation
    private bool isDraggingVisual = false; //flag to block hover scale while dragging

    [Header("Hover Rotation Settings")]
    public float hoverTiltAmount = 5f; //amount of tilt when hovered
    public float tiltLerpSpeed = 8f; //speed of tilt lerp
    public float idleTiltSpeed = 2f; //speed of idle tilt
    public float idleTiltMagnitude = 2f; //magnitude of idle tilt
    private bool isHovered = false; //flag to check if card is hovered
    private int savedIndex; //random index for idle tilt effect

    [Header("Interaction References")]
    private CardInteraction cachedInteraction; //cached reference to CardInteraction component
    private CardLayoutManager cachedLayout; //cached reference to CardLayoutManager component
    public UnityEngine.UI.Image artworkImage; //reference to card artwork image

    private void Awake()
    {
        positionTarget = transform.localPosition; //initialize target position to current

        if (scaleTarget == null)
        {
            scaleTarget = transform.parent != null ? transform.parent : transform; //fallback
            Debug.Log($"[CardVisual] scaleTarget not assigned, defaulting to: {scaleTarget.name}");
        }
        else
        {
            Debug.Log($"[CardVisual] scaleTarget assigned: {scaleTarget.name}");
        }

        savedIndex = Random.Range(0, 100); //save random index for idle tilt
    }

    private void Update()
    {
        HandleRotationEffect();
    }

    //setup visuals from data
    public void Setup(Card cardData)
    {
        card = cardData;
        nameText.text = card.Name;
        descriptionText.text = card.Description;
        costText.text = card.EnergyCost.ToString();

        if (artworkImage != null && card.Artwork != null)
        {
            artworkImage.sprite = card.Artwork;
        }
    }

    //move card to a new local position with tween
    public void MoveToPosition(Vector3 newPosition)
    {
        if (!allowPositionInterpolation)
        {
            moveTween?.Kill();
            transform.localPosition = newPosition;
            positionTarget = newPosition;
            return;
        }

        positionTarget = newPosition;
        moveTween?.Kill();

        moveTween = transform.DOLocalMove(positionTarget, moveDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => positionTarget = transform.localPosition);
    }

    //select visual feedback
    public void SelectVisual()
    {
        KillAllTweens();
        punchTween = transform.DOPunchPosition(Vector3.up * punchStrength, punchDuration, punchVibrato, punchElasticity);
        scaleTween = scaleTarget.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack);
    }

    //deselect visual feedback
    public void DeselectVisual()
    {
        KillAllTweens();
        punchTween = transform.DOPunchPosition(Vector3.up * punchStrength, punchDuration, punchVibrato, punchElasticity);
        scaleTween = scaleTarget.DOScale(1f, 0.2f).SetEase(Ease.InOutSine);
    }

    //hover enter event
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;

        if (!IsSelected() && !isDraggingVisual)
        {
            KillScaleAndShakeTweens();
            scaleTween = scaleTarget.DOScale(hoverScale, hoverDuration).SetEase(Ease.OutSine);
            shakeTween = transform.DOShakePosition(hoverShakeDuration, hoverShakeStrength, hoverShakeVibrato, hoverShakeRandomness, fadeOut: true);
        }
    }

    //hover exit event
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;

        if (!IsSelected() && !isDraggingVisual)
        {
            KillScaleAndShakeTweens();
            scaleTween = scaleTarget.DOScale(1f, hoverDuration).SetEase(Ease.OutSine);
            AnimateMoveToLocalPosition(positionTarget);
        }
    }

    //begin dragging
    public void OnBeginDragVisual()
    {
        isDraggingVisual = true;
        scaleTween?.Kill();
        scaleTween = scaleTarget.DOScale(dragScale, 0.2f).SetEase(Ease.OutBack);
    }

    //on dragging (unused)
    public void OnDragVisual() { }

    //end dragging
    public void OnEndDragVisual()
    {
        isDraggingVisual = false;
        scaleTween?.Kill();
        scaleTween = scaleTarget.DOScale(1f, dragScaleDuration).SetEase(Ease.InOutSine);
    }

    //check if this card is selected
    private bool IsSelected()
    {
        if (cachedInteraction == null)
            cachedInteraction = GetComponentInParent<CardInteraction>();

        if (cachedLayout == null && cachedInteraction != null)
            cachedLayout = cachedInteraction.GetComponentInParent<CardLayoutManager>();

        return cachedLayout != null && cachedLayout.IsSelected(cachedInteraction);
    }

    //kill all tweens
    public void KillAllTweens()
    {
        scaleTween?.Kill();
        shakeTween?.Kill();
        punchTween?.Kill();
    }

    //kill only scale and shake
    private void KillScaleAndShakeTweens()
    {
        scaleTween?.Kill();
        shakeTween?.Kill();
    }

    //cleanup
    private void OnDestroy()
    {
        KillAllTweens();
    }

    //handle rotation effect based on hover state
    //this method applies a tilt effect when hovered or selected
    private void HandleRotationEffect()
    {
        if (isDraggingVisual) return;

        if ((isHovered && !IsSelected()) || (IsSelected() && isHovered))
        {
            Camera cam = Camera.main;
            Vector3 screenPos = Input.mousePosition;

            float zDepth = Vector3.Distance(cam.transform.position, transform.position);
            screenPos.z = zDepth;

            Vector3 mouseWorld = cam.ScreenToWorldPoint(screenPos);
            Vector3 offset = mouseWorld - transform.position;

            float tiltX = Mathf.Clamp(offset.y * -hoverTiltAmount, -10f, 10f);
            float tiltY = Mathf.Clamp(offset.x * hoverTiltAmount, -10f, 10f);

            Quaternion targetRot = Quaternion.Euler(tiltX, tiltY, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * tiltLerpSpeed);
        }
        else
        {
            float angleZ = Mathf.Sin(Time.time * idleTiltSpeed + savedIndex) * idleTiltMagnitude;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, angleZ);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * tiltLerpSpeed);
        }
    }
    
    //animate move to a new local position
    //this method uses a tween to smoothly move the card to the target position
    public void AnimateMoveToLocalPosition(Vector3 targetPosition)
    {
        moveTween?.Kill();
        positionTarget = targetPosition;
        moveTween = transform.DOLocalMove(targetPosition, 0.3f).SetEase(Ease.OutCubic);
    }
}