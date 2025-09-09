using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardVisual : MonoBehaviour
{
    [Header("Text and Image")]
    public Image artworkImage;

    [Header("Smooth and rotate")]
    private Transform target;
    private Vector3 positionOffset = Vector3.zero;
    private Vector3 lastPosition;
    private Vector3 smoothedVelocity;

    [Header("Dotween vars")]
    [SerializeField] private float shakeDur = 0.1f;
    [SerializeField] private float shakeStr = 0.1f;
    [SerializeField] private int shakeVib = 1;
    [SerializeField] private float shakeRand = 0.1f;

    [Header("Sorting layers")]
    public Canvas cardCanvas;
    public int originalSortingOrder;

    [Header("Hover rotation")]
    [SerializeField] private float maxTiltZ = 10f;
    [SerializeField] private float maxTiltX = 10f;
    [SerializeField] private float rotationSmoothSpeed = 8f;
    private Quaternion targetRotation;
    private bool isHovered = false;
    private bool isSelected = false;
    private Vector2 hoverMousePosition = Vector2.zero;
    private Vector2 idleVirtualMousePos = Vector2.zero;
    [SerializeField] private float idleOscillationSpeed = 1f;
    [SerializeField] private float idleOscillationAmplitude = 0.5f;
    [SerializeField] private float scaleRaise = 1.3f;

    private void Awake()
    {
        cardCanvas = GetComponent<Canvas>(); 
        if (cardCanvas != null)
            cardCanvas.overrideSorting = true; //enable override sorting for this canvas

        targetRotation = transform.rotation; //store initial rotation
    }

    public void SetCard(Card card)
    {
        if (card == null || artworkImage == null) return;

        artworkImage.sprite = card.Artwork; //set artwork sprite from card data
    }

    public void SetHoverState(bool isOver)
    {
        isHovered = isOver; //update hover state
        if (!isOver)
            hoverMousePosition = Vector2.zero; //reset hover mouse position when not hovering
    }

    public void SetSelectedState(bool selected)
    {
        isSelected = selected; //update selected state
        if (selected == false)
        {
            isHovered = false; //clear hover when deselected
            hoverMousePosition = Vector2.zero;
        }
    }

    public void UpdateHoverMousePosition(Vector2 normalizedLocalPos)
    {
        hoverMousePosition = normalizedLocalPos; //update hover position for rotation effect
        isHovered = true;
    }

    public void SetFollowTarget(Transform t)
    {
        target = t; //assign the transform to follow
        lastPosition = t.position; //initialize last position for velocity calc
    }

    private void Update()
    {
        if (target == null) return; //exit if no target assigned

        Vector3 targetPos = target.position + positionOffset;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 20f); //smooth follow position

        Vector3 movement = (target.position - lastPosition) / Time.deltaTime; //calculate movement velocity
        smoothedVelocity = Vector3.Lerp(smoothedVelocity, movement, Time.deltaTime * 10f); //smooth velocity
        lastPosition = target.position;

        //apply rotation based on smoothed movement velocity with multiplier for x tilt
        float tiltX = Mathf.Clamp(-smoothedVelocity.y * 0.5f, -maxTiltX, maxTiltX);
        float tiltY = Mathf.Clamp(smoothedVelocity.x * 0.1f, -maxTiltX, maxTiltX);
        float tiltZ = Mathf.Clamp(-smoothedVelocity.x * 0.2f, -maxTiltZ, maxTiltZ);

        float extraX = 0f;
        float extraZ = 0f;

        if (isHovered)
        {
            (extraX, extraZ) = GetHoverRotationOffsets(); //get extra rotation from hover position
        }
        else
        {
            (extraX, extraZ) = GetIdleRotationOffsets(); //get idle rotation offsets for subtle movement
        }

        targetRotation = Quaternion.Euler(tiltX + extraX, tiltY, tiltZ + extraZ); //combine rotations
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothSpeed); //smoothly interpolate rotation
    }

    public (float extraX, float extraZ) GetHoverRotationOffsets()
    {
        float normX = Mathf.Clamp(hoverMousePosition.x, -0.5f, 0.5f) * 2f;
        float normY = Mathf.Clamp(hoverMousePosition.y, -0.5f, 0.5f) * 2f;

        float extraZ = -normX * maxTiltZ; //tilt around z axis based on mouse x pos
        float extraX = normY * maxTiltX;  //tilt around x axis based on mouse y pos

        return (extraX, extraZ);
    }

    public (float extraX, float extraZ) GetIdleRotationOffsets()
    {
        idleVirtualMousePos.x = Mathf.Sin(Time.time * idleOscillationSpeed) * idleOscillationAmplitude; //idle oscillation x
        idleVirtualMousePos.y = Mathf.Cos(Time.time * idleOscillationSpeed * 0.7f) * idleOscillationAmplitude; //idle oscillation y

        float normX = Mathf.Clamp(idleVirtualMousePos.x, -0.5f, 0.5f) * 2f;
        float normY = Mathf.Clamp(idleVirtualMousePos.y, -0.5f, 0.5f) * 2f;

        float extraZ = -normX * maxTiltZ; //idle tilt z
        float extraX = normY * maxTiltX;  //idle tilt x

        return (extraX, extraZ);
    }

    public void SetOriginalSortingOrder(int order)
    {
        originalSortingOrder = order; //store original sorting order
        if (cardCanvas != null)
            cardCanvas.sortingOrder = order; //apply sorting order to canvas

    }

    public void ResetSortingOrder(int newOrder)
    {
        originalSortingOrder = newOrder; //update original sorting order
        if (cardCanvas != null)
            cardCanvas.sortingOrder = originalSortingOrder; //reset canvas sorting order
    }

    public void SetSortingOrder(int order)
    {
        originalSortingOrder = order; //set sorting order explicitly
        if (cardCanvas != null)
            cardCanvas.sortingOrder = order; //apply sorting order
    }

    public void ReturnSortingLayer()
    {
        cardCanvas.sortingOrder = originalSortingOrder; //restore original sorting order
    }

    public void PlayPoniterEnter()
    {
        transform.DOKill(); //kill any running tweens
        transform.localScale = Vector3.one;
        transform.DOScale(1.05f, 0.3f).SetEase(Ease.OutBack); //scale up on pointer enter
        transform.DOShakePosition(0.1f, 10f, 20, 0); //shake effect
    }

    public void PlayPointerExit()
    {
        transform.DOKill(); //kill tweens on exit
        transform.DOScale(1.0f, 0.3f).SetEase(Ease.OutBack); //scale back to normal
    }

    public void SelectVisual()
    {
        transform.DOScale(scaleRaise, 0.3f).SetEase(Ease.OutBack); //scale up on select
        transform.DOShakePosition(shakeDur, shakeStr, shakeVib, shakeRand); //shake effect on select
    }

    public void DeselectVisual()
    {
        transform.DOScale(1.0f, 0.3f).SetEase(Ease.OutBack); //scale back on deselect
        transform.DOShakePosition(shakeDur, shakeStr, shakeVib, shakeRand); //shake effect on deselect
    }

    public void IndragTweens()
    {
        transform.DOScale(scaleRaise, 0.3f).SetEase(Ease.OutBack); //scale up while dragging
    }

    public void OffDragTweens()
    {
        transform.DOScale(1.0f, 0.3f).SetEase(Ease.OutBack); //scale back when drag ends
    }
}