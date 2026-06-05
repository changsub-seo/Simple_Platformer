using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// 💡 1. CSV에서 불러온 기본 아이템 정보
[System.Serializable]
public class CSVItemData
{
    public int id;
    public string itemName;
    public string iconName; 
    public int dropWeight;
    public string description;
    
    [HideInInspector] 
    public Sprite loadedIcon; // Resources 폴더에서 불러온 실제 이미지 저장 공간
}

// 💡 2. CSV에서 불러온 접사(접두/접미) 정보
[System.Serializable]
public class AffixData
{
    public int id;
    public string type;       // "Prefix" 또는 "Suffix"
    public string affixName;  // 접사 이름 (예: "네온빛", "블랙월의")
    public string statType;   // 올라가는 스탯 종류 (예: "Attack")
    public int statValue;     // 올라가는 수치 (예: 10)
}

// ⭐ 3. 유저가 실제로 뽑아서 인벤토리에 들어갈 '완성된 고유 아이템'
[System.Serializable]
public class GeneratedItem
{
    public CSVItemData baseItem;       // 원본 아이템 정보
    public AffixData prefix1;          // 첫 번째 접두사 (없을 수도 있음)
    public AffixData prefix2;          // 두 번째 접두사 (없을 수도 있음)
    public AffixData suffix;           // 접미사 (없을 수도 있음)
    
    public string finalName;           // 접사가 다 붙은 최종 이름 (예: 네온빛 과부하된 본체 [오버클럭])
}

public class GachaManager : MonoBehaviour
{
    [Header("가챠 설정")]
    public int pullCost = 10;
    
    [Header("데이터베이스 (CSV 자동 로드)")]
    public List<CSVItemData> itemPool = new List<CSVItemData>();
    public List<AffixData> prefixPool = new List<AffixData>();
    public List<AffixData> suffixPool = new List<AffixData>();

    [Header("UI 연결")]
    public GameObject gachaPanel;
    public GameObject resultPopup;
    public Image resultIcon;
    public TextMeshProUGUI resultName;
    public TextMeshProUGUI resultDesc;

    void Start()
    {
        // 게임 시작 시 두 개의 CSV 데이터를 모두 불러옵니다.
        LoadItemDatabase();
        LoadAffixDatabase();
    }

