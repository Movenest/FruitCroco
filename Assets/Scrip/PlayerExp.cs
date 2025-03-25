// PlayerXP.cs 改进版
using System;
using UnityEngine;

public class PlayerXP : MonoBehaviour
{
    public event Action<int> OnLevelUp;

    [Header("升级配置")]
    [SerializeField] private int maxLevel = 5;                // 最高可达等级
    [SerializeField] private int[] expRequirements = new int[4]; // 各级经验需求（长度自动适配）

    private int currentExp; //当前累计经验值
    private int currentLevel = 1; //当前玩家等级（初始等级=1）

    // Unity编辑器回调，用于保持经验配置数组与最大等级设置同步
    private void OnValidate()
    {
        // 自动保持数组长度与maxLevel一致
        int requiredLength = Mathf.Max(maxLevel - 1, 0); //防止负数
        if (expRequirements.Length != requiredLength)
        {
            int oldLength = expRequirements.Length;
            System.Array.Resize(ref expRequirements, requiredLength);

            // 初始化新增的数组元素
            for (int i = oldLength; i < requiredLength; i++)
            {
                expRequirements[i] = (i + 1) * 2; // 默认经验公式
            }
        }
    }
    //当前玩家增加经验
    public void AddXP(int amount)
    {
        currentExp += amount;
        Debug.Log($"获得 {amount} 经验，当前经验：{currentExp}");
        CheckLevelUp();
    }

    //检测是否升级
    private void CheckLevelUp()
    {
        while (CanReachNextLevel())
        {
            currentLevel++;
            OnLevelUp?.Invoke(currentLevel);
            Debug.Log($"升级到等级 {currentLevel}!");
        }
    }

    //判断是否可以升到下一级
    private bool CanReachNextLevel()
    {
        if (currentLevel >= maxLevel) return false;
        int required = GetTotalExpRequired(currentLevel + 1);
        return currentExp >= required;
    }

    //计算升到目标等级需要的累计经验
    private int GetTotalExpRequired(int targetLevel)
    {
        if (targetLevel < 2 || targetLevel > maxLevel) return int.MaxValue;

        int total = 0;
        for (int i = 0; i < targetLevel - 1; i++)
        {
            if (i >= expRequirements.Length) return int.MaxValue;
            total += expRequirements[i];
        }
        return total;
    }

    public int GetCurrentLevel() => currentLevel;
}