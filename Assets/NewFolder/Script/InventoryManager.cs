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

    [Header("스탯 색상 팔레트")]
    public Color strColor = new Color(1.0f, 0.4f, 0.4f);
    public Color dexColor = new Color(0.4f, 0.9f, 0.4f);
    public Color intColor = new Color(0.4f, 0.6f, 1.0f);
    public Color atkColor = new Color(1.0f, 0.8f, 0.2f);
    public Color abilityPowerColor = new Color(0.8f, 0.5f, 1.0f);
    public Color defColor = new Color(0.7f, 0.7f, 0.7f);
    public Color msColor = new Color(0.2f, 0.8f, 0.2f);
    public Color jfColor = new Color(0.8f, 0.4f, 1.0f);
    public Color hpColor = new Color(1.0f, 0.3f, 0.3f);
    public Color mpColor = new Color(0.3f, 0.6f, 1.0f);
    public Color spiritColor = new Color(0.6f, 0.4f, 0.8f);

    [Header("가방 크기 설계")]
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

        System.Collections.Generic.List<string> statLines = new System.Collections.Generic.List<string>();
        foreach (System.Collections.Generic.KeyValuePair<string, StatRoll> pair in item.stats)
        {
            if (pair.Value == null || !pair.Value.hasValue) continue;
            statLines.Add(GetColoredStatString(StatKeys.DisplayName(pair.Key), pair.Value.value, GetStatColor(pair.Key)));
        }

        tooltipStats.text = string.Join("\n", statLines);
        tooltipPanel.SetActive(true);

        Canvas.ForceUpdateCanvases();
        if (tooltipName.TryGetComponent(out RectTransform nameRect)) LayoutRebuilder.ForceRebuildLayoutImmediate(nameRect);
        if (tooltipStats.TryGetComponent(out RectTransform statsRect)) LayoutRebuilder.ForceRebuildLayoutImmediate(statsRect);
        
        RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);

        Vector3[] slotCorners = new Vector3[4];
        slotRect.GetWorldCorners(slotCorners);
        Vector3 slotTopCenter = (slotCorners[1] + slotCorners[2]) / 2f;
        Vector3 slotBottomCenter = (slotCorners[0] + slotCorners[3]) / 2f;

        tooltipRect.position = slotTopCenter;

        Vector3[] tooltipCorners = new Vector3[4];
        tooltipRect.GetWorldCorners(tooltipCorners);
        Vector3 tooltipBottomCenter = (tooltipCorners[0] + tooltipCorners[3]) / 2f;
        
        float offsetY = slotTopCenter.y - tooltipBottomCenter.y;
        tooltipRect.position += new Vector3(0, offsetY, 0);

        // 캔버스 삐져나감 제어
        Canvas parentCanvas = tooltipRect.GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            Canvas rootCanvas = parentCanvas.rootCanvas;
            tooltipRect.GetWorldCorners(tooltipCorners); 
            
            Vector3[] canvasCorners = new Vector3[4];
            rootCanvas.GetComponent<RectTransform>().GetWorldCorners(canvasCorners);

            if (tooltipCorners[1].y > canvasCorners[1].y) // 화면 윗부분을 뚫고 나갔다면
            {
                tooltipRect.position = slotBottomCenter; 
                tooltipRect.GetWorldCorners(tooltipCorners);
                Vector3 tooltipTopCenter = (tooltipCorners[1] + tooltipCorners[2]) / 2f;
                
                float flipOffsetY = slotBottomCenter.y - tooltipTopCenter.y;
                tooltipRect.position += new Vector3(0, flipOffsetY, 0);
            }
        }

        ClampTooltipToScreen(tooltipRect);
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    private Color GetStatColor(string statKey)
    {
        switch (StatKeys.Normalize(statKey))
        {
            case "STR": return strColor;
            case "DEX": return dexColor;
            case "INT": return intColor;
            case "AttackDamage": return atkColor;
            case "AbilityPower": return abilityPowerColor;
            case "Defense": return defColor;
            case "MoveSpeed": return msColor;
            case "JumpForce": return jfColor;
            case "HP": return hpColor;
            case "MP": return mpColor;
            case "Spirit": return spiritColor;
            default: return Color.white;
        }
    }

    private string GetColoredStatString(string statName, float rolled, Color color)
    {
        string hex = ColorUtility.ToHtmlStringRGB(color);
        string sign = (rolled >= 0) ? "+" : "";
        string value = Mathf.Approximately(rolled, Mathf.Round(rolled))
            ? Mathf.RoundToInt(rolled).ToString()
            : rolled.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
        return $"<color=#{hex}>{statName} {sign}{value}</color>";
    }

    private void ClampTooltipToScreen(RectTransform tooltipRect)
    {
        Canvas parentCanvas = tooltipRect.GetComponentInParent<Canvas>();
        if (parentCanvas == null) return;

        Canvas rootCanvas = parentCanvas.rootCanvas;
        RectTransform canvasRect = rootCanvas.GetComponent<RectTransform>();

        Vector3[] canvasCorners = new Vector3[4];
        canvasRect.GetWorldCorners(canvasCorners);

        Vector3[] tooltipCorners = new Vector3[4];
        tooltipRect.GetWorldCorners(tooltipCorners);

        float tx = 0f;
        float ty = 0f;

        // 좌우 삐져나감 밀어넣기
        if (tooltipCorners[2].x > canvasCorners[2].x)
        {
            tx = canvasCorners[2].x - tooltipCorners[2].x;
        }
        else if (tooltipCorners[0].x < canvasCorners[0].x)
        {
            tx = canvasCorners[0].x - tooltipCorners[0].x;
        }

        // 상하 삐져나감 밀어넣기
        if (tooltipCorners[0].y < canvasCorners[0].y)
        {
            ty = canvasCorners[0].y - tooltipCorners[0].y;
        }
        else if (tooltipCorners[1].y > canvasCorners[1].y)
        {
            ty = canvasCorners[1].y - tooltipCorners[1].y;
        }

        tooltipRect.position += new Vector3(tx, ty, 0f);
    }
}
