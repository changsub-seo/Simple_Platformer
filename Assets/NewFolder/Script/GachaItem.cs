// GachaItem.cs 전체 코드
using UnityEngine;

[System.Serializable]
public class GachaItem
{
    public int id;
    public string itemName;
    public string rarity;
    public int dropWeight;
    public string description;
    public Sprite itemIcon; // 유니티에서 아이콘 이미지를 넣을 공간

    [Header("장비 분류용 추가 데이터")]
    public string ItemType;  // 장비인지 소모품인지 구분 (예: "Equipment")
    public string EquipSlot; // 어느 부위인지 구분 (예: "Head")
}