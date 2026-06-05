using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 💡 유니티 인스펙터나 다른 스크립트에서 볼 수 있게 직렬화합니다.
[System.Serializable]
public class GachaItem
{
    public int id;              // 아이템 고유 번호 (예: 101)
    public string itemName;     // 아이템 이름
    public string rarity;       // 등급 (Normal, Rare, Epic)
    public int dropWeight;      // 뽑기 확률 가중치 (숫자가 클수록 잘 나옴)
    public string description;  // 아이템 설명
}