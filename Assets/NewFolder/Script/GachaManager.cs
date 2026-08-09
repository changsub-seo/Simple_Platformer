using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class CSVItemData
{
    public int id;
    public string itemName;
    public string iconName; 
    public int dropWeight;
    public string description;
    
    public string itemType; 
    public string equipSlot;
    
    public string hp;
    public string mp;
    public string ad;
    public string ap;
    public string ms;
    
    [HideInInspector] 
    public Sprite loadedIcon; 
}

[System.Serializable]
public class AffixData
{
    public int id;
    public string type;       
    public string affixName;  
    public string statType;   
    public int statValue;     
}

[System.Serializable]
public class GeneratedItem
{
    public CSVItemData baseItem;       
    public AffixData prefix1;          
    public AffixData suffix;           
    public string finalName;           

    public int rolledHP, minHP, maxHP;
    public int rolledMP, minMP, maxMP;
    public int rolledAD, minAD, maxAD;
    public int rolledAP, minAP, maxAP;
    public int rolledMS, minMS, maxMS;
    
    public bool hasHP, hasMP, hasAD, hasAP, hasMS;
}

public class GachaManager : MonoBehaviour
{
    [Header("가챠 및 스탯 설정")]
    public int pullCost = 10;
    
    [Range(0f, 1f)]
    public float statVariation = 0.3f; 

    [Header("데이터베이스")]
    public List<CSVItemData> itemPool = new List<CSVItemData>();
    public List<AffixData> prefixPool = new List<AffixData>();
    public List<AffixData> suffixPool = new List<AffixData>();

    [Header("UI 연결")]
    public GameObject gachaPanel;
    public GameObject resultPopup;
    public Image resultIcon;
    public TextMeshProUGUI resultName;
    public TextMeshProUGUI resultEquipSlot;
    public TextMeshProUGUI resultStats;
    public TextMeshProUGUI resultDesc;

    [Header("기본/특수 아이콘 설정")]
    public Sprite noIconSprite; // 인스펙터에서 Noicon 이미지 지정용

    private GeneratedItem currentDisplayedItem;
    private bool isAltPressed = false;

    void Start()
    {
        LoadItemDatabase();
        LoadAffixDatabase();
    }

    void Update()
    {
        if (resultPopup != null && resultPopup.activeSelf && currentDisplayedItem != null)
        {
            bool currentAltState = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

            if (currentAltState != isAltPressed)
            {
                isAltPressed = currentAltState;
                UpdateStatText(isAltPressed);
            }
        }
    }

