using UnityEngine;

// ⭐ 유니티 에디터의 우클릭 메뉴에 "아이템 만들기" 버튼을 추가해 줍니다!
[CreateAssetMenu(fileName = "New Item", menuName = "Gacha/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;         // 아이템 이름
    public Sprite itemIcon;         // 아이템 이미지(아이콘)
    [TextArea] 
    public string description;      // 아이템 설명
    
    [Header("가챠 확률 (숫자가 클수록 잘 나옴)")]
    public int dropWeight = 10;     // 예: 흔함=50, 희귀=10, 전설=1
}