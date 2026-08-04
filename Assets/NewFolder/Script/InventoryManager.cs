using UnityEngine;
using TMPro;
using System.Collections.Generic; // List 사용을 위해 추가

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("UI 연결")]
    public GameObject inventoryPanel; 
    public Transform slotContent; 
    
    // ⭐ 자동으로 슬롯을 찍어내기 위한 '프리팹(원본)' 변수 추가
    public GameObject slotPrefab; 

    [Header("툴팁 UI 연결")]
    public GameObject tooltipPanel; 
    public TextMeshProUGUI tooltipName; 
    public TextMeshProUGUI tooltipStats; 

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (tooltipPanel != null) tooltipPanel.SetActive(false); 
    }

    // ⭐ 핵심 수정: 슬롯 무한 생성 및 아이템 할당
    public bool AddItemToInventory(GeneratedItem itemToAdd)
    {
        // 1. 현재 Content(부모) 아래에 있는 모든 슬롯을 실시간으로 가져옵니다. (1번 문제 해결)
        InventorySlot[] existingSlots = slotContent.GetComponentsInChildren<InventorySlot>();
        
        // 2. 비어있는 슬롯이 있는지 먼저 찾습니다.
        for (int i = 0; i < existingSlots.Length; i++)
        {
            if (existingSlots[i].currentItem == null)
            {
                existingSlots[i].AddItem(itemToAdd);
                return true;
            }
        }
        
        // 3. 빈 슬롯이 없다면? (인벤토리가 꽉 찼다면) -> 슬롯을 새로 생성합니다! (2, 3번 문제 해결)
        if (slotPrefab != null)
        {
            // slotPrefab을 복사해서 slotContent(바둑판 UI) 안에 자식으로 넣습니다.
            GameObject newSlotObj = Instantiate(slotPrefab, slotContent);
            InventorySlot newSlot = newSlotObj.GetComponent<InventorySlot>();
            
            if (newSlot != null)
            {
                newSlot.AddItem(itemToAdd);
                return true;
            }
        }
        else
        {
            Debug.LogWarning("슬롯 프리팹(Slot Prefab)이 인스펙터에 연결되지 않아 슬롯을 생성할 수 없습니다!");
        }

        return false; 
    }

    public void OpenInventoryUI()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(true);
    }

    public void CloseInventoryUI()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        HideTooltip(); 
    }

    public void ShowTooltip(GeneratedItem item, Vector3 slotPosition)
    {
        if (tooltipPanel == null) return;
        
        tooltipName.text = item.finalName;

        string statText = "";
        if (item.hasHP) statText += GetColoredStatString("체력", item.rolledHP) + "\n";
        if (item.hasMP) statText += GetColoredStatString("마나", item.rolledMP) + "\n";
        if (item.hasAD) statText += GetColoredStatString("물리 공격력", item.rolledAD) + "\n";
        if (item.hasAP) statText += GetColoredStatString("마법 공격력", item.rolledAP) + "\n";
        if (item.hasMS) statText += GetColoredStatString("이동속도", item.rolledMS) + "\n";

        tooltipStats.text = statText.TrimEnd();

        // 툴팁 위치 조정
        tooltipPanel.transform.position = slotPosition + new Vector3(80, -80, 0);
        tooltipPanel.SetActive(true);
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    private string GetColoredStatString(string statName, int rolled)
    {
        Color baseColor = Color.white;
        switch (statName)
        {
            case "체력": baseColor = new Color(1.0f, 0.3f, 0.3f); break;
            case "마나": baseColor = new Color(0.3f, 0.6f, 1.0f); break;
            case "물리 공격력": baseColor = new Color(0.1f, 0.1f, 0.5f); break;
            case "마법 공격력": baseColor = new Color(0.0f, 0.6f, 0.6f); break;
            case "이동속도": baseColor = new Color(0.2f, 0.8f, 0.2f); break;
        }

        string hexColor = ColorUtility.ToHtmlStringRGB(baseColor);
        string sign = (rolled >= 0) ? "+" : "";
        return $"<color=#{hexColor}>{statName} {sign}{rolled}</color>";
    }
}