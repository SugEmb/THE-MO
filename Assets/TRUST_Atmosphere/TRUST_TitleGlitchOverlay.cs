using UnityEngine;
using TMPro;

public class TRUST_TitleGlitchOverlay : MonoBehaviour
{
    [Header("Glitch Timing")]
    [SerializeField] private float minInterval = 2.5f;
    [SerializeField] private float maxInterval = 6f;
    [SerializeField] private float glitchDuration = 0.12f;

    [Header("Glitch Appearance")]
    [SerializeField] private Color glitchColor =
        new Color(1f, 0.25f, 0.35f, 0.7f);

    [SerializeField] private float horizontalOffset = 5f;
    [SerializeField] private float verticalOffset = 1f;

    private TMP_Text glitchText;
    private RectTransform rectTransform;

    private Vector2 originalPosition;
    private Color originalColor;

    private float nextGlitchTime;
    private float glitchEndTime;
    private bool isGlitching;

    private void Awake()
    {
        glitchText = GetComponent<TMP_Text>();
        rectTransform = GetComponent<RectTransform>();

        if (glitchText == null || rectTransform == null)
        {
            Debug.LogError(
                "TRUST_TitleGlitchOverlay requires a TMP text component.",
                this
            );

            enabled = false;
            return;
        }

        originalPosition = rectTransform.anchoredPosition;
        originalColor = glitchText.color;

        SetOverlayVisible(false);
        ScheduleNextGlitch();
    }

    private void Update()
    {
        if (!isGlitching)
        {
            if (Time.unscaledTime >= nextGlitchTime)
            {
                StartGlitch();
            }

            return;
        }

        if (Time.unscaledTime >= glitchEndTime)
        {
            StopGlitch();
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
    }

    private void StartGlitch()
    {
        isGlitching = true;
        glitchEndTime = Time.unscaledTime + glitchDuration;

        glitchText.color = glitchColor;
        SetOverlayVisible(true);
    }

    private void StopGlitch()
    {
        isGlitching = false;

        rectTransform.anchoredPosition = originalPosition;
        glitchText.color = originalColor;

        SetOverlayVisible(false);
    }

    private void ScheduleNextGlitch()
    {
        nextGlitchTime = Time.unscaledTime
            + Random.Range(minInterval, maxInterval);
    }

    private void SetOverlayVisible(bool visible)
    {
        if (glitchText == null)
            return;

        Color color = glitchText.color;
        color.a = visible ? glitchColor.a : 0f;
        glitchText.color = color;
    }

    private void OnDisable()
    {
        if (rectTransform != null)
            rectTransform.anchoredPosition = originalPosition;

        if (glitchText != null)
            glitchText.color = new Color(
                originalColor.r,
                originalColor.g,
                originalColor.b,
                0f
            );

        isGlitching = false;
    }
}