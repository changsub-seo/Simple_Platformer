using UnityEngine;

// 💡 클래스 이름이 파일 이름(ParallaxBackground.cs)과 대소문자까지 완벽히 일치해야 합니다!
public class ParallaxBackground : MonoBehaviour
{
    [Header("카메라 설정")]
    public Transform cameraTransform;     
    
    // ⭐ 인스펙터 창에 무조건 강제로 띄우고, 0부터 1까지 조절하는 슬라이더 바를 만듭니다!
    [Header("원근감 속도 (1에 가까울수록 카메라를 느리게 따라옴)")]
    [SerializeField] 
    [Range(0f, 1f)]
    private float _parallaxEffectMultiplier = 0.5f; 

    // 외부에서 다른 스크립트가 접근할 수 있도록 프로퍼티로 열어둡니다.
    public float parallaxEffectMultiplier 
    {
        get { return _parallaxEffectMultiplier; }
        set { _parallaxEffectMultiplier = Mathf.Clamp01(value); }
    }

    private Vector3 lastCameraPosition;

    void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        lastCameraPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        
        // 인스펙터에서 조절한 _parallaxEffectMultiplier 수치를 적용합니다.
        transform.position += new Vector3(deltaMovement.x * (1f - _parallaxEffectMultiplier), deltaMovement.y * (1f - _parallaxEffectMultiplier), 0);
        
        lastCameraPosition = cameraTransform.position;
    }
}