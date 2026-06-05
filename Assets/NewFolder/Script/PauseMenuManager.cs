using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI 패널")]
    public GameObject pausePanel;         // 메인 옵션(일시정지) 창
    public GameObject quitConfirmPanel;   // 종료 확인 창
    public GameObject gachaPanel;         // ⭐ [추가] 방금 만든 가챠 상점 창

    private bool isPaused = false;

    void Update()
    {
        // ESC 키를 눌렀을 때 작동합니다.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 0. ⭐ 만약 가챠 창이 열려있다면, 가챠 창을 닫고 일시정지 메뉴로 돌아갑니다.
            if (gachaPanel != null && gachaPanel.activeSelf)
            {
                CloseGachaAndReturn();
                return;
            }

            // 1. 만약 '종료 확인 창'이 떠 있는 상태에서 ESC를 누르면 확인 창만 닫습니다.
            if (quitConfirmPanel.activeSelf)
            {
                CancelQuit();
            }
            // 2. 그 외의 경우엔 일시정지 상태를 토글(Toggle)합니다.
            else
            {
                if (isPaused) ResumeGame();
                else PauseGame();
            }
        }
    }

    // 🔴 [내부 로직] 게임 일시정지
    private void PauseGame()
    {
        pausePanel.SetActive(true); // 옵션 창 켜기
        Time.timeScale = 0f;        // 게임 세상의 시간을 완전히 멈춥니다 (0배속)
        isPaused = true;
    }

    // 🟢 1. 게임 다시 진행 (버튼용)
    public void ResumeGame()
    {
        pausePanel.SetActive(false); // 옵션 창 끄기
        if (gachaPanel != null) gachaPanel.SetActive(false); // 가챠창도 안전하게 끄기
        quitConfirmPanel.SetActive(false);
        Time.timeScale = 1f;         // 시간을 다시 정상(1배속)으로 되돌립니다.
        isPaused = false;
    }

    // 🟡 2. 가챠 상점 창 열기 (기존 옵션 버튼과 연동)
    public void OpenOptions()
    {
        if (gachaPanel != null)
        {
            gachaPanel.SetActive(true);  // ⭐ 가챠 창을 켜고
            pausePanel.SetActive(false); // ⭐ 기존 일시정지 메뉴 창은 잠시 숨깁니다.
        }
    }

    // ⭐ 가챠 창에서 [닫기]를 누르거나 ESC를 누르면 다시 일시정지 메뉴로 복귀하는 함수
    public void CloseGachaAndReturn()
    {
        if (gachaPanel != null) gachaPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    // 🔵 3. 메인 화면으로 돌아가기 (버튼용)
    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // 🚨 [매우 중요] 씬을 넘어가기 전에 반드시 시간을 1로 돌려놔야 메인화면이 멈추지 않습니다!
        SceneManager.LoadScene("MainMenuScene"); 
    }

    // 🟣 4. 게임 종료 눌렀을 때 -> 확인 창 띄우기 (버튼용)
    public void ShowQuitConfirm()
    {
        quitConfirmPanel.SetActive(true);
    }

    // 종료 확인 창 - 아니오 (버튼용)
    public void CancelQuit()
    {
        quitConfirmPanel.SetActive(false);
    }

    // 종료 확인 창 - 예 (버튼용)
    public void ConfirmQuit()
    {
        Debug.Log("게임을 완전히 종료합니다.");
        
        // ⭐ 유니티 에디터와 실제 빌드 버전 모두 완벽 종료되도록 보완된 코드
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}