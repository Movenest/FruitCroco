using TMPro;
using UnityEngine;
using System;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    public event Action<GameObject> OnEnemyDestroyed; // 销毁事件
    private string enemyType; // 存储敌人类型
    private int enemyID; // 敌人编号

    [Header("基础属性")]
    public int maxHP = 1; // 基础生命值
    public int expValue = 1; // 基础经验值
    public float enemySpeed = 2f; // 基础移动速度

    [Header("移动范围")]
    private GameObject map; // 地图对象
    private Vector2 mapMin; // 地图左下边界
    private Vector2 mapMax; // 地图右上边界
    private Vector2 enemySize; // 敌人的碰撞体尺寸

    [Header("随机移动距离")]
    public float minMoveDistance = 1f; // 最小移动距离
    public float maxMoveDistance = 5f; // 最大移动距离

    [Header("随机等待时间")]
    public float minWaitTime = 1f; // 最小等待时间
    public float maxWaitTime = 3f; // 最大等待时间

    [Header("分数设置")]
    public int scoreValue = 5; // 每个敌人可获得的分数

    [Header("防御属性")]
    public int baseDefense = 0;  // 基础防御值
    private int actualDefense;    // 实际防御值（考虑类型倍率）

    [Header("攻击属性")] 
    public int attackPower = 1;       // 敌人攻击力
    public float attackInterval = 0.5f; // 攻击间隔（每秒2次）
    private GameObject playerTarget;  // 当前攻击目标
    private Coroutine attackCoroutine; // 攻击协程引用

    // 运行时属性
    private int currentHP;
    private Vector2 randomDirection; // 随机移动方向
    private float moveDistance; // 当前移动距离
    private float currentMoveDistance; // 当前已经移动的距离
    private bool isWaiting = false; // 是否处于等待状态
    private float waitTime; // 当前等待时间

    //动画属性
    [Header("伤害显示")]
    [SerializeField] private GameObject damageTextPrefab; // 伤害数字预制体
    [SerializeField] private Vector3 textOffset = new Vector3(0, 1f, 0); // 文字偏移量

    // 设置敌人编号
    public void SetEnemyID(int id)
    {
        enemyID = id;
    }

    // 获取敌人编号
    public int GetEnemyID()
    {
        return enemyID;
    }

    // 设置敌人类型
    public void SetEnemyType(string type)
    {
        this.enemyType = type;
    }

    // 获取敌人类型
    public string GetEnemyType()
    {
        return enemyType;
    }

    void Start()
    {
        InitializeEnemy();
        Debug.Log($"敌人初始血量：{currentHP}");

        // 动态获取地图对象
        map = GameObject.FindGameObjectWithTag("Map");
        if (map == null)
        {
            Debug.LogError("未找到地图对象，请确保地图对象已添加 'Map' Tag");
            return;
        }

        // 获取地图边界
        CalculateMapBoundaries();
        // 获取敌人的碰撞体尺寸
        CalculateEnemySize();
        // 初始化随机方向和移动距离
        SetRandomDirectionAndDistance();
    }

    void InitializeEnemy()
    {
        currentHP = maxHP;
    }

    private void Update()
    {
        if (isWaiting)
        {
            Wait();
        }
        else
        {
            MoveRandomly();
        }
    }

    // 计算地图边界
    void CalculateMapBoundaries()
    {
        if (map == null)
        {
            Debug.LogError("未找到地图对象");
            return;
        }

        SpriteRenderer mapRenderer = map.GetComponent<SpriteRenderer>();
        if (mapRenderer == null)
        {
            Debug.LogError("地图没有 SpriteRenderer 组件");
            return;
        }

        Vector2 mapSize = mapRenderer.bounds.size;
        Vector2 mapCenter = mapRenderer.bounds.center;

        mapMin = mapCenter - mapSize / 2;
        mapMax = mapCenter + mapSize / 2;
    }

    // 计算敌人的碰撞体尺寸
    void CalculateEnemySize()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            enemySize = collider.bounds.size;
        }
        else
        {
            Debug.LogError("敌人没有 Collider 组件");
        }
    }

    // 设置随机方向和移动距离
    void SetRandomDirectionAndDistance()
    {
        // 生成一个指向地图内部的随机方向
        randomDirection = GetRandomDirectionInsideMap();
        moveDistance = UnityEngine.Random.Range(minMoveDistance, maxMoveDistance); // 随机移动距离
        currentMoveDistance = 0f; // 重置当前移动距离
    }

    // 生成一个指向地图内部的随机方向
    Vector2 GetRandomDirectionInsideMap()
    {
        Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized;
        Vector2 currentPosition = transform.position;

        // 处理X轴边界
        if (currentPosition.x <= mapMin.x + enemySize.x / 2 + 0.5f) // 左边界附近
        {
            direction.x = Mathf.Abs(direction.x); // 强制X方向向右（正值）
        }
        else if (currentPosition.x >= mapMax.x - enemySize.x / 2 - 0.5f) // 右边界附近
        {
            direction.x = -Mathf.Abs(direction.x); // 强制X方向向左（负值）
        }

        // 处理Y轴边界
        if (currentPosition.y <= mapMin.y + enemySize.y / 2 + 0.5f) // 下边界附近
        {
            direction.y = Mathf.Abs(direction.y); // 强制Y方向向上（正值）
        }
        else if (currentPosition.y >= mapMax.y - enemySize.y / 2 - 0.5f) // 上边界附近
        {
            direction.y = -Mathf.Abs(direction.y); // 强制Y方向向下（负值）
        }

        return direction.normalized;
    }

    // 随机移动逻辑
    void MoveRandomly()
    {
        Vector2 oldPosition = transform.position; // 记录移动前的位置

        // 计算移动增量
        Vector2 moveDelta = randomDirection * enemySpeed * Time.deltaTime;
        Vector2 newPosition = (Vector2)oldPosition + moveDelta;

        // 限制移动范围
        newPosition.x = Mathf.Clamp(newPosition.x, mapMin.x + enemySize.x / 2, mapMax.x - enemySize.x / 2);
        newPosition.y = Mathf.Clamp(newPosition.y, mapMin.y + enemySize.y / 2, mapMax.y - enemySize.y / 2);

        // 应用新位置
        transform.position = newPosition;

        // 计算实际移动距离（考虑边界限制）
        Vector2 actualMove = newPosition - oldPosition;
        currentMoveDistance += actualMove.magnitude;

        // 判断是否需要进入等待
        if (IsNearBoundary(newPosition) || currentMoveDistance >= moveDistance)
        {
            StartWait();
        }
    }

    // 开始等待逻辑
    void StartWait()
    {
        isWaiting = true;
        float distanceToBoundary = GetDistanceToBoundary(transform.position);
        waitTime = Mathf.Lerp(minWaitTime, maxWaitTime, distanceToBoundary); // 根据距离调整等待时间
        Debug.Log($"{enemyID}开始等待，等待时间 {waitTime} 秒");
    }

    // 等待逻辑
    void Wait()
    {
        waitTime -= Time.deltaTime; // 减少等待时间
        if (waitTime <= 0)
        {
            isWaiting = false; // 结束等待时间
            SetRandomDirectionAndDistance(); // 重新设置方向和随机距离
            Debug.Log($"{enemyID}结束等待，开始移动");
        }
    }

    // 计算到边界的距离
    float GetDistanceToBoundary(Vector2 position)
    {
        float distanceX = Mathf.Min(
            position.x - (mapMin.x + enemySize.x / 2),
            (mapMax.x - enemySize.x / 2) - position.x
        );

        float distanceY = Mathf.Min(
            position.y - (mapMin.y + enemySize.y / 2),
            (mapMax.y - enemySize.y / 2) - position.y
        );

        return Mathf.Min(distanceX, distanceY) / 10f; // 归一化距离
    }

    // 判断是否接近边界
    bool IsNearBoundary(Vector2 position)
    {
        float boundaryThreshold = 0.5f; // 边界阈值（放宽阈值）
        return position.x <= mapMin.x + enemySize.x / 2 + boundaryThreshold ||
               position.x >= mapMax.x - enemySize.x / 2 - boundaryThreshold ||
               position.y <= mapMin.y + enemySize.y / 2 + boundaryThreshold ||
               position.y >= mapMax.y - enemySize.y / 2 - boundaryThreshold;
    }

    // 受到伤害
    public void TakeDamage(int damageAmount)
    {
        // 计算实际伤害（确保最小为0）
        int actualDamage = Mathf.Max(damageAmount - baseDefense, 0);

        if (actualDamage > 0)
        {
            ShowDamageText(actualDamage);
        }

        currentHP = Mathf.Max(currentHP - actualDamage, 0);
        Debug.Log($"敌人受到 {actualDamage} 点实际伤害（防御抵消 {damageAmount - actualDamage}），剩余血量：{currentHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }


    //伤害动画
    void ShowDamageText(int damage)
    {
        if (damageTextPrefab == null)
        {
            Debug.LogError("damageTextPrefab未赋值！");
            return;
        }

        // 实例化预制体（不要作为子对象！）
        GameObject textObj = Instantiate(damageTextPrefab, transform.position + textOffset, Quaternion.identity);
        Debug.Log($"实例化成功: {textObj != null}");

        // 获取TMP_Text组件（需从子对象中查找）
        TMP_Text textComp = textObj.GetComponentInChildren<TMP_Text>();
        if (textComp != null)
        {
            textComp.text = damage.ToString();

        }
    }

    //敌人攻击逻辑
    void OnTriggerEnter2D(Collider2D other)
    {
        // 检测玩家进入攻击范围
        if (other.CompareTag("Player"))
        {
            playerTarget = other.gameObject;
            attackCoroutine = StartCoroutine(AttackPlayer());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // 检测玩家离开攻击范围
        if (other.CompareTag("Player"))
        {
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
            playerTarget = null;
        }
    }

    IEnumerator AttackPlayer()
    {
        //持续攻击逻辑
        while (playerTarget != null)
        {
            PlayerController player = playerTarget.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(attackPower);
                Debug.Log($"{enemyID}攻击玩家，造成{attackPower}点伤害");
            }
            else
            {
                Debug.Log("没有攻击对象！");
            }
            yield return new WaitForSeconds(attackInterval);
        }
    }
    // 敌人死亡逻辑
    void Die()
    {
        //死亡时停止攻击协程
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }

        // 触发经验值奖励
        PlayerXP playerXP = FindObjectOfType<PlayerXP>();
        if (playerXP != null)
        {
            playerXP.AddXP(expValue);
        }

        // 触发分数奖励
        GameManager.Instance.AddScore(scoreValue);
        OnEnemyDestroyed?.Invoke(gameObject); // 触发销毁事件，传递敌人对象
        //
        Destroy(gameObject);
    }
}