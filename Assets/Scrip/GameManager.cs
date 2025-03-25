// GameManager.cs
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("�ؿ�����")]
    public float levelTime = 60f;
    public int targetScore = 50;

    [Header("�¼�")]
    public UnityEvent OnScoreUpdated;     // ���������¼�
    public UnityEvent OnGameFailed;       // ��Ϸʧ���¼�

    private int currentScore;
    private float remainingTime;
    private bool isGameActive;

    public int CurrentScore => currentScore;
    public float RemainingTime => remainingTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeGame();
    }

    public void InitializeGame()
    {
        currentScore = 0;
        remainingTime = levelTime;
        isGameActive = true;

        OnScoreUpdated?.Invoke();
    }

    private void Update()
    {
        if (!isGameActive) return;

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0)
        {
            remainingTime = 0;
            CheckGameResult();
        }
    }

    public void AddScore(int score)
    {
        if (!isGameActive) return;

        currentScore += score;
        OnScoreUpdated?.Invoke();
        CheckGameResult();
    }

    private void CheckGameResult()
    {
        if (remainingTime <= 0 && currentScore < targetScore)
        {
            isGameActive = false;
            OnGameFailed?.Invoke();
        }
    }

    public void RestartGame()
    {
        InitializeGame();
    }
}