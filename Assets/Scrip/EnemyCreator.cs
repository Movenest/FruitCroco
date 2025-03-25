using UnityEngine;
using System.Collections.Generic;

public class EnemyCreator : MonoBehaviour
{
    [Header("敌人配置")]
    public List<EnemyConfig> enemyConfigs = new List<EnemyConfig>();

    [Header("地图设置")]
    private GameObject map;
    private Vector2 creatAreaMin;
    private Vector2 creatAreaMax;

    // 生成状态记录
    private Dictionary<string, float> lastSpawnTimes = new Dictionary<string, float>(); // 记录每种敌人上次生成的时间
    private Dictionary<string, List<GameObject>> activeEnemies = new Dictionary<string, List<GameObject>>(); // 记录当前在场的敌人
    private int globalEnemyCounter = 0; // 全局敌人计数器

    void Start()
    {
        InitializeMap(); // 获取地图边界
        InitializeDictionaries(); // 初始化数据结构
        InitialSpawn(); // 初始生成
    }

    // 通过 Tag 查找地图对象并计算边界
    void InitializeMap()
    {
        map = GameObject.FindGameObjectWithTag("Map");
        if (map == null)
        {
            Debug.LogError("未找到地图对象");
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
        // 添加位置有效性检测逻辑（如避开墙壁）
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
            ec.SetEnemyType(type); // 设置敌人类型
            ec.OnEnemyDestroyed += HandleEnemyDestroyed; // 绑定事件
        }
        else
        {
            Debug.LogError("敌人预制体缺少 EnemyController 组件");
        }

        activeEnemies[type].Add(enemy); // 加入当前在场列表
    }

    void HandleEnemyDestroyed(GameObject enemy)
    {
        EnemyController ec = enemy.GetComponent<EnemyController>();
        if (ec != null)
        {
            string type = ec.GetEnemyType(); // 获取敌人类型
            if (activeEnemies.ContainsKey(type))
            {
                activeEnemies[type].Remove(enemy); // 从在场列表中移除
            }
        }
    }
}