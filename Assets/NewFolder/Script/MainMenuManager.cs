using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("로딩 화면 UI")]
    public GameObject loadingPanel;
    public Slider progressBar;

    [Header("페이드(암전) 효과")]
    public CanvasGroup fadeGroup;     // 방금 만든 페이드 판넬
    public float fadeDuration = 0.5f; // 서서히 어두워지는 시간 (0.5초)

    public void GameStart()
    {
        // 일반 로딩 대신, 암전이 포함된 비동기 로딩 코루틴을 시작합니다!
        StartCoroutine(LoadGameSceneWithFade());
    }

    IEnumerator LoadGameSceneWithFade()
    {
        // 1. 버튼을 누르면 다른 걸 못 누르게 막고, 화면을 서서히 까맣게 만듭니다.
        fadeGroup.blocksRaycasts = true; 
        yield return StartCoroutine(Fade(1f)); // 알파값을 1(불투명)로 만듦

        // 2. 화면이 완전 까매진 상태에서 몰래 로딩 화면을 켭니다.
        loadingPanel.SetActive(true);

        // 3. 다시 화면을 서서히 밝게 해서 로딩 화면을 보여줍니다.
        yield return StartCoroutine(Fade(0f)); // 알파값을 0(투명)으로 만듦

        // 4. 비동기(Async) 맵 로딩 시작
        AsyncOperation op = SceneManager.LoadSceneAsync("GameScene");
        op.allowSceneActivation = false; 

        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);
            progressBar.value = progress;

            if (progressBar.value >= 1.0f)
            {
                yield return new WaitForSeconds(0.5f); 
                
                // 5. 로딩이 다 끝났다면, 다시 화면을 서서히 까맣게 만듭니다.
                yield return StartCoroutine(Fade(1f));
                
                // 6. 화면이 완전 까매지면 진짜 게임 씬으로 넘어갑니다!
                op.allowSceneActivation = true; 
            }
            yield return null;
        }
    }

    // ⭐ 투명도를 부드럽게 조절해 주는 핵심 함수
    IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }
        fadeGroup.alpha = targetAlpha;
    }

    public void GameQuit()
    {
        Debug.Log("게임 종료!");
        Application.Quit();
    }
}