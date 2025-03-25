using TMPro;
using UnityEngine;
using System;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    public event Action<GameObject> OnEnemyDestroyed; // �����¼�
    private string enemyType; // �洢��������
    private int enemyID; // ���˱��

    [Header("��������")]
    public int maxHP = 1; // ��������ֵ
    public int expValue = 1; // ��������ֵ
    public float enemySpeed = 2f; // �����ƶ��ٶ�

    [Header("�ƶ���Χ")]
    private GameObject map; // ��ͼ����
    private Vector2 mapMin; // ��ͼ���±߽�
    private Vector2 mapMax; // ��ͼ���ϱ߽�
    private Vector2 enemySize; // ���˵���ײ��ߴ�

    [Header("����ƶ�����")]
    public float minMoveDistance = 1f; // ��С�ƶ�����
    public float maxMoveDistance = 5f; // ����ƶ�����

    [Header("����ȴ�ʱ��")]
    public float minWaitTime = 1f; // ��С�ȴ�ʱ��
    public float maxWaitTime = 3f; // ���ȴ�ʱ��

    [Header("��������")]
    public int scoreValue = 5; // ÿ�����˿ɻ�õķ���

    [Header("��������")]
    public int baseDefense = 0;  // ��������ֵ
    private int actualDefense;    // ʵ�ʷ���ֵ���������ͱ��ʣ�

    [Header("��������")] 
    public int attackPower = 1;       // ���˹�����
    public float attackInterval = 0.5f; // ���������ÿ��2�Σ�
    private GameObject playerTarget;  // ��ǰ����Ŀ��
    private Coroutine attackCoroutine; // ����Э������

    // ����ʱ����
    private int currentHP;
    private Vector2 randomDirection; // ����ƶ�����
    private float moveDistance; // ��ǰ�ƶ�����
    private float currentMoveDistance; // ��ǰ�Ѿ��ƶ��ľ���
    private bool isWaiting = false; // �Ƿ��ڵȴ�״̬
    private float waitTime; // ��ǰ�ȴ�ʱ��

    //��������
    [Header("�˺���ʾ")]
    [SerializeField] private GameObject damageTextPrefab; // �˺�����Ԥ����
    [SerializeField] private Vector3 textOffset = new Vector3(0, 1f, 0); // ����ƫ����

    // ���õ��˱��
    public void SetEnemyID(int id)
    {
        enemyID = id;
    }

    // ��ȡ���˱��
    public int GetEnemyID()
    {
        return enemyID;
    }

    // ���õ�������
    public void SetEnemyType(string type)
    {
        this.enemyType = type;
    }

    // ��ȡ��������
    public string GetEnemyType()
    {
        return enemyType;
    }

    void Start()
    {
        InitializeEnemy();
        Debug.Log($"���˳�ʼѪ����{currentHP}");

        // ��̬��ȡ��ͼ����
        map = GameObject.FindGameObjectWithTag("Map");
        if (map == null)
        {
            Debug.LogError("δ�ҵ���ͼ������ȷ����ͼ��������� 'Map' Tag");
            return;
        }

        // ��ȡ��ͼ�߽�
        CalculateMapBoundaries();
        // ��ȡ���˵���ײ��ߴ�
        CalculateEnemySize();
        // ��ʼ�����������ƶ�����
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

    // �����ͼ�߽�
    void CalculateMapBoundaries()
    {
        if (map == null)
        {
            Debug.LogError("δ�ҵ���ͼ����");
            return;
        }

        SpriteRenderer mapRenderer = map.GetComponent<SpriteRenderer>();
        if (mapRenderer == null)
        {
            Debug.LogError("��ͼû�� SpriteRenderer ���");
            return;
        }

        Vector2 mapSize = mapRenderer.bounds.size;
        Vector2 mapCenter = mapRenderer.bounds.center;

        mapMin = mapCenter - mapSize / 2;
        mapMax = mapCenter + mapSize / 2;
    }

    // ������˵���ײ��ߴ�
    void CalculateEnemySize()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            enemySize = collider.bounds.size;
        }
        else
        {
            Debug.LogError("����û�� Collider ���");
        }
    }

    // �������������ƶ�����
    void SetRandomDirectionAndDistance()
    {
        // ����һ��ָ���ͼ�ڲ����������
        randomDirection = GetRandomDirectionInsideMap();
        moveDistance = UnityEngine.Random.Range(minMoveDistance, maxMoveDistance); // ����ƶ�����
        currentMoveDistance = 0f; // ���õ�ǰ�ƶ�����
    }

    // ����һ��ָ���ͼ�ڲ����������
    Vector2 GetRandomDirectionInsideMap()
    {
        Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized;
        Vector2 currentPosition = transform.position;

        // ����X��߽�
        if (currentPosition.x <= mapMin.x + enemySize.x / 2 + 0.5f) // ��߽總��
        {
            direction.x = Mathf.Abs(direction.x); // ǿ��X�������ң���ֵ��
        }
        else if (currentPosition.x >= mapMax.x - enemySize.x / 2 - 0.5f) // �ұ߽總��
        {
            direction.x = -Mathf.Abs(direction.x); // ǿ��X�������󣨸�ֵ��
        }

        // ����Y��߽�
        if (currentPosition.y <= mapMin.y + enemySize.y / 2 + 0.5f) // �±߽總��
        {
            direction.y = Mathf.Abs(direction.y); // ǿ��Y�������ϣ���ֵ��
        }
        else if (currentPosition.y >= mapMax.y - enemySize.y / 2 - 0.5f) // �ϱ߽總��
        {
            direction.y = -Mathf.Abs(direction.y); // ǿ��Y�������£���ֵ��
        }

        return direction.normalized;
    }

    // ����ƶ��߼�
    void MoveRandomly()
    {
        Vector2 oldPosition = transform.position; // ��¼�ƶ�ǰ��λ��

        // �����ƶ�����
        Vector2 moveDelta = randomDirection * enemySpeed * Time.deltaTime;
        Vector2 newPosition = (Vector2)oldPosition + moveDelta;

        // �����ƶ���Χ
        newPosition.x = Mathf.Clamp(newPosition.x, mapMin.x + enemySize.x / 2, mapMax.x - enemySize.x / 2);
        newPosition.y = Mathf.Clamp(newPosition.y, mapMin.y + enemySize.y / 2, mapMax.y - enemySize.y / 2);

        // Ӧ����λ��
        transform.position = newPosition;

        // ����ʵ���ƶ����루���Ǳ߽����ƣ�
        Vector2 actualMove = newPosition - oldPosition;
        currentMoveDistance += actualMove.magnitude;

        // �ж��Ƿ���Ҫ����ȴ�
        if (IsNearBoundary(newPosition) || currentMoveDistance >= moveDistance)
        {
            StartWait();
        }
    }

    // ��ʼ�ȴ��߼�
    void StartWait()
    {
        isWaiting = true;
        float distanceToBoundary = GetDistanceToBoundary(transform.position);
        waitTime = Mathf.Lerp(minWaitTime, maxWaitTime, distanceToBoundary); // ���ݾ�������ȴ�ʱ��
        Debug.Log($"{enemyID}��ʼ�ȴ����ȴ�ʱ�� {waitTime} ��");
    }

    // �ȴ��߼�
    void Wait()
    {
        waitTime -= Time.deltaTime; // ���ٵȴ�ʱ��
        if (waitTime <= 0)
        {
            isWaiting = false; // �����ȴ�ʱ��
            SetRandomDirectionAndDistance(); // �������÷�����������
            Debug.Log($"{enemyID}�����ȴ�����ʼ�ƶ�");
        }
    }

    // ���㵽�߽�ľ���
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

        return Mathf.Min(distanceX, distanceY) / 10f; // ��һ������
    }

    // �ж��Ƿ�ӽ��߽�
    bool IsNearBoundary(Vector2 position)
    {
        float boundaryThreshold = 0.5f; // �߽���ֵ���ſ���ֵ��
        return position.x <= mapMin.x + enemySize.x / 2 + boundaryThreshold ||
               position.x >= mapMax.x - enemySize.x / 2 - boundaryThreshold ||
               position.y <= mapMin.y + enemySize.y / 2 + boundaryThreshold ||
               position.y >= mapMax.y - enemySize.y / 2 - boundaryThreshold;
    }

    // �ܵ��˺�
    public void TakeDamage(int damageAmount)
    {
        // ����ʵ���˺���ȷ����СΪ0��
        int actualDamage = Mathf.Max(damageAmount - baseDefense, 0);

        if (actualDamage > 0)
        {
            ShowDamageText(actualDamage);
        }

        currentHP = Mathf.Max(currentHP - actualDamage, 0);
        Debug.Log($"�����ܵ� {actualDamage} ��ʵ���˺����������� {damageAmount - actualDamage}����ʣ��Ѫ����{currentHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }


    //�˺�����
    void ShowDamageText(int damage)
    {
        if (damageTextPrefab == null)
        {
            Debug.LogError("damageTextPrefabδ��ֵ��");
            return;
        }

        // ʵ����Ԥ���壨��Ҫ��Ϊ�Ӷ��󣡣�
        GameObject textObj = Instantiate(damageTextPrefab, transform.position + textOffset, Quaternion.identity);
        Debug.Log($"ʵ�����ɹ�: {textObj != null}");

        // ��ȡTMP_Text���������Ӷ����в��ң�
        TMP_Text textComp = textObj.GetComponentInChildren<TMP_Text>();
        if (textComp != null)
        {
            textComp.text = damage.ToString();

        }
    }

    //���˹����߼�
    void OnTriggerEnter2D(Collider2D other)
    {
        // �����ҽ��빥����Χ
        if (other.CompareTag("Player"))
        {
            playerTarget = other.gameObject;
            attackCoroutine = StartCoroutine(AttackPlayer());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // �������뿪������Χ
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
        //���������߼�
        while (playerTarget != null)
        {
            PlayerController player = playerTarget.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(attackPower);
                Debug.Log($"{enemyID}������ң����{attackPower}���˺�");
            }
            else
            {
                Debug.Log("û�й�������");
            }
            yield return new WaitForSeconds(attackInterval);
        }
    }
    // ���������߼�
    void Die()
    {
        //����ʱֹͣ����Э��
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }

        // ��������ֵ����
        PlayerXP playerXP = FindObjectOfType<PlayerXP>();
        if (playerXP != null)
        {
            playerXP.AddXP(expValue);
        }

        // ������������
        GameManager.Instance.AddScore(scoreValue);
        OnEnemyDestroyed?.Invoke(gameObject); // ���������¼������ݵ��˶���
        //
        Destroy(gameObject);
    }
}