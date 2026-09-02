using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 추가
using System.Collections.Generic; // HashSet(중복 방지)을 사용하기 위해 추가

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager instance;
    
    [Header("장비 슬롯 12개 연결")]
    public EquipSlot[] allEquipSlots; 

    [Header("스탯창 텍스트 연결")]
    public TextMeshProUGUI statTextUI; // ⭐ 여기에 텍스트를 연결할 겁니다!

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
                UpdateTotalStats(); // ⭐ 장착 성공 시 스탯 갱신
                return true;
            }
            return false;
        }

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
                UpdateTotalStats(); // ⭐ 장착 성공 시 스탯 갱신
                return true;
            }
        }

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

        // 3. 양손무기 처리
        if (parsedEquipSlot.Equals("TwoHandWeapon", System.StringComparison.OrdinalIgnoreCase))
        {
            if (weapon1 != null && weapon2 != null)
            {
                bool hasW1 = (weapon1.equippedItem != null && !string.IsNullOrEmpty(weapon1.equippedItem.finalName));
                bool hasW2 = (weapon2.equippedItem != null && !string.IsNullOrEmpty(weapon2.equippedItem.finalName));

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
                UpdateTotalStats(); // ⭐ 장착 성공 시 스탯 갱신
                return true;
            }
            return false;
        }

        // 4. 한손무기 처리
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
                UpdateTotalStats(); // ⭐ 장착 성공 시 스탯 갱신
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
            UpdateTotalStats(); // ⭐ 장착 성공 시 스탯 갱신
            return true; 
        }

        return false;
    }

    // ⭐ 새롭게 추가된 스탯 종합 계산기 함수입니다!
    public void UpdateTotalStats()
    {
        if (statTextUI == null) return; // 텍스트가 연결 안 되어있으면 무시

        int totalHP = 0, totalMP = 0, totalAD = 0, totalAP = 0, totalMS = 0;

        // 양손무기나 한벌옷이 두 번 더해지는 것을 막기 위해 중복 제거(HashSet)를 사용합니다.
        HashSet<GeneratedItem> uniqueItems = new HashSet<GeneratedItem>();

        foreach (EquipSlot slot in allEquipSlots)
        {
            if (slot.equippedItem != null && !string.IsNullOrEmpty(slot.equippedItem.finalName))
            {
                uniqueItems.Add(slot.equippedItem);
            }
        }

        // 겹치지 않는 진짜 장비들만 스탯을 더해줍니다.
        foreach (GeneratedItem item in uniqueItems)
        {
            if (item.hasHP) totalHP += item.rolledHP;
            if (item.hasMP) totalMP += item.rolledMP;
            if (item.hasAD) totalAD += item.rolledAD;
            if (item.hasAP) totalAP += item.rolledAP;
            if (item.hasMS) totalMS += item.rolledMS;
        }

        // 합산된 스탯을 예쁜 문자열로 만듭니다.
        string resultText = "";
        resultText += FormatStatString("체력", totalHP);
        resultText += FormatStatString("마나", totalMP);
        resultText += FormatStatString("물리 공격력", totalAD);
        resultText += FormatStatString("마법 공격력", totalAP);
        resultText += FormatStatString("이동속도", totalMS);

        // 아무것도 낀 게 없으면 띄울 기본 멘트
        if (string.IsNullOrEmpty(resultText))
        {
            resultText = "장착된 장비가 없습니다.";
        }

        statTextUI.text = resultText.TrimEnd(); // 마지막 줄바꿈 제거 후 출력
    }

    // 스탯 수치에 따라 +, - 기호를 예쁘게 붙여주는 도우미 함수
    private string FormatStatString(string statName, int value)
    {
        if (value == 0) return ""; // 0이면 출력 안 함
        string sign = value > 0 ? "+" : ""; // 양수면 + 붙이기
        return $"{statName} {sign}{value}\n";
    }
}