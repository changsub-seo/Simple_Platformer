using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager instance;
    
    [Header("장비 슬롯 12개 연결")]
    public EquipSlot[] allEquipSlots; 

    void Awake()
    {
        if (instance == null) instance = this;
    }

    public bool EquipFromInventory(GeneratedItem itemToEquip) 
    {
        if (itemToEquip == null || itemToEquip.baseItem == null) return false;

        string parsedItemType = itemToEquip.baseItem.itemType.Trim();
        string parsedEquipSlot = itemToEquip.baseItem.equipSlot.Trim();

        if (parsedItemType != "Equipment") 
        {
            Debug.Log($"장착 실패: 아이템 타입이 장비가 아닙니다. (현재: '{parsedItemType}')");
            return false;
        }

        // 상/하의 슬롯 미리 찾기
        EquipSlot topSlot = null;
        EquipSlot bottomSlot = null;
        foreach (EquipSlot slot in allEquipSlots)
        {
            if (slot.slotType == EquipmentType.Top) topSlot = slot;
            if (slot.slotType == EquipmentType.Bottom) bottomSlot = slot;
        }

        // 1. 한벌옷 처리
        if (parsedEquipSlot.Equals("Overall", System.StringComparison.OrdinalIgnoreCase))
        {
            if (topSlot != null && bottomSlot != null)
            {
                bool hasTop = (topSlot.equippedItem != null && !string.IsNullOrEmpty(topSlot.equippedItem.finalName));
                bool hasBottom = (bottomSlot.equippedItem != null && !string.IsNullOrEmpty(bottomSlot.equippedItem.finalName));

                if (hasTop && hasBottom && (topSlot.equippedItem == bottomSlot.equippedItem)) return false;

                if (hasTop)
                {
                    if (!InventoryManager.instance.AddItemToInventory(topSlot.equippedItem)) return false;
                    topSlot.UnequipItem();
                }
                if (hasBottom)
                {
                    if (bottomSlot.equippedItem != topSlot.equippedItem)
                    {
                        if (!InventoryManager.instance.AddItemToInventory(bottomSlot.equippedItem)) return false;
                    }
                    bottomSlot.UnequipItem();
                }

                topSlot.EquipItemWithAlpha(itemToEquip, 1.0f);
                bottomSlot.EquipItemWithAlpha(itemToEquip, 0.4f);
                return true;
            }
            return false;
        }

        // 단독 상/하의 장착 시 한벌옷 찌꺼기 정리
        if (parsedEquipSlot.Equals("Top", System.StringComparison.OrdinalIgnoreCase) || parsedEquipSlot.Equals("Bottom", System.StringComparison.OrdinalIgnoreCase))
        {
            if (topSlot != null && bottomSlot != null)
            {
                if (topSlot.equippedItem != null && bottomSlot.equippedItem != null && (topSlot.equippedItem == bottomSlot.equippedItem))
                {
                    if (!InventoryManager.instance.AddItemToInventory(topSlot.equippedItem)) return false;
                    topSlot.UnequipItem();
                    bottomSlot.UnequipItem();
                }
            }
        }

        // 2. 반지 처리
        if (parsedEquipSlot.Equals("Ring", System.StringComparison.OrdinalIgnoreCase))
        {
            EquipSlot ring1 = null;
            EquipSlot ring2 = null;
            foreach (EquipSlot slot in allEquipSlots)
            {
                if (slot.slotType == EquipmentType.Ring)
                {
                    if (ring1 == null) ring1 = slot;
                    else if (ring2 == null) ring2 = slot;
                }
            }

            if (ring1 != null && ring2 != null)
            {
                EquipSlot targetRing = null;
                if (ring1.equippedItem == null || string.IsNullOrEmpty(ring1.equippedItem.finalName)) targetRing = ring1;
                else if (ring2.equippedItem == null || string.IsNullOrEmpty(ring2.equippedItem.finalName)) targetRing = ring2;
                else targetRing = ring1;

                if (targetRing.equippedItem != null && !string.IsNullOrEmpty(targetRing.equippedItem.finalName))
                {
                    if (!InventoryManager.instance.AddItemToInventory(targetRing.equippedItem)) return false;
                    targetRing.UnequipItem();
                }

                targetRing.EquipItemWithAlpha(itemToEquip, 1.0f);
                return true;
            }
        }

        // 무기 슬롯 수집
        EquipSlot weapon1 = null;
        EquipSlot weapon2 = null;
        foreach (EquipSlot slot in allEquipSlots)
        {
            if (slot.slotType == EquipmentType.OneHandWeapon || slot.slotType == EquipmentType.TwoHandWeapon)
            {
                if (weapon1 == null) weapon1 = slot;
                else if (weapon2 == null) weapon2 = slot;
            }
        }

        // ⭐ 3. 양손무기 처리 (잔상 및 복사 방지 로직 보완)
        if (parsedEquipSlot.Equals("TwoHandWeapon", System.StringComparison.OrdinalIgnoreCase))
        {
            if (weapon1 != null && weapon2 != null)
            {
                bool hasW1 = (weapon1.equippedItem != null && !string.IsNullOrEmpty(weapon1.equippedItem.finalName));
                bool hasW2 = (weapon2.equippedItem != null && !string.IsNullOrEmpty(weapon2.equippedItem.finalName));

                // 이미 같은 양손무기를 끼고 있다면 무시
                if (hasW1 && hasW2 && (weapon1.equippedItem == weapon2.equippedItem) && (weapon1.equippedItem.finalName == itemToEquip.finalName)) return false;

                if (hasW1)
                {
                    if (!InventoryManager.instance.AddItemToInventory(weapon1.equippedItem)) return false;
                    weapon1.UnequipItem();
                }
                if (hasW2)
                {
                    if (weapon2.equippedItem != weapon1.equippedItem)
                    {
                        if (!InventoryManager.instance.AddItemToInventory(weapon2.equippedItem)) return false;
                    }
                    weapon2.UnequipItem();
                }

                weapon1.EquipItemWithAlpha(itemToEquip, 1.0f);
                weapon2.EquipItemWithAlpha(itemToEquip, 0.4f);
                return true;
            }
            return false;
        }

        // ⭐ 4. 한손무기 처리 (양손무기 장착 중일 때 찌꺼기 세트 해제 포함)
        if (parsedEquipSlot.Equals("OneHandWeapon", System.StringComparison.OrdinalIgnoreCase))
        {
            if (weapon1 != null && weapon2 != null)
            {
                if (weapon1.equippedItem != null && weapon2.equippedItem != null && (weapon1.equippedItem == weapon2.equippedItem))
                {
                    if (!InventoryManager.instance.AddItemToInventory(weapon1.equippedItem)) return false;
                    weapon1.UnequipItem();
                    weapon2.UnequipItem();
                }

                EquipSlot targetWeapon = null;
                bool isW1Empty = (weapon1.equippedItem == null || string.IsNullOrEmpty(weapon1.equippedItem.finalName));
                bool isW2Empty = (weapon2.equippedItem == null || string.IsNullOrEmpty(weapon2.equippedItem.finalName));

                if (isW1Empty) targetWeapon = weapon1;
                else if (isW2Empty) targetWeapon = weapon2;
                else targetWeapon = weapon1;

                if (targetWeapon.equippedItem != null && !string.IsNullOrEmpty(targetWeapon.equippedItem.finalName))
                {
                    if (!InventoryManager.instance.AddItemToInventory(targetWeapon.equippedItem)) return false;
                    targetWeapon.UnequipItem();
                }

                targetWeapon.EquipItemWithAlpha(itemToEquip, 1.0f);
                return true;
            }
            return false;
        }

        // 5. 일반 장비 처리
        EquipSlot targetSlotGeneric = null;
        if (System.Enum.TryParse(parsedEquipSlot, true, out EquipmentType targetType))
        {
            foreach (EquipSlot slot in allEquipSlots)
            {
                if (slot.slotType == targetType) { targetSlotGeneric = slot; break; }
            }
        }
        else
        {
            foreach (EquipSlot slot in allEquipSlots)
            {
                if (slot.slotType.ToString().Trim().Equals(parsedEquipSlot, System.StringComparison.OrdinalIgnoreCase)) { targetSlotGeneric = slot; break; }
            }
        }

        if (targetSlotGeneric != null)
        {
            if (targetSlotGeneric.equippedItem != null && !string.IsNullOrEmpty(targetSlotGeneric.equippedItem.finalName))
            {
                if (!InventoryManager.instance.AddItemToInventory(targetSlotGeneric.equippedItem)) return false;
                targetSlotGeneric.UnequipItem(); 
            }

            targetSlotGeneric.EquipItemWithAlpha(itemToEquip, 1.0f);
            return true; 
        }

        return false;
    }
}