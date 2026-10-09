using UnityEngine;
using TMPro; 
using System.Collections.Generic; 

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager instance;
    public static EquipmentManager Instance => instance; 

    [Header("장비 슬롯 12개 연결")]
    public EquipSlot[] allEquipSlots; 

    [Header("스탯창 텍스트 연결")]
    public TextMeshProUGUI statTextUI; 

    private PlayerStats playerStats;

    void Awake()
    {
        if (instance == null) instance = this;
        playerStats = GetComponent<PlayerStats>();
    }

    void Start()
    {
        if (playerStats == null) playerStats = FindObjectOfType<PlayerStats>();
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
                UpdateTotalStats(); 
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
                UpdateTotalStats(); 
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
                UpdateTotalStats(); 
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
                UpdateTotalStats(); 
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
            UpdateTotalStats(); 
            return true; 
        }

        return false;
    }

    public HashSet<GeneratedItem> GetUniqueEquippedItems()
    {
        HashSet<GeneratedItem> uniqueItems = new HashSet<GeneratedItem>();
        if (allEquipSlots == null) return uniqueItems;

        foreach (EquipSlot slot in allEquipSlots)
        {
            if (slot != null && slot.equippedItem != null && !string.IsNullOrEmpty(slot.equippedItem.finalName))
            {
                uniqueItems.Add(slot.equippedItem);
            }
        }
        return uniqueItems;
    }

    public void UpdateTotalStats()
    {
        Dictionary<string, float> totals = new Dictionary<string, float>(System.StringComparer.OrdinalIgnoreCase);
        foreach (GeneratedItem item in GetUniqueEquippedItems())
        {
            if (item == null) continue;
            foreach (KeyValuePair<string, StatRoll> pair in item.stats)
            {
                if (pair.Value == null || !pair.Value.hasValue) continue;
                string key = StatKeys.Normalize(pair.Key);
                totals[key] = totals.TryGetValue(key, out float current) ? current + pair.Value.value : pair.Value.value;
            }
        }

        if (statTextUI != null)
        {
            List<string> lines = new List<string>();
            List<string> keys = new List<string>(totals.Keys);
            keys.Sort(System.StringComparer.OrdinalIgnoreCase);
            foreach (string key in keys)
            {
                float value = totals[key];
                if (Mathf.Approximately(value, 0f)) continue;
                string formatted = Mathf.Approximately(value, Mathf.Round(value))
                    ? Mathf.RoundToInt(value).ToString()
                    : value.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
                string sign = value > 0f ? "+" : string.Empty;
                lines.Add($"{StatKeys.DisplayName(key)} {sign}{formatted}");
            }
            statTextUI.text = lines.Count == 0 ? "장착된 장비가 없습니다." : string.Join("\n", lines);
        }

        if (playerStats == null) playerStats = GetComponent<PlayerStats>();
        if (playerStats == null) playerStats = FindObjectOfType<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.UpdateDerivedStats();
        }
    }

    private string FormatStatString(string statName, int value)
    {
        if (value == 0) return ""; 
        string sign = value > 0 ? "+" : ""; 
        return $"{statName} {sign}{value}\n";
    }

    private string FormatFloatStatString(string statName, float value)
    {
        if (Mathf.Abs(value) < 0.01f) return ""; 
        string sign = value > 0 ? "+" : ""; 
        return $"{statName} {sign}{value:F1}\n";
    }

    public float GetTotalBonusStat(string statKey)
    {
        float total = 0f;
        foreach (var item in GetUniqueEquippedItems())
        {
            if (item != null && item.HasStat(statKey)) total += item.GetStat(statKey);
        }
        return total;
    }

    public int GetTotalBonusStrength() => Mathf.RoundToInt(GetTotalBonusStat("STR"));
    public int GetTotalBonusDexterity() => Mathf.RoundToInt(GetTotalBonusStat("DEX"));
    public int GetTotalBonusIntelligence() => Mathf.RoundToInt(GetTotalBonusStat("INT"));

    public float GetTotalBonusAttackDamage() => GetTotalBonusStat("AttackDamage");
    public float GetTotalBonusAbilityPower() => GetTotalBonusStat("AbilityPower");
    public float GetTotalBonusAttackSpeed() => GetTotalBonusStat("AttackSpeed");
    public int GetTotalBonusDefense() => Mathf.RoundToInt(GetTotalBonusStat("Defense"));
    public int GetTotalBonusResistance() => Mathf.RoundToInt(GetTotalBonusStat("Resistance"));
    public float GetTotalBonusHitRate() => GetTotalBonusStat("HitRate");
    public float GetTotalBonusMoveSpeed() => GetTotalBonusStat("MoveSpeed");
    public float GetTotalBonusJumpForce() => GetTotalBonusStat("JumpForce");
    public int GetTotalBonusHP() => Mathf.RoundToInt(GetTotalBonusStat("HP"));
    public int GetTotalBonusMP() => Mathf.RoundToInt(GetTotalBonusStat("MP"));
    public int GetTotalBonusSpirit() => Mathf.RoundToInt(GetTotalBonusStat("Spirit"));

}
