using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class PuzzlePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private int pieceId;
    private PuzzleManager manager;
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;

    // 新增：轮廓组件
    private Outline outline;

    public void Init(int id, PuzzleManager mgr)
    {
        pieceId = id;
        manager = mgr;
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvas = GetComponentInParent<Canvas>();
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;

        // 添加 Outline 组件（如果还没有）
        outline = GetComponent<Outline>();
        if (outline == null)
            outline = gameObject.AddComponent<Outline>();

        // 设置 Outline 效果
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(3, 3);
        outline.effectColor = new Color(1, 1, 1, 0);  // 初始透明
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.8f;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(canvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        Slot slot = null;

        if (eventData.pointerCurrentRaycast.isValid)
        {
            GameObject target = eventData.pointerCurrentRaycast.gameObject;
            slot = target.GetComponent<Slot>();
            if (slot == null && target.transform.parent != null)
                slot = target.transform.parent.GetComponent<Slot>();
        }

        if (slot != null && !slot.isOccupied)
        {
            bool isCorrect = manager.TryPlacePiece(pieceId, slot.slotId);

            if (isCorrect)
            {
                transform.SetParent(slot.transform);
                rectTransform.localPosition = Vector3.zero;
                rectTransform.localScale = Vector3.one;
                canvasGroup.blocksRaycasts = false;
                slot.SetOccupied(true);
                StartCoroutine(FlashOutline(Color.green));
                return;
            }
            else
            {
                StartCoroutine(FlashOutline(Color.red));
                ReturnToOriginal();
                return;
            }
        }

        ReturnToOriginal();
    }

    // 闪烁轮廓（只改变外边框颜色，不变图片）
    IEnumerator FlashOutline(Color targetColor)
    {
        if (outline == null) yield break;

        // 保存原始颜色
        Color originalColor = outline.effectColor;

        // 设置轮廓颜色（完全不透明）
        outline.effectColor = targetColor;
        outline.effectDistance = new Vector2(4, 4);  // 加粗轮廓让它更明显

        // 等待0.3秒
        yield return new WaitForSeconds(0.3f);

        // 恢复透明
        outline.effectColor = new Color(1, 1, 1, 0);
    }

    void ReturnToOriginal()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
    }
}