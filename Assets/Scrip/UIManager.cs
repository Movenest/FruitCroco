// UIManager.cs
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI绑定")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text timeText;
    [SerializeField] private Text targetScoreText;
    [SerializeField] private GameObject gameOverPanel;

    private float timeUpdateInterval = 0.1f; //时间UI刷新时间
    private float timeSinceLastUpdate;

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager 未初始化！");
            return;
        }

        // 分数更新时只刷新分数显示
        GameManager.Instance.OnScoreUpdated.AddListener(UpdateAllUI);

        // 游戏失败事件保持不变
        GameManager.Instance.OnGameFailed.AddListener(ShowGameOver);

        // 初始UI设置
        InitializeUI();
    }

    void InitializeUI()
    {
        targetScoreText.text = $"Target: {GameManager.Instance.targetScore}";
        UpdateAllUI();
        gameOverPanel.SetActive(false);
    }
    private void Update()
    {
        //增加时间刷新频率时间
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate >= timeUpdateInterval)
        {
            UpdateTimeUI();
            timeSinceLastUpdate = 0;
        }
    }

    void UpdateAllUI()
    {
        // 更新分数
        scoreText.text = $"Score: {GameManager.Instance.CurrentScore}";

        // 更新时间
        int minutes = Mathf.FloorToInt(GameManager.Instance.RemainingTime / 60);
        int seconds = Mathf.FloorToInt(GameManager.Instance.RemainingTime % 60);
        timeText.text = $"Time: {minutes:00}:{seconds:00}";
    }

    void UpdateTimeUI()
    {
        // 独立的时间更新逻辑
        int minutes = Mathf.FloorToInt(GameManager.Instance.RemainingTime / 60);
        int seconds = Mathf.FloorToInt(GameManager.Instance.RemainingTime % 60);
        timeText.text = $"Time: {minutes:00}:{seconds:00}";

        //动态颜色变化，最后10s颜色变化
        if (GameManager.Instance.RemainingTime <= 10)
        {
            float lerpValue = Mathf.PingPong(Time.time, 0.5f);
            timeText.color = Color.Lerp(Color.white, Color.red, lerpValue);
        }
        else
        {
            timeText.color = Color.white;
        }
    }

    void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        timeText.color = Color.red;
    }

    // 按钮调用方法
    public void OnRestartButtonClicked()
    {
        GameManager.Instance.RestartGame();
        gameOverPanel.SetActive(false);
        UpdateAllUI();
    }
}