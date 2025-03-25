// PlayerXP.cs �Ľ���
using System;
using UnityEngine;

public class PlayerXP : MonoBehaviour
{
    public event Action<int> OnLevelUp;

    [Header("��������")]
    [SerializeField] private int maxLevel = 5;                // ��߿ɴ�ȼ�
    [SerializeField] private int[] expRequirements = new int[4]; // �����������󣨳����Զ����䣩

    private int currentExp; //��ǰ�ۼƾ���ֵ
    private int currentLevel = 1; //��ǰ��ҵȼ�����ʼ�ȼ�=1��

    // Unity�༭���ص������ڱ��־����������������ȼ�����ͬ��
    private void OnValidate()
    {
        // �Զ��������鳤����maxLevelһ��
        int requiredLength = Mathf.Max(maxLevel - 1, 0); //��ֹ����
        if (expRequirements.Length != requiredLength)
        {
            int oldLength = expRequirements.Length;
            System.Array.Resize(ref expRequirements, requiredLength);

            // ��ʼ������������Ԫ��
            for (int i = oldLength; i < requiredLength; i++)
            {
                expRequirements[i] = (i + 1) * 2; // Ĭ�Ͼ��鹫ʽ
            }
        }
    }
    //��ǰ������Ӿ���
    public void AddXP(int amount)
    {
        currentExp += amount;
        Debug.Log($"��� {amount} ���飬��ǰ���飺{currentExp}");
        CheckLevelUp();
    }

    //����Ƿ�����
    private void CheckLevelUp()
    {
        while (CanReachNextLevel())
        {
            currentLevel++;
            OnLevelUp?.Invoke(currentLevel);
            Debug.Log($"�������ȼ� {currentLevel}!");
        }
    }

    //�ж��Ƿ����������һ��
    private bool CanReachNextLevel()
    {
        if (currentLevel >= maxLevel) return false;
        int required = GetTotalExpRequired(currentLevel + 1);
        return currentExp >= required;
    }

    //��������Ŀ��ȼ���Ҫ���ۼƾ���
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