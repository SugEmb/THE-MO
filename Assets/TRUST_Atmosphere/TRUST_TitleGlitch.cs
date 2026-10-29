using UnityEngine;

public class TRUST_TitleGlitch : MonoBehaviour
{
    [Header("Glitch Timing")]
    [SerializeField] private float minInterval = 2.5f;
    [SerializeField] private float maxInterval = 6f;
    [SerializeField] private float glitchDuration = 0.12f;

    [Header("Glitch Movement")]
    [SerializeField] private float horizontalOffset = 5f;
    [SerializeField] private float verticalOffset = 1.5f;
    [SerializeField] private float scaleJitter = 0.015f;

    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Vector3 originalScale;

    private float nextGlitchTime;
    private float glitchEndTime;
    private bool isGlitching;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            Debug.LogError(
                "TRUST_TitleGlitch requires a UI object with RectTransform.",
                this
            );

            enabled = false;
            return;
        }

        originalPosition = rectTransform.anchoredPosition;
        originalScale = rectTransform.localScale;

        ScheduleNextGlitch();
    }

    private void Update()
    {
        if (isGlitching)
        {
            if (Time.unscaledTime >= glitchEndTime)
            {
                RestoreTitle();
                ScheduleNextGlitch();
                return;
            }

            float x = Random.Range(
                -horizontalOffset,
                horizontalOffset
            );

            float y = Random.Range(
                -verticalOffset,
                verticalOffset
            );

            rectTransform.anchoredPosition =
                originalPosition + new Vector2(x, y);

            float scaleChange = Random.Range(
                -scaleJitter,
                scaleJitter
            );

            rectTransform.localScale = originalScale
                * (1f + scaleChange);

            return;
        }

        if (Time.unscaledTime >= nextGlitchTime)
        {
            isGlitching = true;
            glitchEndTime =
                Time.unscaledTime + glitchDuration;
        }
    }

    private void ScheduleNextGlitch()
    {
        nextGlitchTime = Time.unscaledTime
            + Random.Range(minInterval, maxInterval);
    }

    private void RestoreTitle()
    {
        isGlitching = false;

        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localScale = originalScale;
    }

    private void OnDisable()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = originalPosition;
            rectTransform.localScale = originalScale;
        }
    }
}