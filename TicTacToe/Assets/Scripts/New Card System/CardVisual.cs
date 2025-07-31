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
    private int originalSortingOrder;

    [Header("Balatro Hover Rotation")]
    [SerializeField] private float hoverZAmplitude = 5f;
    [SerializeField] private float hoverZSpeed = 3f;
    [SerializeField] private int savedIndex = 0;
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

    private void Awake()
    {
        cardCanvas = GetComponent<Canvas>();
        if (cardCanvas != null)
            cardCanvas.overrideSorting = true;

        targetRotation = transform.rotation;
    }

    public void SetCard(Card card)
    {
        if (card == null || artworkImage == null) return;

        artworkImage.sprite = card.Artwork;
    }

    public void SetHoverState(bool isOver)
    {
        isHovered = isOver;
        if (!isOver)
            hoverMousePosition = Vector2.zero;
    }

    public void SetSelectedState(bool selected)
    {
        isSelected = selected;
        if (selected == false)
        {
            isHovered = false;
            hoverMousePosition = Vector2.zero;
        }
    }

    public void UpdateHoverMousePosition(Vector2 normalizedLocalPos)
    {
        hoverMousePosition = normalizedLocalPos;
        isHovered = true;
    }

    public void SetFollowTarget(Transform t)
    {
        target = t;
        lastPosition = t.position;
    }

    private void Update()
    {
        if (target == null) return;

        Vector3 targetPos = target.position + positionOffset;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 20f);

        Vector3 movement = (target.position - lastPosition) / Time.deltaTime;
        smoothedVelocity = Vector3.Lerp(smoothedVelocity, movement, Time.deltaTime * 10f);
        lastPosition = target.position;

        float tiltX = Mathf.Clamp(-smoothedVelocity.y * 0.1f, -10f, 10f);
        float tiltY = Mathf.Clamp(smoothedVelocity.x * 0.1f, -10f, 10f);
        float tiltZ = Mathf.Clamp(-smoothedVelocity.x * 0.2f, -15f, 15f);

        float extraX = 0f;
        float extraZ = 0f;

        if (isHovered)
        {
            (extraX, extraZ) = GetHoverRotationOffsets();
        }
        else
        {
            (extraX, extraZ) = GetIdleRotationOffsets();
        }

        targetRotation = Quaternion.Euler(tiltX + extraX, tiltY, tiltZ + extraZ);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothSpeed);
    }

    public (float extraX, float extraZ) GetHoverRotationOffsets()
    {
        float normX = Mathf.Clamp(hoverMousePosition.x, -0.5f, 0.5f) * 2f;
        float normY = Mathf.Clamp(hoverMousePosition.y, -0.5f, 0.5f) * 2f;

        float extraZ = -normX * maxTiltZ;
        float extraX = normY * maxTiltX;

        return (extraX, extraZ);
    }

    public (float extraX, float extraZ) GetIdleRotationOffsets()
    {
        idleVirtualMousePos.x = Mathf.Sin(Time.time * idleOscillationSpeed) * idleOscillationAmplitude;
        idleVirtualMousePos.y = Mathf.Cos(Time.time * idleOscillationSpeed * 0.7f) * idleOscillationAmplitude;

        float normX = Mathf.Clamp(idleVirtualMousePos.x, -0.5f, 0.5f) * 2f;
        float normY = Mathf.Clamp(idleVirtualMousePos.y, -0.5f, 0.5f) * 2f;

        float extraZ = -normX * maxTiltZ;
        float extraX = normY * maxTiltX;

        return (extraX, extraZ);
    }

    public void SetOriginalSortingOrder(int order)
    {
        originalSortingOrder = order;
        if (cardCanvas != null)
            cardCanvas.sortingOrder = order;
    }

    public void ResetSortingOrder(int newOrder)
    {
        originalSortingOrder = newOrder;
        if (cardCanvas != null)
            cardCanvas.sortingOrder = originalSortingOrder;
    }

    public void SetSortingOrder(int order)
    {
        originalSortingOrder = order;
        if (cardCanvas != null)
            cardCanvas.sortingOrder = order;
    }

    public void ReturnSortingLayer()
    {
        cardCanvas.sortingOrder = originalSortingOrder;
    }

    public void PlayPoniterEnter()
    {
        transform.DOKill();
        transform.localScale = Vector3.one;
        transform.DOScale(1.1f, 0.3f).SetEase(Ease.OutBack);
        transform.DOShakePosition(0.1f, 10f, 20, 0);
    }

    public void PlayPointerExit()
    {
        transform.DOKill();
        transform.DOScale(1.0f, 0.3f).SetEase(Ease.OutBack);
    }

    public void SelectVisual()
    {
        transform.DOScale(1.3f, 0.3f).SetEase(Ease.OutBack);
        transform.DOShakePosition(shakeDur, shakeStr, shakeVib, shakeRand);
    }

    public void DeselectVisual()
    {
        transform.DOScale(1.0f, 0.3f).SetEase(Ease.OutBack);
        transform.DOShakePosition(shakeDur, shakeStr, shakeVib, shakeRand);
    }

    public void IndragTweens()
    {
        transform.DOScale(1.5f, 0.3f).SetEase(Ease.OutBack);
    }

    public void OffDragTweens()
    {
        transform.DOScale(1.0f, 0.3f).SetEase(Ease.OutBack);
    }

}