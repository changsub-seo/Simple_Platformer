using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("UI 연결")]
    public GameObject inventoryPanel; 
    public Transform slotContent; 
    public GameObject slotPrefab; 

    [Header("툴팁 UI 연결")]
    public GameObject tooltipPanel; 
    public TextMeshProUGUI tooltipName; 
    public TextMeshProUGUI tooltipStats; 

    [Header("스탯 색상 팔레트 (인스펙터에서 수정 가능)")]
    public Color hpColor = new Color(1.0f, 0.3f, 0.3f);
    public Color mpColor = new Color(0.3f, 0.6f, 1.0f);
    public Color adColor = new Color(0.4f, 0.7f, 1.0f); 
    public Color apColor = new Color(0.0f, 0.8f, 0.8f);
    public Color msColor = new Color(0.2f, 0.9f, 0.2f);

    [Header("가방 크기 설계 (칸 수 조절용)")]
    public int columns = 5;          
    public int rows = 5;             
    public float slotSize = 75f;     
    public float slotSpacing = 2f;   
    public float padding = 20f;      
    public float headerHeight = 60f; 

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (tooltipPanel != null) tooltipPanel.SetActive(false); 
        InitializeInventorySlots();
    }

    private void InitializeInventorySlots()
    {
        if (slotPrefab == null || slotContent == null) return;
        
        foreach (Transform child in slotContent) 
        {
            Destroy(child.gameObject);
        }

        GridLayoutGroup gridLayout = slotContent.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = slotContent.gameObject.AddComponent<GridLayoutGroup>();
        }

        gridLayout.cellSize = new Vector2(slotSize, slotSize);
        gridLayout.spacing = new Vector2(slotSpacing, slotSpacing);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = columns;
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperLeft;

        float contentWidth = (columns * slotSize) + ((columns - 1) * slotSpacing);
        float contentHeight = (rows * slotSize) + ((rows - 1) * slotSpacing);

        RectTransform contentRect = slotContent.GetComponent<RectTransform>();
        contentRect.sizeDelta = new Vector2(contentWidth, contentHeight);

        float panelWidth = contentWidth + (padding * 2);
        float panelHeight = contentHeight + headerHeight + (padding * 2);

        RectTransform panelRect = inventoryPanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);

        int totalSlots = columns * rows;
        for (int i = 0; i < totalSlots; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, slotContent);
            newSlot.transform.localScale = Vector3.one; 
        }
    }

    public bool AddItemToInventory(GeneratedItem itemToAdd)
    {
        InventorySlot[] existingSlots = slotContent.GetComponentsInChildren<InventorySlot>();
        
        for (int i = 0; i < existingSlots.Length; i++)
        {
            if (existingSlots[i].currentItem == null || string.IsNullOrEmpty(existingSlots[i].currentItem.finalName))
            {
                existingSlots[i].AddItem(itemToAdd);
                return true; 
            }
        }
        
        Debug.LogWarning("인벤토리가 꽉 찼습니다!");
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

    public void ShowTooltip(GeneratedItem item, RectTransform slotRect)
    {
        if (tooltipPanel == null || slotRect == null) return;
        
        tooltipName.text = item.finalName;

        string statText = "";
        if (item.hasHP) statText += GetColoredStatString("체력", item.rolledHP, hpColor) + "\n";
        if (item.hasMP) statText += GetColoredStatString("마나", item.rolledMP, mpColor) + "\n";
        if (item.hasAD) statText += GetColoredStatString("물리 공격력", item.rolledAD, adColor) + "\n";
        if (item.hasAP) statText += GetColoredStatString("마법 공격력", item.rolledAP, apColor) + "\n";
        if (item.hasMS) statText += GetColoredStatString("이동속도", item.rolledMS, msColor) + "\n";

        tooltipStats.text = statText.TrimEnd();

        tooltipPanel.SetActive(true);

        Canvas.ForceUpdateCanvases();
        if (tooltipName.TryGetComponent(out RectTransform nameRect))
            LayoutRebuilder.ForceRebuildLayoutImmediate(nameRect);
        if (tooltipStats.TryGetComponent(out RectTransform statsRect))
            LayoutRebuilder.ForceRebuildLayoutImmediate(statsRect);
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipPanel.GetComponent<RectTransform>());

        RectTransform inventoryRect = inventoryPanel.GetComponent<RectTransform>();
        RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        Canvas canvas = tooltipRect.GetComponentInParent<Canvas>();

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            inventoryRect, 
            RectTransformUtility.WorldToScreenPoint(null, slotRect.position), 
            null, 
            out localPoint
        );

        float tooltipHeight = tooltipRect.rect.height;
        float slotHeight = slotRect.rect.height;
        float pad = 5f;

        // ⭐ 1. 기본 위치: 슬롯의 상단 정중앙으로 배치 (Pivot Y가 1이므로 높이를 더해줍니다)
        float topY = localPoint.y + (slotHeight * 0.5f) + tooltipHeight + pad;
        // ⭐ 2. 예외 위치: 공간이 없을 때 반대로 출력할 하단 위치
        float bottomY = localPoint.y - (slotHeight * 0.5f) - pad;

        // 우선 상단으로 배치해 봅니다.
        localPoint.y = topY;
        tooltipRect.anchoredPosition = localPoint;

        // 화면 위에 툴팁을 띄울 공간이 충분한지 검사합니다.
        if (canvas != null)
        {
            Vector3[] tooltipCorners = new Vector3[4];
            tooltipRect.GetWorldCorners(tooltipCorners); 

            Vector3[] canvasCorners = new Vector3[4];
            ((RectTransform)canvas.transform).GetWorldCorners(canvasCorners); 

            // 툴팁의 가장 위쪽 끝(인덱스 1)이 화면 가장 위쪽을 벗어났다면
            if (tooltipCorners[1].y > canvasCorners[1].y)
            {
                // 위치를 아이템 하단으로 뒤집어줍니다.
                localPoint.y = bottomY;
                tooltipRect.anchoredPosition = localPoint;
            }
        }

        // 좌우로 벗어남 등 최종적인 안전망 보정 실행
        ClampTooltipToScreen(tooltipRect);
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    private string GetColoredStatString(string statName, int rolled, Color color)
    {
        string hex = ColorUtility.ToHtmlStringRGB(color);
        string sign = (rolled >= 0) ? "+" : "";
        return $"<color=#{hex}>{statName} {sign}{rolled}</color>";
    }

    private void ClampTooltipToScreen(RectTransform tooltipRect)
    {
        Canvas canvas = tooltipRect.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        RectTransform canvasRect = canvas.transform as RectTransform;

        Vector3[] canvasCorners = new Vector3[4];
        canvasRect.GetWorldCorners(canvasCorners);

        Vector3[] tooltipCorners = new Vector3[4];
        tooltipRect.GetWorldCorners(tooltipCorners);

        float tx = 0f;
        float ty = 0f;

        if (tooltipCorners[2].x > canvasCorners[2].x)
        {
            tx = canvasCorners[2].x - tooltipCorners[2].x;
        }
        else if (tooltipCorners[0].x < canvasCorners[0].x)
        {
            tx = canvasCorners[0].x - tooltipCorners[0].x;
        }

        if (tooltipCorners[0].y < canvasCorners[0].y)
        {
            ty = canvasCorners[0].y - tooltipCorners[0].y;
        }
        else if (tooltipCorners[2].y > canvasCorners[2].y)
        {
            ty = canvasCorners[2].y - tooltipCorners[2].y;
        }

        tooltipRect.position = new Vector3(tooltipRect.position.x + tx, tooltipRect.position.y + ty, tooltipRect.position.z);
    }
}