using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("基础移动")]
    public float moveSpeed = 5f;
    public GameObject Map;

    [Header("冲刺设置")]
    [SerializeField] private float _dashMultiplier = 1.5f;  // 新增冲刺速度倍数
    // 公共属性暴露私有字段
    public float DashMultiplier
    {
        get => _dashMultiplier;
        set => _dashMultiplier = value;
    }
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.1f;
    [SerializeField] private TrailRenderer dashTrail;


    // 组件引用
    private Rigidbody2D rb;

    // 移动相关
    private Vector2 movementInput;    // 归一化后的输入方向
    private Vector2 mapMin;           // 地图左下边界
    private Vector2 mapMax;           // 地图右上边界
    private Vector2 playerSize;       // 玩家碰撞体尺寸

    // 冲刺状态
    private bool isDashing = false;           // 是否正在冲刺
    private float lastDashTime = -Mathf.Infinity; // 上次冲刺时间
    private Vector2 dashDirection;      // 冲刺方向（归一化）

    void Start()
    {
        InitializeComponents();
        CalculateBoundaries();
    }

    //初始化必要组件
    void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();

        // 自动获取拖尾效果组件（可选）
        if (dashTrail == null)
            dashTrail = GetComponent<TrailRenderer>();
        //初始化拖尾效果
        if (dashTrail != null)
            dashTrail.emitting = false;
    }
    
    //计算地图边界
    void CalculateBoundaries()
    {
        SpriteRenderer mapRenderer = Map.GetComponent<SpriteRenderer>();
        Vector2 mapSize = mapRenderer.bounds.size;
        Vector2 mapCenter = mapRenderer.bounds.center;
        
        //计算地图有效范围
        mapMin = mapCenter - mapSize / 2;
        mapMax = mapCenter + mapSize / 2;

        //获取玩家碰撞体尺寸
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            playerSize = playerCollider.bounds.size;
        }
        else
        {
            Debug.LogError("缺少碰撞体组件！");
            playerSize = Vector2.one * 0.5f; // 默认值
        }
    }

    void Update()
    {
        HandleMovementInput();
        HandleDashInput();
    }

    //处理移动输入（方向键）
    void HandleMovementInput()
    {
        // 获取标准化输入（保持方向向量长度不超过1）
        movementInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
    }

    //处理冲刺输入（空格）
    void HandleDashInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && CanDash())
        {
            StartCoroutine(PerformDash());
        }
    }

    //判断是否可以冲刺
    bool CanDash()
    {
        return !isDashing &&
               Time.time > lastDashTime + dashCooldown &&
               movementInput != Vector2.zero; // 需要移动方向才能冲刺
    }

    //执行冲刺协程
    IEnumerator PerformDash()
    {
        // 初始化冲刺状态
        isDashing = true;
        lastDashTime = Time.time;
        dashDirection = movementInput.normalized;

        // 激活拖尾效果
        if (dashTrail != null)
            dashTrail.emitting = true;

        // 冲刺持续时间
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // 结束冲刺
        isDashing = false;
        if (dashTrail != null)
            dashTrail.emitting = false;
    }

    //物理更新循环
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

    //应用常规移动
    void ApplyNormalMovement()
    {
        if (!isDashing)
        {
            // 直接通过速度控制移动
            rb.velocity = movementInput * moveSpeed;
            rb.position = ClampPosition(rb.position);
        }
    }

    //应用冲刺移动
    void ApplyDashMovement()
    {
        // 修改为基于当前移动速度的动态计算
        float currentDashSpeed = moveSpeed * DashMultiplier;
        Vector2 dashVelocity = dashDirection * currentDashSpeed;

        Vector2 newPosition = rb.position + dashVelocity * Time.fixedDeltaTime;
        newPosition = ClampPosition(newPosition);
        rb.MovePosition(newPosition);
    }

    //限制玩家位置在地图边界
    Vector2 ClampPosition(Vector2 position)
    {
        return new Vector2(
            Mathf.Clamp(position.x, mapMin.x + playerSize.x / 2, mapMax.x - playerSize.x / 2),
            Mathf.Clamp(position.y, mapMin.y + playerSize.y / 2, mapMax.y - playerSize.y / 2)
        );
    }
    // PlayerMovement.cs 新增代码
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            // 完全停止所有物理运动
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    // 可视化调试边界
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