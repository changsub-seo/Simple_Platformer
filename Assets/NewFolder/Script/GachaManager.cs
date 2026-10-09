using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Globalization;

public static class StatKeys
{
    private static readonly Dictionary<string, string> aliases = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
    {
        { "AD", "AttackDamage" },
        { "AP", "AbilityPower" }, { "AtkSpeed", "AttackSpeed" }, { "Accuracy", "HitRate" },
        { "MS", "MoveSpeed" }, { "JF", "JumpForce" }, { "DEF", "Defense" }, { "SP", "Spirit" }
    };

    private static readonly HashSet<string> fractionalStats = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
    {
        "AttackSpeed", "HitRate", "MoveSpeed", "JumpForce"
    };

    public static string Normalize(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return string.Empty;
        string trimmed = key.Trim();
        return aliases.TryGetValue(trimmed, out string canonical) ? canonical : trimmed;
    }

    public static bool IsFractional(string key) => fractionalStats.Contains(Normalize(key));

    public static string DisplayName(string key)
    {
        switch (Normalize(key))
        {
            case "STR": return "힘 (STR)";
            case "DEX": return "민첩 (DEX)";
            case "INT": return "지능 (INT)";
            case "AttackDamage": return "물리 공격력";
            case "AbilityPower": return "마법 공격력";
            case "AttackSpeed": return "공격속도";
            case "Defense": return "방어력";
            case "Resistance": return "저항력";
            case "HitRate": return "명중률";
            case "MoveSpeed": return "이동속도";
            case "JumpForce": return "점프력";
            case "HP": return "체력";
            case "MP": return "마나";
            case "Spirit": return "정신력";
            default: return Normalize(key);
        }
    }
}

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
    
    public Dictionary<string, float> stats = new Dictionary<string, float>(System.StringComparer.OrdinalIgnoreCase);
    
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
    public float statValue;   
}

[System.Serializable]
public class StatRoll
{
    public float value;
    public float min;
    public float max;
    public bool hasValue;
}

[System.Serializable]
public class GeneratedItem
{
    public CSVItemData baseItem;       
    public AffixData prefix1;          
    public AffixData suffix;           
    public string finalName;           

    public Dictionary<string, StatRoll> stats = new Dictionary<string, StatRoll>(System.StringComparer.OrdinalIgnoreCase);

    public bool HasStat(string key) => stats.TryGetValue(StatKeys.Normalize(key), out StatRoll stat) && stat.hasValue;
    public float GetStat(string key) => stats.TryGetValue(StatKeys.Normalize(key), out StatRoll stat) ? stat.value : 0f;

    public void AddStat(string key, float amount)
    {
        string canonicalKey = StatKeys.Normalize(key);
        if (string.IsNullOrEmpty(canonicalKey)) return;
        if (!stats.TryGetValue(canonicalKey, out StatRoll stat))
        {
            stat = new StatRoll();
            stats.Add(canonicalKey, stat);
        }
        stat.value += amount;
        stat.min += amount;
        stat.max += amount;
        stat.hasValue = true;
    }
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
    public Sprite noIconSprite;

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
        if (csvData == null)
        {
            Debug.LogError("ItemDatabase.csv를 찾을 수 없습니다!");
            return;
        }

        string[] rows = csvData.text.TrimStart('\uFEFF').Replace("\r", "").Split('\n');
        if (rows.Length == 0) return;

        List<string> headers = ParseCsvRow(rows[0]);
        Dictionary<string, int> columnByName = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
        for (int column = 0; column < headers.Count; column++)
        {
            string header = headers[column].Trim();
            if (!string.IsNullOrEmpty(header)) columnByName[header] = column;
        }

        int dataStart = rows.Length > 1 && !int.TryParse(ParseCsvRow(rows[1])[0].Trim(), out _) ? 2 : 1;
        HashSet<string> metadataColumns = new HashSet<string>(new[]
        {
            "ID", "ItemName", "IconName", "DropWeight", "Description", "ItemType", "EquipSlot"
        }, System.StringComparer.OrdinalIgnoreCase);

        for (int i = dataStart; i < rows.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(rows[i])) continue;
            List<string> cols = ParseCsvRow(rows[i]);
            if (!TryGetColumn(cols, columnByName, "ID", out string idText) || !int.TryParse(idText, out int parsedId)) continue;