    private void LoadItemDatabase()
    {
        TextAsset csvData = Resources.Load<TextAsset>("ItemDatabase");
        if (csvData == null) return;

        string[] rows = csvData.text.Replace("\r", "").Split('\n');
        
        for (int i = 1; i < rows.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(rows[i])) continue;
            string[] cols = rows[i].Split(',');

            if (cols.Length < 12) continue;

            CSVItemData newItem = new CSVItemData();
            newItem.id = int.Parse(cols[0]);
            newItem.itemName = cols[1];
            newItem.iconName = cols[2];
            newItem.dropWeight = int.Parse(cols[3]);
            newItem.description = cols[4];
            
            newItem.itemType = cols[5];
            newItem.equipSlot = cols[6];
            newItem.hp = cols[7];
            newItem.mp = cols[8];
            newItem.ad = cols[9];
            newItem.ap = cols[10];
            newItem.ms = cols[11];

            // 1. 고유 아이콘 로드 시도
            newItem.loadedIcon = Resources.Load<Sprite>(newItem.iconName);

            // 2. 고유 아이콘이 없다면 장착 부위(equipSlot)를 기반으로 기본 아이콘 로드
            if (newItem.loadedIcon == null && !string.IsNullOrEmpty(newItem.equipSlot) && newItem.equipSlot != "NULL")
            {
                string defaultPath = GetDefaultIconPath(newItem.equipSlot);
                newItem.loadedIcon = Resources.Load<Sprite>(defaultPath);
            }

            itemPool.Add(newItem);
        }
    }

    private string GetDefaultIconPath(string equipSlot)
    {
        return "ItemIcon/" + equipSlot;
    }

    private void LoadAffixDatabase()
    {
        TextAsset csvData = Resources.Load<TextAsset>("AffixDatabase");
        if (csvData == null)
        {
            Debug.LogError("AffixDatabase.csv를 찾을 수 없습니다!");
            return;
        }

        string[] rows = csvData.text.Replace("\r", "").Split('\n');
        for (int i = 1; i < rows.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(rows[i])) continue;

            List<string> colsList = new List<string>();
            bool inQuotes = false;
            string currentVal = "";

            for (int c = 0; c < rows[i].Length; c++)
            {
                char ch = rows[i][c];
                if (ch == '"')
                {
                    inQuotes = !inQuotes; 
                }
                else if (ch == ',' && !inQuotes)
                {
                    colsList.Add(currentVal);
                    currentVal = "";
                }
                else
                {
                    currentVal += ch;
                }
            }
            colsList.Add(currentVal); 
            string[] cols = colsList.ToArray();

            if (cols.Length < 5) continue;
            if (string.IsNullOrWhiteSpace(cols[0])) continue;

            AffixData newAffix = new AffixData();
            
            if (!int.TryParse(cols[0].Trim(), out newAffix.id)) continue;

            newAffix.type = cols[1].Trim();
            newAffix.affixName = cols[2].Trim();
            newAffix.statType = cols[3].Trim().Replace("\"", "");

            if (!int.TryParse(cols[4].Trim().Replace("\"", ""), out newAffix.statValue))
            {
                newAffix.statValue = 0;
            }

            if (newAffix.type == "Prefix") prefixPool.Add(newAffix);
            else if (newAffix.type == "Suffix") suffixPool.Add(newAffix);
        }
    }

    public void OpenGachaUI() { gachaPanel.SetActive(true); }
    public void CloseGachaUI() { gachaPanel.SetActive(false); }
    public void CloseResultPopup() { resultPopup.SetActive(false); }

    public void PullGacha()
    {
        if (GameManager.instance != null && GameManager.instance.SpendCoin(pullCost))
        {
            CSVItemData baseItem = GetRandomItem();                
            GeneratedItem finalItem = GenerateItemWithStats(baseItem); 
            
            if (InventoryManager.instance != null)
            {
                InventoryManager.instance.AddItemToInventory(finalItem);
            }

            ShowResult(finalItem);                                 
        }
    }

    private CSVItemData GetRandomItem()
    {
        int totalWeight = 0;
        foreach (var item in itemPool) totalWeight += item.dropWeight;
        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var item in itemPool)
        {
            currentWeight += item.dropWeight;
            if (randomValue < currentWeight) return item;
        }
        return itemPool[0]; 
    }

    private GeneratedItem GenerateItemWithStats(CSVItemData baseItem)
    {
        GeneratedItem item = new GeneratedItem();
        item.baseItem = baseItem;
        string fullName = baseItem.itemName;

        RollStat(baseItem.hp, out item.hasHP, out item.rolledHP, out item.minHP, out item.maxHP);
        RollStat(baseItem.mp, out item.hasMP, out item.rolledMP, out item.minMP, out item.maxMP);
        RollStat(baseItem.ad, out item.hasAD, out item.rolledAD, out item.minAD, out item.maxAD);
        RollStat(baseItem.ap, out item.hasAP, out item.rolledAP, out item.minAP, out item.maxAP);
        RollStat(baseItem.ms, out item.hasMS, out item.rolledMS, out item.minMS, out item.maxMS);

        if (baseItem.itemType == "Equipment")
        {
            if (prefixPool.Count > 0)
            {
                int prefixRoll = Random.Range(0, 100);
                if (prefixRoll >= 40) 
                {
                    item.prefix1 = prefixPool[Random.Range(0, prefixPool.Count)];
                    fullName = $"{item.prefix1.affixName} {fullName}";
                }
            }

            if (suffixPool.Count > 0)
            {
                int suffixRoll = Random.Range(0, 100);
                if (suffixRoll >= 50) 
                {
                    item.suffix = suffixPool[Random.Range(0, suffixPool.Count)];
                    fullName = $"{fullName} [{item.suffix.affixName}]"; 
                }
            }
        }

        item.finalName = fullName;

        if (item.prefix1 != null) ApplyAffixStats(item, item.prefix1);
        if (item.suffix != null) ApplyAffixStats(item, item.suffix);

        return item;
    }

    private void ApplyAffixStats(GeneratedItem item, AffixData affix)
    {
        if (string.IsNullOrEmpty(affix.statType)) return;

        string[] statTypes = affix.statType.Split(',');

        foreach (string st in statTypes)
        {
            string type = st.Trim().ToUpper();

            switch (type)
            {
                case "HP":
                    item.hasHP = true;
                    item.rolledHP += affix.statValue;
                    item.minHP += affix.statValue; 
                    item.maxHP += affix.statValue;
                    break;
                case "MP":
                    item.hasMP = true;
                    item.rolledMP += affix.statValue;
                    item.minMP += affix.statValue;
                    item.maxMP += affix.statValue;
                    break;
                case "AD":
                    item.hasAD = true;
                    item.rolledAD += affix.statValue;
                    item.minAD += affix.statValue;
                    item.maxAD += affix.statValue;
                    break;
                case "AP":
                    item.hasAP = true;
                    item.rolledAP += affix.statValue;
                    item.minAP += affix.statValue;
                    item.maxAP += affix.statValue;
                    break;
                case "MS":
                    item.hasMS = true;
                    item.rolledMS += affix.statValue;
                    item.minMS += affix.statValue;
                    item.maxMS += affix.statValue;
                    break;
            }
        }
    }

    private void RollStat(string baseStatStr, out bool hasStat, out int rolledVal, out int minVal, out int maxVal)
    {
        if (baseStatStr != "NULL" && int.TryParse(baseStatStr, out int baseStat))
        {
            hasStat = true;
            int variation = Mathf.Max(1, Mathf.RoundToInt(baseStat * statVariation)); 
            
            minVal = baseStat - variation;
            maxVal = baseStat + variation;
            
            rolledVal = Random.Range(minVal, maxVal + 1); 
        }
        else
        {
            hasStat = false;
            rolledVal = 0; minVal = 0; maxVal = 0;
        }
    }

    private void ShowResult(GeneratedItem item)
    {
        currentDisplayedItem = item;

        // ⭐ 가챠 결과 아이콘 처리 (아이콘이 없으면 Noicon 출력)
        if (resultIcon != null)
        {
            if (item.baseItem != null && item.baseItem.loadedIcon != null)
            {
                resultIcon.sprite = item.baseItem.loadedIcon;
                resultIcon.enabled = true;
                resultIcon.color = Color.white;
            }
            else if (noIconSprite != null)
            {
                resultIcon.sprite = noIconSprite;
                resultIcon.enabled = true;
                resultIcon.color = Color.white;
            }
            else
            {
                resultIcon.sprite = null;
                resultIcon.enabled = false;
            }
        }

        resultName.text = item.finalName; 
        
        if (item.baseItem.equipSlot != "NULL" && !string.IsNullOrEmpty(item.baseItem.equipSlot))
        {
            string koreanSlot = GetKoreanSlotName(item.baseItem.equipSlot);
            resultEquipSlot.text = $"착용 부위 : <color=#FFD700>{koreanSlot}</color>";
        }
        else
        {
            resultEquipSlot.text = ""; 
        }

        isAltPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        UpdateStatText(isAltPressed);

        string extraDesc = (item.prefix1 != null || item.suffix != null) 
            ? "\n\n<color=#FFD700>특별한 옵션이 부여된 장비입니다!</color>" 
            : "";
            
        resultDesc.text = item.baseItem.description + extraDesc;
        resultPopup.SetActive(true);
    }

    private string GetColoredStatString(string statName, int rolled, int min, int max, bool showDetails)
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

        float ratio = 0.5f; 
        if (max > min)
        {
            ratio = (float)(rolled - min) / (max - min);
            ratio = Mathf.Clamp01(ratio); 
        }

        Color finalColor = Color.Lerp(baseColor * 0.5f, baseColor * 1.4f, ratio);
        finalColor.a = 1.0f; 

        string hexColor = ColorUtility.ToHtmlStringRGB(finalColor);
        string sign = (rolled >= 0) ? "+" : "";
        string result = $"<color=#{hexColor}>{statName} {sign}{rolled}</color>";

        if (showDetails)
        {
            result += $" <color=#A9A9A9>({min}~{max})</color>";
        }

        return result;
    }

    private void UpdateStatText(bool showDetails)
    {
        if (currentDisplayedItem == null) return;

        string statText = "";
        var item = currentDisplayedItem;

        if (item.hasHP) statText += GetColoredStatString("체력", item.rolledHP, item.minHP, item.maxHP, showDetails) + "\n";
        if (item.hasMP) statText += GetColoredStatString("마나", item.rolledMP, item.minMP, item.maxMP, showDetails) + "\n";
        if (item.hasAD) statText += GetColoredStatString("물리 공격력", item.rolledAD, item.minAD, item.maxAD, showDetails) + "\n";
        if (item.hasAP) statText += GetColoredStatString("마법 공격력", item.rolledAP, item.minAP, item.maxAP, showDetails) + "\n";
        if (item.hasMS) statText += GetColoredStatString("이동속도", item.rolledMS, item.minMS, item.maxMS, showDetails) + "\n";

        resultStats.text = statText.TrimEnd(); 
    }

    private string GetKoreanSlotName(string englishSlot)
    {
        switch (englishSlot)
        {
            case "Head": return "머리";
            case "Neck": return "목걸이";
            case "Ring": return "반지";
            case "Gloves": return "장갑";
            case "Waist": return "허리";
            case "Top": return "상의";
            case "Bottom": return "하의";
            case "Overall": return "한벌옷";
            case "Wrist": return "손목";
            case "Shoes": return "신발";
            case "OneHandWeapon": return "한손 무기";
            case "TwoHandWeapon": return "양손 무기";
            default: return englishSlot;
        }
    }
}