using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;          // Ҫ�������Ҷ���
    public float smoothSpeed = 0.125f;// ����ƽ���ȣ�0-1��ԽСԽƽ����
    public Vector3 offset = new Vector3(0, 0, -10f); // �����ƫ����������Z��Ϊ-10��
    private Vector3 velocity = Vector3.zero; //== Vector3(0f, 0f, 0f)

    void LateUpdate()
    {
        if (target == null) return;

        // ����Ŀ��λ�ã�����ԭ��Z�ᣩ
        Vector3 targetPosition = target.position + offset;

        // ʹ��ƽ�������ƶ�
        transform.position = Vector3.SmoothDamp(
            transform.position, //current position
            targetPosition, //Target position
            ref velocity, //Current v
            smoothSpeed * Time.deltaTime   // smoothSpeed:go to Target time(s); deltaTime:usually Default
        );
    }
}