            CSVItemData newItem = new CSVItemData();
            newItem.id = parsedId;
            newItem.itemName = GetColumn(cols, columnByName, "ItemName");
            newItem.iconName = GetColumn(cols, columnByName, "IconName");
            if (!int.TryParse(GetColumn(cols, columnByName, "DropWeight"), out newItem.dropWeight)) newItem.dropWeight = 10;
            newItem.description = GetColumn(cols, columnByName, "Description");
            newItem.itemType = GetColumn(cols, columnByName, "ItemType");
            newItem.equipSlot = GetColumn(cols, columnByName, "EquipSlot");

            foreach (KeyValuePair<string, int> column in columnByName)
            {
                if (metadataColumns.Contains(column.Key) || column.Value >= cols.Count) continue;
                string value = cols[column.Value].Trim();
                if (string.IsNullOrEmpty(value) || value.Equals("NULL", System.StringComparison.OrdinalIgnoreCase)) continue;
                if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedValue))
                {
                    newItem.stats[StatKeys.Normalize(column.Key)] = parsedValue;
                }
                else
                {
                    Debug.LogWarning($"ItemDatabase 행 {i + 1}, 스탯 '{column.Key}' 값 '{value}'를 숫자로 읽을 수 없습니다.");
                }
            }

            // 아이콘 로드
            newItem.loadedIcon = Resources.Load<Sprite>(newItem.iconName);

            if (newItem.loadedIcon == null && !string.IsNullOrEmpty(newItem.equipSlot) && newItem.equipSlot != "NULL")
            {
                string defaultPath = GetDefaultIconPath(newItem.equipSlot);
                newItem.loadedIcon = Resources.Load<Sprite>(defaultPath);
            }

            itemPool.Add(newItem);
        }
    }

    private static List<string> ParseCsvRow(string row)
    {
        List<string> values = new List<string>();
        bool insideQuotes = false;
        System.Text.StringBuilder value = new System.Text.StringBuilder();

        for (int i = 0; i < row.Length; i++)
        {
            char character = row[i];
            if (character == '"')
            {
                if (insideQuotes && i + 1 < row.Length && row[i + 1] == '"')
                {
                    value.Append('"');
                    i++;
                }
                else insideQuotes = !insideQuotes;
            }
            else if (character == ',' && !insideQuotes)
            {
                values.Add(value.ToString());
                value.Length = 0;
            }
            else value.Append(character);
        }

        values.Add(value.ToString());
        return values;
    }

    private static bool TryGetColumn(List<string> values, Dictionary<string, int> columns, string name, out string value)
    {
        value = string.Empty;
        if (!columns.TryGetValue(name, out int index) || index >= values.Count) return false;
        value = values[index].Trim();
        return true;
    }

    private static string GetColumn(List<string> values, Dictionary<string, int> columns, string name)
    {
        return TryGetColumn(values, columns, name, out string value) ? value : string.Empty;
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

        string[] rows = csvData.text.TrimStart('\uFEFF').Replace("\r", "").Split('\n');
        if (rows.Length == 0) return;

        List<string> headers = ParseCsvRow(rows[0]);
        Dictionary<string, int> columnByName = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
        for (int column = 0; column < headers.Count; column++)
        {
            string header = headers[column].Trim();
            if (!string.IsNullOrEmpty(header)) columnByName[header] = column;
        }

        for (int i = 1; i < rows.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(rows[i])) continue;
            List<string> cols = ParseCsvRow(rows[i]);
            if (!TryGetColumn(cols, columnByName, "ID", out string idText) || !int.TryParse(idText, out int parsedId)) continue;

            AffixData newAffix = new AffixData();
            newAffix.id = parsedId;
            newAffix.type = GetColumn(cols, columnByName, "Type");
            newAffix.affixName = GetColumn(cols, columnByName, "AffixName");
            newAffix.statType = GetColumn(cols, columnByName, "StatType");

            if (!float.TryParse(GetColumn(cols, columnByName, "StatValue"), NumberStyles.Float, CultureInfo.InvariantCulture, out newAffix.statValue))
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
        if (itemPool.Count == 0) return null;

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

        foreach (KeyValuePair<string, float> baseStat in baseItem.stats)
        {
            StatRoll rolledStat = RollStat(baseStat.Key, baseStat.Value);
            item.stats[baseStat.Key] = rolledStat;
        }

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

    // ⭐ 어픽스 스탯 명칭 통일 파싱 함수
    private void ApplyAffixStats(GeneratedItem item, AffixData affix)
    {
        if (string.IsNullOrEmpty(affix.statType)) return;

        string[] statTypes = affix.statType.Split(',');
        foreach (string st in statTypes)
        {
            string key = StatKeys.Normalize(st);
            if (string.IsNullOrEmpty(key)) continue;
            item.AddStat(key, affix.statValue);
        }
    }

    private StatRoll RollStat(string key, float baseValue)
    {
        StatRoll stat = new StatRoll { hasValue = true };
        if (StatKeys.IsFractional(key) || !Mathf.Approximately(baseValue, Mathf.Round(baseValue)))
        {
            float variation = Mathf.Abs(baseValue) * statVariation;
            stat.min = baseValue - variation;
            stat.max = baseValue + variation;
            stat.value = Random.Range(stat.min, stat.max);
        }
        else
        {
            int integerBase = Mathf.RoundToInt(baseValue);
            int variation = Mathf.Max(1, Mathf.RoundToInt(Mathf.Abs(integerBase) * statVariation));
            stat.min = integerBase - variation;
            stat.max = integerBase + variation;
            stat.value = Random.Range(Mathf.RoundToInt(stat.min), Mathf.RoundToInt(stat.max) + 1);
        }
        return stat;
    }

    private void ShowResult(GeneratedItem item)
    {
        currentDisplayedItem = item;

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

    private string GetColoredStatString(string statName, float rolled, float min, float max, bool showDetails, bool isFloat = false)
    {
        Color baseColor = Color.white;
        switch (statName)
        {
            case "힘 (STR)": baseColor = new Color(1.0f, 0.4f, 0.4f); break;
            case "민첩 (DEX)": baseColor = new Color(0.4f, 0.9f, 0.4f); break;
            case "지능 (INT)": baseColor = new Color(0.4f, 0.6f, 1.0f); break;
            case "물리 공격력":
            case "마법 공격력": baseColor = new Color(1.0f, 0.8f, 0.2f); break;
            case "방어력": baseColor = new Color(0.7f, 0.7f, 0.7f); break;
            case "체력": baseColor = new Color(1.0f, 0.3f, 0.3f); break;   
            case "마나": baseColor = new Color(0.3f, 0.6f, 1.0f); break;   
            case "이동속도": baseColor = new Color(0.2f, 0.8f, 0.2f); break;   
            case "점프력": baseColor = new Color(0.8f, 0.4f, 1.0f); break;   
        }

        string valStr = FormatStatValue(rolled, isFloat);
        string minStr = FormatStatValue(min, isFloat);
        string maxStr = FormatStatValue(max, isFloat);

        string hexColor = ColorUtility.ToHtmlStringRGB(baseColor);
        string sign = (rolled >= 0) ? "+" : "";
        string result = $"<color=#{hexColor}>{statName} {sign}{valStr}</color>";

        if (showDetails)
        {
            result += $" <color=#A9A9A9>({minStr}~{maxStr})</color>";
        }

        return result;
    }

    private static string FormatStatValue(float value, bool forceFractional)
    {
        return forceFractional || !Mathf.Approximately(value, Mathf.Round(value))
            ? value.ToString("F1", CultureInfo.InvariantCulture)
            : Mathf.RoundToInt(value).ToString(CultureInfo.InvariantCulture);
    }

    private void UpdateStatText(bool showDetails)
    {
        if (currentDisplayedItem == null) return;

        List<string> lines = new List<string>();
        foreach (KeyValuePair<string, StatRoll> pair in currentDisplayedItem.stats)
        {
            StatRoll stat = pair.Value;
            if (stat == null || !stat.hasValue) continue;
            lines.Add(GetColoredStatString(StatKeys.DisplayName(pair.Key), stat.value, stat.min, stat.max,
                showDetails, StatKeys.IsFractional(pair.Key)));
        }

        resultStats.text = string.Join("\n", lines);
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
