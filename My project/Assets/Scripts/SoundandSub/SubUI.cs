using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class SubUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI subtitleText = default;

    public static SubUI instance;

    private Coroutine clearCoroutine;

    private void Awake()
    {
        instance = this;
        ClearSubtitle();
    }

    public void SetSubtitle(string subtitle, float delay = 0f)
    {
        if (clearCoroutine != null)
        {
            StopCoroutine(clearCoroutine);
            clearCoroutine = null;
        }

     subtitleText.text = subtitle;

        if (delay > 0f)
        {
            clearCoroutine = StartCoroutine(ClearAfterSeconds(delay));
        }
    }

    public void ClearSubtitle()
    {
        if (clearCoroutine != null)
        {
            StopCoroutine(clearCoroutine);
            clearCoroutine = null;
        }

        subtitleText.text = "";
    }

    private IEnumerator ClearAfterSeconds(float delay)
    {
        yield return new WaitForSeconds(delay);

        clearCoroutine = null;
        ClearSubtitle();
    }
}
