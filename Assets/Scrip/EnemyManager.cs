using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance; // ����ʵ��������ȫ�ַ���

    [System.Serializable]
    public class TypeData // �������͵���������
    {
        public string typeID;         // ���ͱ�ʶ���� EnemyConfig �е� enemyType ��Ӧ��
        public float hpMultiplier = 1f; // HP ���������ڵ�����������ֵ��
        public float speedMultiplier = 1f; // �ٶȱ��������ڵ��������ƶ��ٶȣ�
        public int scoreMultiplier = 1;   // �������������ڵ������˱����ܺ�ĵ÷֣�
    }

    [Header("��������")]
    public List<TypeData> typeConfigs = new List<TypeData>(); // ���е������͵������б�

    private Dictionary<string, TypeData> typeDictionary = new Dictionary<string, TypeData>(); // ���ڿ��ٲ�����������

    void Awake()
    {
        if (Instance == null) // ������ʼ��
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // ��ֹ�ظ�����
        }

        InitializeDictionary(); // ��ʼ�������ֵ�
    }

    void InitializeDictionary()
    {
        // ���б�����ת��Ϊ�ֵ䣬���ڿ��ٲ���
        foreach (TypeData data in typeConfigs)
        {
            typeDictionary[data.typeID] = data;
        }
    }

    // �������ͱ�ʶ��ȡ��������
    public TypeData GetTypeData(string typeID)
    {
        if (typeDictionary.ContainsKey(typeID))
        {
            return typeDictionary[typeID];
        }
        else
        {
            Debug.LogError($"δ�ҵ����� {typeID} ����������");
            return null;
        }
    }
}