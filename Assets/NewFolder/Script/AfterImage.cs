using UnityEngine;

public class AfterImage : MonoBehaviour
{
    private SpriteRenderer sr;
    private float alpha;
    
    // 잔상이 사라지는 속도 (작을수록 빨리 사라짐)
    public float fadeSpeed = 3f; 

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        alpha = sr.color.a; // 현재 투명도(알파값) 저장
    }

    void Update()
    {
        // 매 프레임마다 투명도(알파값)를 줄여서 서서히 사라지게 만듭니다.
        alpha -= fadeSpeed * Time.deltaTime;
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);

        // 완전히 투명해지면 유니티 상에서 삭제(파괴)합니다.
        if (alpha <= 0f)
        {
            Destroy(gameObject);
        }
    }
}