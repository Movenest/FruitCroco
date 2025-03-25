using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("玩家属性")]
    public int maxHP = 5;
    public int playerAttackCount = 1;

    [Header("等级属性配置")]
    [SerializeField] private int[] maxHPByLevel = { 5, 8, 12, 16, 20 };
    [SerializeField] private float[] moveSpeedByLevel = { 5f, 6f, 7f, 8f, 9f };
    [SerializeField] private int[] attackCountByLevel = { 1, 2, 3, 4, 5 };
    //新增防御属性
    [SerializeField] private int[] defenseByLevel = { 0, 1, 2, 3, 4 }; // 每级防御值
    private int currentDefense; // 当前防御值

    [Header("成长系数")]
    [SerializeField] private float scaleMultiplier = 1.05f;

    // 配置的冲刺倍数
    [Header("冲刺配置")]
    [SerializeField] private float dashMultiplier = 1.5f;

    [Header("攻击设置")]
    [SerializeField] private float attackInterval = 0.5f; // 攻击间隔（秒）
    private float attackTimer;
    private List<EnemyController> contactedEnemies = new List<EnemyController>(); // 接触中的敌人列表

    private PlayerXP playerXP;
    private PlayerMovement playerMovement;
    private Vector3 initialScale;
    private int currentHP;

    void Start()
    {
        InitializeComponents();
        InitializeScale();
        InitializeHealth();
    }

    void Update()
    {
        HandleAttack(); // 新增攻击处理逻辑
    }

    //攻击逻辑
    void HandleAttack()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackInterval && contactedEnemies.Count > 0)
        {
            attackTimer = 0f;
            foreach (var enemy in contactedEnemies.ToArray()) // 使用ToArray防止集合修改异常
            {
                if (enemy != null)
                {
                    enemy.TakeDamage(playerAttackCount);
                    Debug.Log($"玩家攻击敌人，伤害值：{playerAttackCount}");
                }
            }
        }
    }
    void InitializeComponents()
    {
        playerXP = GetComponent<PlayerXP>();
        playerMovement = GetComponent<PlayerMovement>();

        if (playerXP != null)
        {
            playerXP.OnLevelUp += HandleLevelUp;
        }

        ApplyLevelAttributes(1); // 初始化一级属性
    }

    void InitializeScale()
    {
        initialScale = transform.localScale;
        Debug.Log($"初始尺寸：{initialScale}");
    }

    void InitializeHealth()
    {
        currentHP = maxHP;
        Debug.Log($"玩家初始血量：{currentHP}");
    }

    void HandleLevelUp(int newLevel)
    {
        ApplyLevelAttributes(newLevel);
        UpdatePlayerScale(newLevel);
        Debug.Log($"升级效果应用完成，当前等级：{newLevel}");
    }

    void ApplyLevelAttributes(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, maxHPByLevel.Length - 1);

        // 更新血量
        maxHP = maxHPByLevel[index];
        currentHP = maxHP; // 升级回满血

        // 更新攻击力
        playerAttackCount = attackCountByLevel[index];

        // 更新移动速度
        if (playerMovement != null)
        {
            playerMovement.moveSpeed = moveSpeedByLevel[index];
            // 同步更新冲刺倍数
            playerMovement.DashMultiplier = dashMultiplier;
        }
        //更新防御值
        currentDefense = defenseByLevel[index];

        Debug.Log($"等级{level}属性生效 | 血量:{maxHP} 攻击:{playerAttackCount} 防御值:{defenseByLevel[index]} 移速:{moveSpeedByLevel[index]}");
    }

    void UpdatePlayerScale(int level)
    {
        transform.localScale = initialScale * Mathf.Pow(scaleMultiplier, level - 1);
    }

    //收到攻击
    public void TakeDamage(int damageAmount)
    {
        int actualDamage = Mathf.Max(damageAmount - currentDefense, 0);
        currentHP = Mathf.Max(currentHP - actualDamage, 0);

        Debug.Log($"玩家受到 {actualDamage} 点实际伤害（防御抵消 {damageAmount - actualDamage}），剩余血量：{currentHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("玩家死亡");
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null && !contactedEnemies.Contains(enemy))
            {
                contactedEnemies.Add(enemy);
                Debug.Log($"开始持续攻击敌人：{enemy.name}");
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null && contactedEnemies.Contains(enemy))
            {
                contactedEnemies.Remove(enemy);
                Debug.Log($"停止攻击敌人：{enemy.name}");
            }
        }
    }
}