using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class CardVisual : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Text References")]
    public TextMeshProUGUI nameText;          //text field for card name
    public TextMeshProUGUI descriptionText;   //text field for card description
    public TextMeshProUGUI costText;          //text field for card energy cost

    [Header("Movement")]
    public float moveDuration = 0.25f;         //duration for card movement tween
    [HideInInspector] public bool allowPositionInterpolation = true;  //flag to allow smooth move

    [Header("Hover Effect")]
    public Transform scaleTarget;              //transform to apply scale effects
    public float hoverScale = 1.05f;           //scale amount on hover
    public float hoverDuration = 0.15f;        //duration of hover scale tween

    [Header("Shake Settings")]
    public float hoverShakeDuration = 0.25f;  //duration of shake on hover
    public float hoverShakeStrength = 10f;    //strength of shake effect
    public int hoverShakeVibrato = 10;        //number of shakes
    public float hoverShakeRandomness = 90f;  //randomness angle of shake

    [Header("Punch Settings")]
    public float punchDuration = 0.3f;         //duration of punch effect
    public float punchStrength = 20f;          //strength of punch movement
    public int punchVibrato = 10;              //vibrato for punch tween
    public float punchElasticity = 1f;         //elasticity of punch tween

    [Header("Drag Effect")]
    public float dragScale = 1.2f;             //scale when dragging card
    public float dragScaleDuration = 1.0f;    //duration to return scale after drag
    public float maxRotationZ = 10f;           //max z rotation during drag

    private Card card;                         //reference to card data
    private Vector3 positionTarget;            //target local position for smooth movement
    private Tween moveTween;                   //tween for position movement
    private Tween scaleTween;                  //tween for scaling effects
    private Tween shakeTween;                  //tween for shake effect
    private Tween punchTween;                  //tween for punch effect
    private float previousX;                   //previous x position for rotation calculation

    private void Awake()
    {
        positionTarget = transform.localPosition; //initialize target position to current

        if (scaleTarget == null)
            scaleTarget = transform.parent;     //default scale target to parent if not assigned
    }

    public void Setup(Card cardData)
    {
        card = cardData;
        nameText.text = card.Name;              //set name text from card data
        descriptionText.text = card.Description; //set description text from card data
        costText.text = card.EnergyCost.ToString(); //set cost text from card data
    }

    public void MoveToPosition(Vector3 newPosition)
    {
        if (!allowPositionInterpolation)
        {
            moveTween?.Kill();                  //stop any existing move tween
            transform.localPosition = newPosition;  //set position instantly
            positionTarget = newPosition;       //update target position
            return;
        }

        positionTarget = newPosition;
        moveTween?.Kill();                      //kill existing move tween before starting new one
        moveTween = transform.DOLocalMove(positionTarget, moveDuration).SetEase(Ease.OutCubic); //tween to new position
    }

    public void SelectVisual()
    {
        KillAllTweens();                       //stop all ongoing tweens
        punchTween = transform.DOPunchPosition(Vector3.up * punchStrength, punchDuration, punchVibrato, punchElasticity); //punch up effect
        scaleTween = scaleTarget.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack); //scale up selected card
    }

    public void DeselectVisual()
    {
        KillAllTweens();                       //stop all ongoing tweens
        punchTween = transform.DOPunchPosition(Vector3.down * (punchStrength / 2f), punchDuration * 0.7f, punchVibrato, punchElasticity); //punch down effect
        scaleTween = scaleTarget.DOScale(1f, 0.2f).SetEase(Ease.InOutSine); //scale back to normal
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsSelected())
        {
            KillScaleAndShakeTweens();          //stop scale and shake tweens on hover start
            scaleTween = scaleTarget.DOScale(hoverScale, hoverDuration).SetEase(Ease.OutSine); //scale up slightly on hover
            shakeTween = transform.DOShakePosition(hoverShakeDuration, hoverShakeStrength, hoverShakeVibrato, hoverShakeRandomness); //shake effect on hover
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsSelected())
        {
            KillScaleAndShakeTweens();          //stop scale and shake tweens on hover end
            scaleTween = scaleTarget.DOScale(1f, hoverDuration).SetEase(Ease.OutSine); //scale back to normal
            transform.localPosition = positionTarget; //reset position to target to avoid shake offset
        }
    }

    public void OnBeginDragVisual()
    {
        scaleTween?.Kill();                     //kill any scale tween before drag scale
        scaleTarget.localScale = Vector3.one * dragScale; //scale card up on drag begin
        previousX = transform.position.x;      //store current x for rotation calc during drag
    }

    public void OnDragVisual()
    {
        float currentX = transform.position.x;
        float deltaX = currentX - previousX;   //calculate horizontal movement delta
        previousX = currentX;

        float targetRotZ = Mathf.Clamp(-deltaX * 2f, -maxRotationZ, maxRotationZ); //calculate z rotation based on deltaX
        scaleTarget.localRotation = Quaternion.Euler(0f, 0f, targetRotZ);          //apply rotation around z-axis during drag
    }

    public void OnEndDragVisual()
    {
        scaleTween?.Kill();                     //kill any scale tween before return
        scaleTarget.DOScale(1f, dragScaleDuration).SetEase(Ease.InOutSine); //scale back to normal over duration
        scaleTarget.DOLocalRotate(Vector3.zero, 0.2f).SetEase(Ease.OutCubic); //reset rotation smoothly
    }

    private bool IsSelected()
    {
        var interaction = GetComponentInParent<CardInteraction>();
        if (interaction == null) return false;

        var layout = interaction.GetComponentInParent<CardLayoutManager>();
        return layout != null && layout.IsSelected(interaction);  //return whether this card is currently selected
    }

    private void KillAllTweens()
    {
        scaleTween?.Kill();                     //stop scale tween if running
        shakeTween?.Kill();                     //stop shake tween if running
        punchTween?.Kill();                     //stop punch tween if running
    }

    private void KillScaleAndShakeTweens()
    {
        scaleTween?.Kill();                     //stop scale tween if running
        shakeTween?.Kill();                     //stop shake tween if running
    }
}
