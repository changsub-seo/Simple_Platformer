// ItemDatabaseManager.cs 전체 코드
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDatabaseManager : MonoBehaviour
{
    [Header("불러온 아이템 목록")]
    public List<GachaItem> allItems = new List<GachaItem>();

    void Start()
    {
        LoadItemCSV();
    }

    void LoadItemCSV()
    {
        // 1. Resources 폴더에서 'ItemDatabase'라는 이름의 텍스트 파일을 불러옵니다.
        TextAsset csvData = Resources.Load<TextAsset>("ItemDatabase");

        if (csvData == null)
        {
            Debug.LogError("CSV 파일을 찾을 수 없습니다! Resources 폴더를 확인하세요.");
            return;
        }

        // 2. 엔터키를 기준으로 줄 나누기
        string[] rows = csvData.text.Replace("\r", "").Split('\n');

        // 3. 첫 번째 줄(i=0)은 헤더이므로 i=1부터 시작
        for (int i = 1; i < rows.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(rows[i])) continue; 

            // 4. 쉼표(,)를 기준으로 칸 나누기
            string[] columns = rows[i].Split(',');

            GachaItem newItem = new GachaItem();
            newItem.id = int.Parse(columns[0]);
            newItem.itemName = columns[1];
            newItem.rarity = columns[2];
            newItem.dropWeight = int.Parse(columns[3]);
            newItem.description = columns[4];
            
            // ⭐ 이 두 줄이 누락되어서 발생한 에러입니다! CSV의 5번째와 6번째 칸(ItemType, EquipSlot) 데이터를 담아줍니다.
            if (columns.Length > 5) newItem.ItemType = columns[5];
            if (columns.Length > 6) newItem.EquipSlot = columns[6];

            allItems.Add(newItem);
        }

        Debug.Log($"총 {allItems.Count}개의 가챠 아이템 데이터를 성공적으로 불러왔습니다!");
    }
}