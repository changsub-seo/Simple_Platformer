using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("슬롯 부위 설정")]
    public EquipmentType slotType; 

    [Header("인스펙터에서 연결한 아이콘 이미지")]
    public Image itemIcon;         

    [HideInInspector] 
    public GeneratedItem equippedItem;  

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void EquipItem(GeneratedItem newItem)
    {
        EquipItemWithAlpha(newItem, 1.0f);
    }

    public void EquipItemWithAlpha(GeneratedItem newItem, float alpha)
    {
        equippedItem = newItem;
        
        if (newItem.baseItem != null && newItem.baseItem.loadedIcon != null)
        {
            if (itemIcon != null)
            {
                itemIcon.sprite = newItem.baseItem.loadedIcon;  
                itemIcon.color = new Color(1, 1, 1, alpha); 
                itemIcon.enabled = true;
                itemIcon.preserveAspect = true; 
                itemIcon.raycastTarget = true; 
            }
        }
    }

    public void UnequipItem()
    {
        if (equippedItem == null || string.IsNullOrEmpty(equippedItem.finalName)) return; 

        equippedItem = null;
        if (itemIcon != null)
        {
            itemIcon.sprite = null; 
            itemIcon.color = new Color(1, 1, 1, 0); 
            itemIcon.enabled = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (equippedItem != null && !string.IsNullOrEmpty(equippedItem.finalName))
        {
            InventoryManager.instance.ShowTooltip(equippedItem, rectTransform);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.HideTooltip();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && equippedItem != null && !string.IsNullOrEmpty(equippedItem.finalName))
        {
            if (InventoryManager.instance != null)
            {
                InventoryManager.instance.HideTooltip();
            }

            EquipmentManager mgr = EquipmentManager.instance;
            if (mgr != null)
            {
                // ⭐ 1. 한벌옷 세트 해제 처리 (상의나 하의 어디를 우클릭해도 둘 다 세트로 해제)
                if (slotType == EquipmentType.Top || slotType == EquipmentType.Bottom)
                {
                    EquipSlot topS = null;
                    EquipSlot botS = null;
                    foreach (EquipSlot s in mgr.allEquipSlots)
                    {
                        if (s.slotType == EquipmentType.Top) topS = s;
                        if (s.slotType == EquipmentType.Bottom) botS = s;
                    }

                    if (topS != null && botS != null && topS.equippedItem != null && botS.equippedItem != null && (topS.equippedItem == botS.equippedItem))
                    {
                        bool added = InventoryManager.instance.AddItemToInventory(topS.equippedItem);
                        if (!added)
                        {
                            Debug.LogWarning("인벤토리가 꽉 찼습니다!");
                            return;
                        }
                        topS.UnequipItem();
                        botS.UnequipItem();
                        return;
                    }
                }

                // ⭐ 2. 양손무기 세트 해제 처리 (무기1 혹은 무기2 어디를 우클릭해도 세트로 해제)
                if (slotType == EquipmentType.OneHandWeapon || slotType == EquipmentType.TwoHandWeapon)
                {
                    EquipSlot w1 = null;
                    EquipSlot w2 = null;
                    foreach (EquipSlot s in mgr.allEquipSlots)
                    {
                        if (s.slotType == EquipmentType.OneHandWeapon || s.slotType == EquipmentType.TwoHandWeapon)
                        {
                            if (w1 == null) w1 = s;
                            else if (w2 == null) w2 = s;
                        }
                    }

                    if (w1 != null && w2 != null && w1.equippedItem != null && w2.equippedItem != null && (w1.equippedItem == w2.equippedItem))
                    {
                        bool added = InventoryManager.instance.AddItemToInventory(w1.equippedItem);
                        if (!added)
                        {
                            Debug.LogWarning("인벤토리가 꽉 찼습니다!");
                            return;
                        }
                        w1.UnequipItem();
                        w2.UnequipItem();
                        return;
                    }
                }
            }

            // 일반 장비 또는 단독 장비 해제
            bool isAdded = InventoryManager.instance.AddItemToInventory(equippedItem);
            if (!isAdded)
            {
                Debug.LogWarning("인벤토리가 꽉 차서 장비를 해제할 수 없습니다!");
                return;
            }

            UnequipItem(); 
        }
    }
}

public enum EquipmentType
{
    Head,           
    Neck,           
    Ring,           
    Gloves,         
    Waist,          
    Top,            
    Bottom,         
    Overall,        
    Wrist,          
    Shoes,          
    OneHandWeapon,  
    TwoHandWeapon   
}