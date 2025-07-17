using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CardInteraction : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    private Canvas canvas;                      //canvas reference for drag parenting
    private CanvasGroup canvasGroup;            //canvasGroup for blocking raycasts during drag
    private RectTransform rectTransform;        //rectTransform of the card
    private Transform originalParent;           //original parent to restore after drag

    public CardVisual cardVisual;               //reference to CardVisual component
    private CardLayoutManager layoutManager;   //reference to layout manager handling cards

    [Header("Smooth Movement")]
    private Coroutine moveCoroutine;            //coroutine for smooth move
    public float moveSpeed = 1000f;             //speed of smooth move

    [Header("Drag Control")]
    private Vector3 targetDragPosition;         //target position during drag
    private Vector3 velocity = Vector3.zero;    //velocity used in SmoothDamp
    public float followSmoothTime = 0.05f;      //smooth time for follow movement
    private bool isDragging = false;             //flag if card is being dragged

    [Header("Rotation Control")]
    public float rotationMultiplier = 0.1f;     //multiplier for rotation based on drag delta
    public float rotationReturnSpeed = 10f;     //speed to return rotation to original
    private Quaternion originalRotation;        //original rotation to reset after drag

    [Header("Sorting Order")]
    private Canvas cardCanvas;                   //canvas for sorting order control
    private int originalSortingOrder;            //store original sorting order

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>(); //get rectTransform
        canvasGroup = GetComponent<CanvasGroup>();     //get or add canvasGroup
        canvas = GetComponentInParent<Canvas>();        //get parent canvas
        layoutManager = GetComponentInParent<CardLayoutManager>(); //get parent layout manager

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        cardCanvas = GetComponent<Canvas>();             //get or add canvas for sorting
        if (cardCanvas == null)
        {
            cardCanvas = gameObject.AddComponent<Canvas>();
            cardCanvas.overrideSorting = true;
        }

        originalSortingOrder = cardCanvas.sortingOrder;  //store original sorting order
        originalRotation = transform.localRotation;      //store original rotation
    }

    private void Update()
    {
        if (isDragging)
        {
            //smoothly move towards target drag position
            transform.localPosition = Vector3.SmoothDamp(
                transform.localPosition,
                targetDragPosition,
                ref velocity,
                followSmoothTime
            );

            //rotation during drag based on horizontal delta
            float angle = (targetDragPosition.x - transform.localPosition.x) * rotationMultiplier;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * 10f);
        }
        else
        {
            //smoothly return rotation to original
            transform.localRotation = Quaternion.Slerp(transform.localRotation, originalRotation, Time.deltaTime * rotationReturnSpeed);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (layoutManager != null)
        {
            //toggle selection on click
            if (layoutManager.IsSelected(this))
                layoutManager.DeselectCard();
            else
                layoutManager.SelectCard(this);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;    //store original parent
        canvasGroup.blocksRaycasts = false;   //disable blocking raycasts to allow drop
        transform.SetParent(canvas.transform, true); //move to top-level canvas for dragging
        isDragging = true;

        cardVisual?.OnBeginDragVisual();      //notify visual about drag start
    }

    public void OnDrag(PointerEventData eventData)
    {
        //convert screen point to local point in canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 pos
        );

        targetDragPosition = pos;              //set target drag position
        layoutManager?.SimulateDrag(this, targetDragPosition.x); //simulate drag effect on layout
        cardVisual?.OnBeginDragVisual();      //notify visual about ongoing drag
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;    //enable raycasts again
        transform.SetParent(originalParent, true); //restore original parent

        //apply slight rotation on release based on last delta
        float angle = (targetDragPosition.x - transform.localPosition.x) * rotationMultiplier * 1.5f;
        transform.localRotation = Quaternion.Euler(0f, 0f, angle);

        isDragging = false;
        layoutManager?.ReorderCard(this, transform.localPosition.x); //reorder cards in layout
        cardVisual?.OnEndDragVisual();           //notify visual about drag end

        targetDragPosition = transform.localPosition; //reset target position
    }

    public void MoveToLocalPosition(Vector3 targetPos)
    {
        //start smooth move coroutine to target position
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveRoutine(targetPos));
    }

    public void SetLocalPositionInstant(Vector3 pos)
    {
        //stop movement coroutine and set position instantly
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        transform.localPosition = pos;
    }

    private IEnumerator MoveRoutine(Vector3 endPos)
    {
        while (Vector3.Distance(transform.localPosition, endPos) > 0.01f)
        {
            //move towards target position
            Vector3 direction = endPos - transform.localPosition;
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, endPos, moveSpeed * Time.deltaTime);

            //apply rotation based on movement direction
            float angleZ = Mathf.Clamp((endPos.x - transform.localPosition.x) * rotationMultiplier * 15f, -20f, 20f);
            Quaternion targetRotation = Quaternion.Euler(0, 0, angleZ);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * 8f);

            yield return null;
        }

        //snap to final position
        transform.localPosition = endPos;

        //smoothly return to original rotation
        float t = 0f;
        while (Quaternion.Angle(transform.localRotation, originalRotation) > 0.5f)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, originalRotation, t);
            t += Time.deltaTime * 4f;
            yield return null;
        }

        transform.localRotation = originalRotation;
        moveCoroutine = null;
    }

    public void SetSortingOrder(int order)
    {
        cardCanvas.sortingOrder = order;   //set sorting order of the card canvas
    }

    public void RestoreOriginalSortingOrder()
    {
        cardCanvas.sortingOrder = originalSortingOrder; //restore to original sorting order
    }

    public void SetOriginalSortingOrder(int order)
    {
        originalSortingOrder = order;     //set the stored original sorting order
    }
}
