using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI 패널")]
    public GameObject pausePanel;         
    public GameObject quitConfirmPanel;   
    public GameObject gachaPanel;         
    public GameObject characterInfoPanel; 

    // ⭐ 새롭게 추가된 스킬창(SkillPanel) 변수
    public GameObject skillPanel;

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // ⭐ 추가됨: 스킬창이 열려있다면 가장 먼저 닫고 일시정지로 돌아갑니다.
            if (skillPanel != null && skillPanel.activeSelf)
            {
                CloseSkillAndReturn();
                return; // 🛑 스킬창만 닫고 실행 종료
            }

            if (characterInfoPanel != null && characterInfoPanel.activeSelf)
            {
                CloseCharacterInfoAndReturn();
                return; 
            }

            if (InventoryManager.instance != null && 
                InventoryManager.instance.inventoryPanel != null && 
                InventoryManager.instance.inventoryPanel.activeSelf)
            {
                InventoryManager.instance.CloseInventoryUI();
                return; 
            }

            if (gachaPanel != null && gachaPanel.activeSelf)
            {
                CloseGachaAndReturn();
                return; 
            }

            if (quitConfirmPanel.activeSelf)
            {
                CancelQuit();
                return; 
            }
            
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

    private void PauseGame()
    {
        pausePanel.SetActive(true); 
        Time.timeScale = 0f;        
        isPaused = true;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false); 
        if (gachaPanel != null) gachaPanel.SetActive(false); 
        if (characterInfoPanel != null) characterInfoPanel.SetActive(false); 
        
        // ⭐ 게임 재개 시 스킬창도 안전하게 강제로 꺼줍니다.
        if (skillPanel != null) skillPanel.SetActive(false); 
        
        if (InventoryManager.instance != null) InventoryManager.instance.CloseInventoryUI(); 
        
        quitConfirmPanel.SetActive(false);
        Time.timeScale = 1f;         
        isPaused = false;
    }

    public void OpenOptions()
    {
        if (gachaPanel != null)
        {
            gachaPanel.SetActive(true);  
            pausePanel.SetActive(false); 
        }
    }

    public void CloseGachaAndReturn()
    {
        if (gachaPanel != null) gachaPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void OpenCharacterInfo()
    {
        if (characterInfoPanel != null)
        {
            characterInfoPanel.SetActive(true);
            pausePanel.SetActive(false); 
        }
    }

    public void CloseCharacterInfoAndReturn()
    {
        if (characterInfoPanel != null) characterInfoPanel.SetActive(false);
        pausePanel.SetActive(true); 
    }

    // ⭐ 새롭게 추가된 기능: 스킬창 열기 (일시정지 메뉴의 '스킬창' 버튼용)
    public void OpenSkillPanel()
    {
        if (skillPanel != null)
        {
            skillPanel.SetActive(true);
            pausePanel.SetActive(false); // 일시정지 메뉴 껍데기는 잠깐 숨김
        }
    }

    // ⭐ 새롭게 추가된 기능: 스킬창 닫기 (스킬창 내부의 X 버튼용)
    public void CloseSkillAndReturn()
    {
        if (skillPanel != null) skillPanel.SetActive(false);
        pausePanel.SetActive(true); // 다시 일시정지 메뉴를 띄워줌
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("MainMenuScene"); 
    }

    public void ShowQuitConfirm()
    {
        quitConfirmPanel.SetActive(true);
    }

    public void CancelQuit()
    {
        quitConfirmPanel.SetActive(false);
    }

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