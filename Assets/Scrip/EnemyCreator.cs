using UnityEngine;
using System.Collections.Generic;

public class EnemyCreator : MonoBehaviour
{
    [Header("��������")]
    public List<EnemyConfig> enemyConfigs = new List<EnemyConfig>();

    [Header("��ͼ����")]
    private GameObject map;
    private Vector2 creatAreaMin;
    private Vector2 creatAreaMax;

    // ����״̬��¼
    private Dictionary<string, float> lastSpawnTimes = new Dictionary<string, float>(); // ��¼ÿ�ֵ����ϴ����ɵ�ʱ��
    private Dictionary<string, List<GameObject>> activeEnemies = new Dictionary<string, List<GameObject>>(); // ��¼��ǰ�ڳ��ĵ���
    private int globalEnemyCounter = 0; // ȫ�ֵ��˼�����

    void Start()
    {
        InitializeMap(); // ��ȡ��ͼ�߽�
        InitializeDictionaries(); // ��ʼ�����ݽṹ
        InitialSpawn(); // ��ʼ����
    }

    // ͨ�� Tag ���ҵ�ͼ���󲢼���߽�
    void InitializeMap()
    {
        map = GameObject.FindGameObjectWithTag("Map");
        if (map == null)
        {
            Debug.LogError("δ�ҵ���ͼ����");
            return;
        }

        SpriteRenderer mapRenderer = map.GetComponent<SpriteRenderer>();
        Vector2 mapSize = mapRenderer.bounds.size;
        Vector2 mapCenter = mapRenderer.bounds.center;

        creatAreaMax = mapCenter + mapSize / 2;
        creatAreaMin = mapCenter - mapSize / 2;
    }

    void InitializeDictionaries()
    {
        foreach (EnemyConfig config in enemyConfigs)
        {
            lastSpawnTimes[config.enemyType] = Time.time;
            activeEnemies[config.enemyType] = new List<GameObject>();
        }
    }

    void InitialSpawn()
    {
        foreach (EnemyConfig config in enemyConfigs)
        {
            for (int i = 0; i < config.maxCount; i++)
            {
                TrySpawnEnemy(config);
            }
        }
    }

    void Update()
    {
        foreach (EnemyConfig config in enemyConfigs)
        {
            if (ShouldSpawn(config))
            {
                TrySpawnEnemy(config);
                lastSpawnTimes[config.enemyType] = Time.time;
            }
        }
    }

    bool ShouldSpawn(EnemyConfig config)
    {
        return activeEnemies[config.enemyType].Count < config.maxCount
            && Time.time - lastSpawnTimes[config.enemyType] >= config.spawnInterval;
    }

    void TrySpawnEnemy(EnemyConfig config)
    {
        Vector2 spawnPos = GetValidSpawnPosition();
        GameObject enemy = Instantiate(config.prefab, spawnPos, Quaternion.identity);
        SetupEnemy(enemy, config.enemyType);
    }

    Vector2 GetValidSpawnPosition()
    {
        // ���λ����Ч�Լ���߼�����ܿ�ǽ�ڣ�
        return new Vector2(
            Random.Range(creatAreaMin.x, creatAreaMax.x),
            Random.Range(creatAreaMin.y, creatAreaMax.y)
        );
    }

    void SetupEnemy(GameObject enemy, string type)
    {
        globalEnemyCounter++;
        enemy.name = $"{type}_{globalEnemyCounter}";

        EnemyController ec = enemy.GetComponent<EnemyController>();
        if (ec != null)
        {
            ec.SetEnemyType(type); // ���õ�������
            ec.OnEnemyDestroyed += HandleEnemyDestroyed; // ���¼�
        }
        else
        {
            Debug.LogError("����Ԥ����ȱ�� EnemyController ���");
        }

        activeEnemies[type].Add(enemy); // ���뵱ǰ�ڳ��б�
    }

    void HandleEnemyDestroyed(GameObject enemy)
    {
        EnemyController ec = enemy.GetComponent<EnemyController>();
        if (ec != null)
        {
            string type = ec.GetEnemyType(); // ��ȡ��������
            if (activeEnemies.ContainsKey(type))
            {
                activeEnemies[type].Remove(enemy); // ���ڳ��б����Ƴ�
            }
        }
    }
}