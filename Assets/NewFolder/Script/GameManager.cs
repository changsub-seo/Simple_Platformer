using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("재화(지갑) 설정")]
    public int currentCoin = 0;           
    public TextMeshProUGUI coinText;      

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject); 
    }

    void Start()
    {
        // ⭐ 1. 게임이 시작되자마자 내 금고(PlayerPrefs)에서 저장된 돈을 불러옵니다!
        LoadCoin();
        
        UpdateCoinUI();
    }

    public void AddCoin(int amount)
    {
        currentCoin += amount; 
        UpdateCoinUI();
        
        // ⭐ 2. 돈을 얻었으니 잃어버리지 않게 즉시 금고에 저장합니다!
        SaveCoin();
    }

    public bool SpendCoin(int amount)
    {
        if (currentCoin >= amount)
        {
            currentCoin -= amount;
            UpdateCoinUI();
            
            // ⭐ 3. 돈을 썼으니 줄어든 돈도 즉시 금고에 덮어씌워 저장합니다!
            SaveCoin();
            return true; 
        }
        else
        {
            Debug.Log("돈이 부족합니다!");
            return false; 
        }
    }

    private void UpdateCoinUI()
    {
        if (coinText != null)
        {
            coinText.text = "보유 돈 : " + currentCoin.ToString();
        }
    }

    // ==========================================
    // 💾 세이브 & 로드 (PlayerPrefs 마법)
    // ==========================================
    private void SaveCoin()
    {
        // "SavedCoin"이라는 이름의 라벨을 붙여서 현재 코인 숫자를 기기에 기록합니다.
        PlayerPrefs.SetInt("SavedCoin", currentCoin);
        PlayerPrefs.Save(); // 기록을 확정(도장 쾅!) 짓습니다.
    }

    private void LoadCoin()
    {
        // "SavedCoin"이라는 라벨이 붙은 숫자가 있는지 찾아봅니다.
        // 만약 처음 게임을 켜서 저장된 게 없다면, 기본값인 0을 가져옵니다.
        currentCoin = PlayerPrefs.GetInt("SavedCoin", 0);
    }

    void OnValidate()
    {
        UpdateCoinUI();
    }
}