using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("�������")]
    public int maxHP = 5;
    public int playerAttackCount = 1;

    [Header("�ȼ���������")]
    [SerializeField] private int[] maxHPByLevel = { 5, 8, 12, 16, 20 };
    [SerializeField] private float[] moveSpeedByLevel = { 5f, 6f, 7f, 8f, 9f };
    [SerializeField] private int[] attackCountByLevel = { 1, 2, 3, 4, 5 };
    //������������
    [SerializeField] private int[] defenseByLevel = { 0, 1, 2, 3, 4 }; // ÿ������ֵ
    private int currentDefense; // ��ǰ����ֵ

    [Header("�ɳ�ϵ��")]
    [SerializeField] private float scaleMultiplier = 1.05f;

    // ���õĳ�̱���
    [Header("�������")]
    [SerializeField] private float dashMultiplier = 1.5f;

    [Header("��������")]
    [SerializeField] private float attackInterval = 0.5f; // ����������룩
    private float attackTimer;
    private List<EnemyController> contactedEnemies = new List<EnemyController>(); // �Ӵ��еĵ����б�

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
        HandleAttack(); // �������������߼�
    }

    //�����߼�
    void HandleAttack()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackInterval && contactedEnemies.Count > 0)
        {
            attackTimer = 0f;
            foreach (var enemy in contactedEnemies.ToArray()) // ʹ��ToArray��ֹ�����޸��쳣
            {
                if (enemy != null)
                {
                    enemy.TakeDamage(playerAttackCount);
                    Debug.Log($"��ҹ������ˣ��˺�ֵ��{playerAttackCount}");
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

        ApplyLevelAttributes(1); // ��ʼ��һ������
    }

    void InitializeScale()
    {
        initialScale = transform.localScale;
        Debug.Log($"��ʼ�ߴ磺{initialScale}");
    }

    void InitializeHealth()
    {
        currentHP = maxHP;
        Debug.Log($"��ҳ�ʼѪ����{currentHP}");
    }

    void HandleLevelUp(int newLevel)
    {
        ApplyLevelAttributes(newLevel);
        UpdatePlayerScale(newLevel);
        Debug.Log($"����Ч��Ӧ����ɣ���ǰ�ȼ���{newLevel}");
    }

    void ApplyLevelAttributes(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, maxHPByLevel.Length - 1);

        // ����Ѫ��
        maxHP = maxHPByLevel[index];
        currentHP = maxHP; // ��������Ѫ

        // ���¹�����
        playerAttackCount = attackCountByLevel[index];

        // �����ƶ��ٶ�
        if (playerMovement != null)
        {
            playerMovement.moveSpeed = moveSpeedByLevel[index];
            // ͬ�����³�̱���
            playerMovement.DashMultiplier = dashMultiplier;
        }
        //���·���ֵ
        currentDefense = defenseByLevel[index];

        Debug.Log($"�ȼ�{level}������Ч | Ѫ��:{maxHP} ����:{playerAttackCount} ����ֵ:{defenseByLevel[index]} ����:{moveSpeedByLevel[index]}");
    }

    void UpdatePlayerScale(int level)
    {
        transform.localScale = initialScale * Mathf.Pow(scaleMultiplier, level - 1);
    }

    //�յ�����
    public void TakeDamage(int damageAmount)
    {
        int actualDamage = Mathf.Max(damageAmount - currentDefense, 0);
        currentHP = Mathf.Max(currentHP - actualDamage, 0);

        Debug.Log($"����ܵ� {actualDamage} ��ʵ���˺����������� {damageAmount - actualDamage}����ʣ��Ѫ����{currentHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("�������");
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
                Debug.Log($"��ʼ�����������ˣ�{enemy.name}");
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
                Debug.Log($"ֹͣ�������ˣ�{enemy.name}");
            }
        }
    }
}