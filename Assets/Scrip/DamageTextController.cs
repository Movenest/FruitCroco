// DamageTextController.cs
using TMPro;
using UnityEngine;
using System.Collections;

public class DamageTextController : MonoBehaviour
{
    [SerializeField] private float textScaleDuration = 0.5f;
    //[SerializeField] private float textRiseSpeed = 1f;
    [SerializeField] private float totalRiseTime = 1.0f;

    private TMP_Text textComp;
    private Vector3 startPos;

    void Start()
    {
        textComp = GetComponentInChildren<TMP_Text>();
        startPos = transform.position;
        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        // 缩放动画
        float elapsed = 0f;
        while (elapsed < textScaleDuration)
        {
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, elapsed / textScaleDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 上升和淡出动画
        Vector3 targetPos = startPos + Vector3.up * 2.0f;
        float riseElapsed = 0f;
        while (riseElapsed < totalRiseTime)
        {
            float t = riseElapsed / totalRiseTime;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            textComp.alpha = Mathf.Lerp(1, 0, t);
            riseElapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
