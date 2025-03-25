using System.Collections;
using System.Collections.Generic;
// EnemyConfig.cs
using UnityEngine;

[System.Serializable] // 使该类可以在 Inspector 中显示和编辑
public class EnemyConfig
{
    public string enemyType;       // 敌人类型标识（如 "melee" 或 "ranged"）
    public GameObject prefab;      // 敌人预制体（拖拽到 Inspector 中）
    public int maxCount = 5;       // 该类型敌人的最大在场数量
    public float spawnInterval = 3f; // 生成间隔时间（秒）
    public int spawnWeight = 1;    // 生成权重（用于控制生成概率，权重越高生成概率越大）
}