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
        // 1. Resources 폴더에서 'ItemDatabase'라는 이름의 텍스트 파일을 불러옵니다. (.csv 확장자는 뺍니다)
        TextAsset csvData = Resources.Load<TextAsset>("ItemDatabase");

        if (csvData == null)
        {
            Debug.LogError("CSV 파일을 찾을 수 없습니다! Resources 폴더를 확인하세요.");
            return;
        }

        // 2. 엔터키(\n)를 기준으로 줄을 나눕니다. (맥과 윈도우의 차이인 \r은 제거해줍니다)
        string[] rows = csvData.text.Replace("\r", "").Split('\n');

        // 3. 첫 번째 줄(i=0)은 표의 제목(헤더)이므로 i=1부터 시작합니다.
        for (int i = 1; i < rows.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(rows[i])) continue; // 빈 줄은 무시

            // 4. 쉼표(,)를 기준으로 칸을 나눕니다.
            string[] columns = rows[i].Split(',');

            // 5. 나눈 데이터를 GachaItem 그릇에 예쁘게 담아서 리스트에 추가합니다.
            GachaItem newItem = new GachaItem();
            newItem.id = int.Parse(columns[0]);
            newItem.itemName = columns[1];
            newItem.rarity = columns[2];
            newItem.dropWeight = int.Parse(columns[3]);
            newItem.description = columns[4];

            allItems.Add(newItem);
        }

        Debug.Log($"총 {allItems.Count}개의 가챠 아이템 데이터를 성공적으로 불러왔습니다!");
    }
}