using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBar : MonoBehaviour
{
    [Header("UI 연결")]
    public Image greenBar; 
    public Image redBar;   
    public CanvasGroup canvasGroup; 

    [Header("연출 설정")]
    public float delayBeforeShrink = 0.5f; 
    public float shrinkDuration = 1.0f;    

    [Header("숨김 설정 (Fade Out)")]
    public float hideDelay = 5.0f;    
    public float fadeDuration = 1.0f; 

    private Coroutine shrinkCoroutine;
    private Coroutine fadeCoroutine;

    public void InitializeHealth(int currentHp, int maxHp)
    {
        if (shrinkCoroutine != null) StopCoroutine(shrinkCoroutine);
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        float fillRatio = (float)currentHp / maxHp;

        if (greenBar != null) greenBar.fillAmount = fillRatio;
        if (redBar != null) redBar.fillAmount = fillRatio;

        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }

    public void UpdateHealth(int currentHp, int maxHp)
    {
        float fillRatio = (float)currentHp / maxHp;

        if (greenBar != null) greenBar.fillAmount = fillRatio;
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        if (shrinkCoroutine != null) StopCoroutine(shrinkCoroutine);
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine); 

        shrinkCoroutine = StartCoroutine(ShrinkRedBar(fillRatio));
    }

    private IEnumerator ShrinkRedBar(float targetFill)
    {
        yield return new WaitForSeconds(delayBeforeShrink);

        float startFill = redBar.fillAmount;
        float timeElapsed = 0f;

        while (timeElapsed < shrinkDuration)
        {
            timeElapsed += Time.deltaTime;
            redBar.fillAmount = Mathf.Lerp(startFill, targetFill, timeElapsed / shrinkDuration);
            yield return null; 
        }
        redBar.fillAmount = targetFill;

        fadeCoroutine = StartCoroutine(FadeOutHealthBar());
    }

    private IEnumerator FadeOutHealthBar()
    {
        yield return new WaitForSeconds(hideDelay);

        if (canvasGroup != null)
        {
            float timeElapsed = 0f;
            while (timeElapsed < fadeDuration)
            {
                timeElapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, timeElapsed / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f; 
        }
    }

    // ⭐ 추가된 기능: 부모(더미)가 좌우 반전되어도 체력바는 무조건 정방향을 유지하게 만듭니다!
    void LateUpdate()
    {
        if (transform.parent != null)
        {
            Vector3 scale = transform.localScale;
            // 부모의 방향(Scale.x의 부호)에 맞춰 캔버스의 Scale.x에 -를 곱해 상쇄시킵니다.
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(transform.parent.localScale.x);
            transform.localScale = scale;
        }
    }
}