using System.Collections;
using System.Collections.Generic;
// EnemyConfig.cs
using UnityEngine;

[System.Serializable] // ʹ��������� Inspector ����ʾ�ͱ༭
public class EnemyConfig
{
    public string enemyType;       // �������ͱ�ʶ���� "melee" �� "ranged"��
    public GameObject prefab;      // ����Ԥ���壨��ק�� Inspector �У�
    public int maxCount = 5;       // �����͵��˵�����ڳ�����
    public float spawnInterval = 3f; // ���ɼ��ʱ�䣨�룩
    public int spawnWeight = 1;    // ����Ȩ�أ����ڿ������ɸ��ʣ�Ȩ��Խ�����ɸ���Խ��
}