using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; 

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage; 
    public GeneratedItem currentItem; 

    void Awake()
    {
        if (iconImage == null)
        {
            iconImage = GetComponent<Image>();
        }
    }

    void Start()
    {
        if (currentItem == null)
        {
            ClearSlot();
        }
    }

    public void AddItem(GeneratedItem newItem)
    {
        currentItem = newItem;
        
        if (iconImage != null)
        {
            if (newItem.baseItem.loadedIcon != null)
            {
                // 정상적으로 아이콘을 찾았을 때
                iconImage.sprite = newItem.baseItem.loadedIcon;
                iconImage.color = new Color(1, 1, 1, 1); 
            }
            else
            {
                // 🚨 아이콘을 찾지 못했을 때 눈에 띄게 빨간색으로 표시하고 원인 출력
                iconImage.sprite = null;
                iconImage.color = new Color(1, 0, 0, 0.5f); 
                Debug.LogWarning($"[{newItem.finalName}] 아이템의 아이콘을 찾지 못했습니다! CSV의 장착 부위 이름: '{newItem.baseItem.equipSlot}'");
            }
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.color = new Color(1, 1, 1, 0); 
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null && InventoryManager.instance != null)
        {
            InventoryManager.instance.ShowTooltip(currentItem, transform.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.HideTooltip();
        }
    }
}