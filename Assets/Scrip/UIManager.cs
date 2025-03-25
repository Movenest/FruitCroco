// UIManager.cs
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI��")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text timeText;
    [SerializeField] private Text targetScoreText;
    [SerializeField] private GameObject gameOverPanel;

    private float timeUpdateInterval = 0.1f; //ʱ��UIˢ��ʱ��
    private float timeSinceLastUpdate;

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager δ��ʼ����");
            return;
        }

        // ��������ʱֻˢ�·�����ʾ
        GameManager.Instance.OnScoreUpdated.AddListener(UpdateAllUI);

        // ��Ϸʧ���¼����ֲ���
        GameManager.Instance.OnGameFailed.AddListener(ShowGameOver);

        // ��ʼUI����
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
        //����ʱ��ˢ��Ƶ��ʱ��
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate >= timeUpdateInterval)
        {
            UpdateTimeUI();
            timeSinceLastUpdate = 0;
        }
    }

    void UpdateAllUI()
    {
        // ���·���
        scoreText.text = $"Score: {GameManager.Instance.CurrentScore}";

        // ����ʱ��
        int minutes = Mathf.FloorToInt(GameManager.Instance.RemainingTime / 60);
        int seconds = Mathf.FloorToInt(GameManager.Instance.RemainingTime % 60);
        timeText.text = $"Time: {minutes:00}:{seconds:00}";
    }

    void UpdateTimeUI()
    {
        // ������ʱ������߼�
        int minutes = Mathf.FloorToInt(GameManager.Instance.RemainingTime / 60);
        int seconds = Mathf.FloorToInt(GameManager.Instance.RemainingTime % 60);
        timeText.text = $"Time: {minutes:00}:{seconds:00}";

        //��̬��ɫ�仯�����10s��ɫ�仯
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

    // ��ť���÷���
    public void OnRestartButtonClicked()
    {
        GameManager.Instance.RestartGame();
        gameOverPanel.SetActive(false);
        UpdateAllUI();
    }
}