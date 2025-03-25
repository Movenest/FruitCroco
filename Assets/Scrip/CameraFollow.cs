using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;          // 要跟随的玩家对象
    public float smoothSpeed = 0.125f;// 跟随平滑度（0-1，越小越平滑）
    public Vector3 offset = new Vector3(0, 0, -10f); // 摄像机偏移量（保持Z轴为-10）
    private Vector3 velocity = Vector3.zero; //== Vector3(0f, 0f, 0f)

    void LateUpdate()
    {
        if (target == null) return;

        // 计算目标位置（保持原有Z轴）
        Vector3 targetPosition = target.position + offset;

        // 使用平滑阻尼移动
        transform.position = Vector3.SmoothDamp(
            transform.position, //current position
            targetPosition, //Target position
            ref velocity, //Current v
            smoothSpeed * Time.deltaTime   // smoothSpeed:go to Target time(s); deltaTime:usually Default
        );
    }
}