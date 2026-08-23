using UnityEngine;

[System.Serializable]
public class ItemData
{
    public int id;
    public string itemName;
    public string rarity;
    public int dropWeight;
    public string description;
    public Sprite itemIcon; 

    [Header("장비 분류용 추가 데이터")]
    public string itemType;  // 장비인지 소모품인지 구분 (예: "Equipment")
    public string equipSlot; // 어느 부위인지 구분 (예: "Head")
}