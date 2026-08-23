using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    public Image iconImage;
    public GeneratedItem currentItem;
    private RectTransform rectTransform;

    [Header("기본/특수 아이콘 설정")]
    public Sprite defaultIcon; 

    private static GameObject draggingIcon;
    public static bool isDraggingItem = false; 

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        if (iconImage == null)
        {
            iconImage = transform.Find("ItemIcon")?.GetComponent<Image>();
        }
        UpdateSlotUI();
    }

    public void AddItem(GeneratedItem newItem)
    {
        currentItem = newItem;
        UpdateSlotUI();
    }

    public void ClearSlot()
    {
        currentItem = null;
        UpdateSlotUI();
    }

    public void UpdateSlotUI()
    {
        if (iconImage != null)
        {
            if (currentItem == null || string.IsNullOrEmpty(currentItem.finalName))
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }
            else if (currentItem.baseItem == null || currentItem.baseItem.loadedIcon == null)
            {
                if (defaultIcon != null)
                {
                    iconImage.sprite = defaultIcon;
                    iconImage.enabled = true;
                    iconImage.color = Color.white;
                }
                else
                {
                    iconImage.sprite = null;
                    iconImage.enabled = false;
                }
            }
            else
            {
                iconImage.sprite = currentItem.baseItem.loadedIcon;
                iconImage.enabled = true;
                iconImage.color = Color.white;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDraggingItem) return;

        if (currentItem != null && !string.IsNullOrEmpty(currentItem.finalName))
        {
            InventoryManager.instance.ShowTooltip(currentItem, rectTransform);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InventoryManager.instance.HideTooltip();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (currentItem == null || string.IsNullOrEmpty(currentItem.finalName)) return;

        if (draggingIcon != null)
        {
            Destroy(draggingIcon);
        }

        isDraggingItem = true;
        InventoryManager.instance.HideTooltip();

        draggingIcon = new GameObject("DraggingIcon");
        draggingIcon.transform.SetParent(InventoryManager.instance.inventoryPanel.transform.root, false);
        CanvasGroup canvasGroup = draggingIcon.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;

        Image img = draggingIcon.AddComponent<Image>();
        img.sprite = iconImage.sprite;
        img.rectTransform.sizeDelta = new Vector2(rectTransform.rect.width, rectTransform.rect.height);

        iconImage.color = new Color(1, 1, 1, 0.4f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (draggingIcon != null)
        {
            Vector3 globalMousePos;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                draggingIcon.GetComponent<RectTransform>(), 
                eventData.position, 
                eventData.pressEventCamera, 
                out globalMousePos))
            {
                draggingIcon.transform.position = globalMousePos;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        isDraggingItem = false;

        if (draggingIcon != null)
        {
            Destroy(draggingIcon);
        }
        UpdateSlotUI();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        InventorySlot sourceSlot = eventData.pointerDrag?.GetComponent<InventorySlot>();
        if (sourceSlot == null || sourceSlot == this) return;

        GeneratedItem tempItem = this.currentItem;
        this.currentItem = sourceSlot.currentItem;
        sourceSlot.currentItem = tempItem;

        this.UpdateSlotUI();
        sourceSlot.UpdateSlotUI();

        if (this.currentItem != null && !string.IsNullOrEmpty(this.currentItem.finalName))
        {
            InventoryManager.instance.ShowTooltip(this.currentItem, this.rectTransform);
        }
        else
        {
            InventoryManager.instance.HideTooltip();
        }
    }

    // ⭐ 안전장치(Null 체크)가 완벽하게 추가된 우클릭 함수
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (currentItem != null && currentItem.baseItem != null)
            {
                // itemType이 비어있지 않은지 먼저 검사합니다.
                if (!string.IsNullOrEmpty(currentItem.baseItem.itemType) && 
                    currentItem.baseItem.itemType.Trim() == "Equipment")
                {
                    bool isEquipped = EquipmentManager.instance.EquipFromInventory(currentItem);
                    if (isEquipped)
                    {
                        ClearSlot();
                        InventoryManager.instance.HideTooltip();
                    }
                }
                else
                {
                    Debug.Log("이 아이템은 장비 타입이 아니거나 데이터가 누락되어 있습니다!");
                }
            }
        }
    }
}