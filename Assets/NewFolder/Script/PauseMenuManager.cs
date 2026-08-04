using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI 패널")]
    public GameObject pausePanel;         // 메인 옵션(일시정지) 창
    public GameObject quitConfirmPanel;   // 종료 확인 창
    public GameObject gachaPanel;         // 가챠 상점 창

    private bool isPaused = false;

    void Update()
    {
        // ESC 키를 눌렀을 때 작동합니다.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // ⭐ 1순위: 인벤토리 창이 열려있다면 가장 먼저 닫습니다!
            if (InventoryManager.instance != null && 
                InventoryManager.instance.inventoryPanel != null && 
                InventoryManager.instance.inventoryPanel.activeSelf)
            {
                InventoryManager.instance.CloseInventoryUI();
                return; // 🛑 인벤토리만 닫고 아래 코드는 더 이상 실행하지 않음
            }

            // ⭐ 2순위: 가챠 창이 열려있다면, 가챠 창을 닫고 일시정지 메뉴로 돌아갑니다.
            if (gachaPanel != null && gachaPanel.activeSelf)
            {
                CloseGachaAndReturn();
                return; // 🛑 가챠 창만 닫고 실행 종료
            }

            // ⭐ 3순위: '종료 확인 창'이 떠 있는 상태에서 ESC를 누르면 확인 창만 닫습니다.
            if (quitConfirmPanel.activeSelf)
            {
                CancelQuit();
                return; // 🛑 확인 창만 닫고 실행 종료
            }
            
            // ⭐ 4순위: 위에 열려있는 팝업이 아무것도 없다면 일시정지 상태를 토글합니다.
            if (isPaused) 
            {
                ResumeGame();
            }
            else 
            {
                PauseGame();
            }
        }
    }

    // 🔴 [내부 로직] 게임 일시정지
    private void PauseGame()
    {
        pausePanel.SetActive(true); 
        Time.timeScale = 0f;        
        isPaused = true;
    }

    // 🟢 1. 게임 다시 진행 (버튼용)
    public void ResumeGame()
    {
        pausePanel.SetActive(false); 
        if (gachaPanel != null) gachaPanel.SetActive(false); 
        
        // ⭐ 게임 재개 시 인벤토리 창도 안전하게 강제로 꺼줍니다.
        if (InventoryManager.instance != null) InventoryManager.instance.CloseInventoryUI(); 
        
        quitConfirmPanel.SetActive(false);
        Time.timeScale = 1f;         
        isPaused = false;
    }

    // 🟡 2. 가챠 상점 창 열기 (기존 옵션 버튼과 연동)
    public void OpenOptions()
    {
        if (gachaPanel != null)
        {
            gachaPanel.SetActive(true);  
            pausePanel.SetActive(false); 
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
        Time.timeScale = 1f; 
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
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}