using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("�����ƶ�")]
    public float moveSpeed = 5f;
    public GameObject Map;

    [Header("�������")]
    [SerializeField] private float _dashMultiplier = 1.5f;  // ��������ٶȱ���
    // �������Ա�¶˽���ֶ�
    public float DashMultiplier
    {
        get => _dashMultiplier;
        set => _dashMultiplier = value;
    }
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.1f;
    [SerializeField] private TrailRenderer dashTrail;


    // �������
    private Rigidbody2D rb;

    // �ƶ����
    private Vector2 movementInput;    // ��һ��������뷽��
    private Vector2 mapMin;           // ��ͼ���±߽�
    private Vector2 mapMax;           // ��ͼ���ϱ߽�
    private Vector2 playerSize;       // �����ײ��ߴ�

    // ���״̬
    private bool isDashing = false;           // �Ƿ����ڳ��
    private float lastDashTime = -Mathf.Infinity; // �ϴγ��ʱ��
    private Vector2 dashDirection;      // ��̷��򣨹�һ����

    void Start()
    {
        InitializeComponents();
        CalculateBoundaries();
    }

    //��ʼ����Ҫ���
    void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();

        // �Զ���ȡ��βЧ���������ѡ��
        if (dashTrail == null)
            dashTrail = GetComponent<TrailRenderer>();
        //��ʼ����βЧ��
        if (dashTrail != null)
            dashTrail.emitting = false;
    }
    
    //�����ͼ�߽�
    void CalculateBoundaries()
    {
        SpriteRenderer mapRenderer = Map.GetComponent<SpriteRenderer>();
        Vector2 mapSize = mapRenderer.bounds.size;
        Vector2 mapCenter = mapRenderer.bounds.center;
        
        //�����ͼ��Ч��Χ
        mapMin = mapCenter - mapSize / 2;
        mapMax = mapCenter + mapSize / 2;

        //��ȡ�����ײ��ߴ�
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            playerSize = playerCollider.bounds.size;
        }
        else
        {
            Debug.LogError("ȱ����ײ�������");
            playerSize = Vector2.one * 0.5f; // Ĭ��ֵ
        }
    }

    void Update()
    {
        HandleMovementInput();
        HandleDashInput();
    }

    //�����ƶ����루�������
    void HandleMovementInput()
    {
        // ��ȡ��׼�����루���ַ����������Ȳ�����1��
        movementInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
    }

    //���������루�ո�
    void HandleDashInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && CanDash())
        {
            StartCoroutine(PerformDash());
        }
    }

    //�ж��Ƿ���Գ��
    bool CanDash()
    {
        return !isDashing &&
               Time.time > lastDashTime + dashCooldown &&
               movementInput != Vector2.zero; // ��Ҫ�ƶ�������ܳ��
    }

    //ִ�г��Э��
    IEnumerator PerformDash()
    {
        // ��ʼ�����״̬
        isDashing = true;
        lastDashTime = Time.time;
        dashDirection = movementInput.normalized;

        // ������βЧ��
        if (dashTrail != null)
            dashTrail.emitting = true;

        // ��̳���ʱ��
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // �������
        isDashing = false;
        if (dashTrail != null)
            dashTrail.emitting = false;
    }

    //�������ѭ��
    void FixedUpdate()
    {
        if (isDashing)
        {
            ApplyDashMovement();
        }
        else
        {
            ApplyNormalMovement();
        }
    }

    //Ӧ�ó����ƶ�
    void ApplyNormalMovement()
    {
        if (!isDashing)
        {
            // ֱ��ͨ���ٶȿ����ƶ�
            rb.velocity = movementInput * moveSpeed;
            rb.position = ClampPosition(rb.position);
        }
    }

    //Ӧ�ó���ƶ�
    void ApplyDashMovement()
    {
        // �޸�Ϊ���ڵ�ǰ�ƶ��ٶȵĶ�̬����
        float currentDashSpeed = moveSpeed * DashMultiplier;
        Vector2 dashVelocity = dashDirection * currentDashSpeed;

        Vector2 newPosition = rb.position + dashVelocity * Time.fixedDeltaTime;
        newPosition = ClampPosition(newPosition);
        rb.MovePosition(newPosition);
    }

    //�������λ���ڵ�ͼ�߽�
    Vector2 ClampPosition(Vector2 position)
    {
        return new Vector2(
            Mathf.Clamp(position.x, mapMin.x + playerSize.x / 2, mapMax.x - playerSize.x / 2),
            Mathf.Clamp(position.y, mapMin.y + playerSize.y / 2, mapMax.y - playerSize.y / 2)
        );
    }
    // PlayerMovement.cs ��������
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            // ��ȫֹͣ���������˶�
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    // ���ӻ����Ա߽�
    void OnDrawGizmosSelected()
    {
        if (Map != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Map.GetComponent<SpriteRenderer>().bounds.center,
                              Map.GetComponent<SpriteRenderer>().bounds.size);
        }
    }
}