    // 📂 기본 아이템 CSV 불러오기 및 이미지 자동 매칭
    private void LoadItemDatabase()
    {
        TextAsset csvData = Resources.Load<TextAsset>("ItemDatabase");
        if (csvData == null)
        {
            Debug.LogError("ItemDatabase.csv를 찾을 수 없습니다! Resources 폴더를 확인하세요.");
            return;
        }

        string[] rows = csvData.text.Replace("\r", "").Split('\n');
        
        for (int i = 1; i < rows.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(rows[i])) continue;
            
            string[] cols = rows[i].Split(',');

            CSVItemData newItem = new CSVItemData();
            newItem.id = int.Parse(cols[0]);
            newItem.itemName = cols[1];
            newItem.iconName = cols[2];
            newItem.dropWeight = int.Parse(cols[3]);
            newItem.description = cols[4];

            // 이미지 불러오기 (Resources/Image 폴더 안에 있다면 "Image/" + newItem.iconName)
            newItem.loadedIcon = Resources.Load<Sprite>(newItem.iconName);

            if (newItem.loadedIcon == null)
            {
                Debug.LogWarning($"[경고] '{newItem.iconName}' 이미지를 찾을 수 없습니다!");
            }

            itemPool.Add(newItem);
        }
        Debug.Log($"가챠 기본 아이템 {itemPool.Count}개 세팅 완료!");
    }

    // 📂 접사 CSV 불러오기
    private void LoadAffixDatabase()
    {
        TextAsset csvData = Resources.Load<TextAsset>("AffixDatabase");
        if (csvData == null)
        {
            Debug.LogError("AffixDatabase.csv를 찾을 수 없습니다! Resources 폴더를 확인하세요.");
            return;
        }

        string[] rows = csvData.text.Replace("\r", "").Split('\n');
        for (int i = 1; i < rows.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(rows[i])) continue;
            string[] cols = rows[i].Split(',');

            AffixData newAffix = new AffixData();
            newAffix.id = int.Parse(cols[0]);
            newAffix.type = cols[1];
            newAffix.affixName = cols[2];
            newAffix.statType = cols[3];
            newAffix.statValue = int.Parse(cols[4]);

            // 타입에 따라 접두사 풀과 접미사 풀로 나누어 저장
            if (newAffix.type == "Prefix") prefixPool.Add(newAffix);
            else if (newAffix.type == "Suffix") suffixPool.Add(newAffix);
        }
        Debug.Log($"접두사 {prefixPool.Count}개, 접미사 {suffixPool.Count}개 세팅 완료!");
    }

    // --- UI 열기/닫기 ---
    public void OpenGachaUI() { gachaPanel.SetActive(true); }
    public void CloseGachaUI() { gachaPanel.SetActive(false); }
    public void CloseResultPopup() { resultPopup.SetActive(false); }

    // 🎰 대망의 뽑기 버튼 클릭 시 실행
    public void PullGacha()
    {
        // GameManager 인스턴스를 통해 돈이 충분한지 확인
        if (GameManager.instance != null && GameManager.instance.SpendCoin(pullCost))
        {
            CSVItemData baseItem = GetRandomItem();                // 1. 기본 아이템 뽑기
            GeneratedItem finalItem = GenerateAffixes(baseItem);   // 2. 접사 조합하기
            
            ShowResult(finalItem);                                 // 3. 결과 띄우기
        }
        else if (GameManager.instance == null)
        {
            Debug.LogError("GameManager.instance가 존재하지 않습니다!");
        }
    }

    // 🎲 확률(Weight)에 기반한 기본 아이템 무작위 뽑기
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
        return itemPool[0]; // 혹시 모를 에러 방지용
    }

    // 🎲 확률에 따라 접사를 붙여 최종 고유 아이템 생성
    private GeneratedItem GenerateAffixes(CSVItemData baseItem)
    {
        GeneratedItem item = new GeneratedItem();
        item.baseItem = baseItem;
        
        string fullName = baseItem.itemName;

        // 접두사/접미사가 하나라도 등록되어 있을 때만 실행
        if (prefixPool.Count > 0)
        {
            int prefixRoll = Random.Range(0, 100);
            if (prefixRoll >= 40 && prefixRoll < 80) // 40% 확률로 1개
            {
                item.prefix1 = prefixPool[Random.Range(0, prefixPool.Count)];
                fullName = $"{item.prefix1.affixName} {fullName}";
            }
            else if (prefixRoll >= 80) // 20% 확률로 2개
            {
                item.prefix1 = prefixPool[Random.Range(0, prefixPool.Count)];
                item.prefix2 = prefixPool[Random.Range(0, prefixPool.Count)];
                fullName = $"{item.prefix1.affixName} {item.prefix2.affixName} {fullName}";
            }
        }

        if (suffixPool.Count > 0)
        {
            int suffixRoll = Random.Range(0, 100);
            if (suffixRoll >= 50) // 50% 확률로 1개
            {
                item.suffix = suffixPool[Random.Range(0, suffixPool.Count)];
                fullName = $"{fullName} [{item.suffix.affixName}]"; 
            }
        }

        item.finalName = fullName;
        return item;
    }

    // 🎆 결과창 띄우기
    private void ShowResult(GeneratedItem item)
    {
        if (item.baseItem.loadedIcon != null)
        {
            resultIcon.sprite = item.baseItem.loadedIcon;
        }

        resultName.text = item.finalName; 
        
        // 접사가 하나라도 붙었다면 설명란에 추가 텍스트 표시
        string extraDesc = (item.prefix1 != null || item.suffix != null) 
            ? "\n\n<color=#FFD700>특별한 옵션이 부여된 장비입니다!</color>" 
            : "";
            
        resultDesc.text = item.baseItem.description + extraDesc;
        
        resultPopup.SetActive(true);
    }
}