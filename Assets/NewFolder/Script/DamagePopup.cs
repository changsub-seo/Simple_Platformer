using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public float moveSpeed = 2f;    // 위로 올라가는 속도
    public float destroyTime = 1f;  // 사라지기까지 걸리는 시간

    private TextMeshPro textMesh;
    private Color textColor;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh != null) textColor = textMesh.color;
    }

    void Start()
    {
        // 생성된 지 destroyTime 초가 지나면 자동으로 오브젝트 파괴
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // 매 프레임 위로 이동
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // 서서히 투명해지는 페이드아웃 효과 (선택 사항)
        if (textMesh != null)
        {
            textColor.a -= (1f / destroyTime) * Time.deltaTime;
            textMesh.color = textColor;
        }
    }

    // 외부에서 데미지 숫자를 입력해주는 함수
    public void Setup(int damageAmount)
    {
        if (textMesh != null)
        {
            textMesh.text = damageAmount.ToString();
        }
    }
}