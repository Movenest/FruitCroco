using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance; // 单例实例，方便全局访问

    [System.Serializable]
    public class TypeData // 敌人类型的数据配置
    {
        public string typeID;         // 类型标识（与 EnemyConfig 中的 enemyType 对应）
        public float hpMultiplier = 1f; // HP 倍数（用于调整敌人生命值）
        public float speedMultiplier = 1f; // 速度倍数（用于调整敌人移动速度）
        public int scoreMultiplier = 1;   // 分数倍数（用于调整敌人被击败后的得分）
    }

    [Header("类型配置")]
    public List<TypeData> typeConfigs = new List<TypeData>(); // 所有敌人类型的配置列表

    private Dictionary<string, TypeData> typeDictionary = new Dictionary<string, TypeData>(); // 用于快速查找类型配置

    void Awake()
    {
        if (Instance == null) // 单例初始化
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // 防止重复创建
        }

        InitializeDictionary(); // 初始化配置字典
    }

    void InitializeDictionary()
    {
        // 将列表数据转换为字典，便于快速查找
        foreach (TypeData data in typeConfigs)
        {
            typeDictionary[data.typeID] = data;
        }
    }

    // 根据类型标识获取配置数据
    public TypeData GetTypeData(string typeID)
    {
        if (typeDictionary.ContainsKey(typeID))
        {
            return typeDictionary[typeID];
        }
        else
        {
            Debug.LogError($"未找到类型 {typeID} 的配置数据");
            return null;
        }
    }
}