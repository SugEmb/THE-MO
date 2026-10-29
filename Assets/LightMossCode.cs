using System.Collections;
using UnityEngine;

public class LightMorseCode : MonoBehaviour
{
    private Light targetLight;
    private Material targetMaterial;

    [Header("Settings")]
    public Color lightColor = Color.red;
    public Color offColor = Color.black;
    public float dotDuration = 0.15f;  // ระยะเวลาที่จุดติด
    public float dashDuration = 0.5f;  // ระยะเวลาที่ขีดติด
    public float symbolPause = 0.15f;  // ช่วงดับสั้นๆ คั่นระหว่างจุด/ขีด
    public float letterPause = 0.8f;   // เว้นระหว่างตัวเลข
    public float wordPause = 15.0f;     // เว้นช่วงยาว 7 วินาทีก่อนเริ่มรอบใหม่

    void Start()
    {
        targetLight = GetComponent<Light>();
        
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            targetMaterial = rend.material;
        }

        StartCoroutine(PlayMorseRoutine());
    }

    IEnumerator PlayMorseRoutine()
    {
        while (true)
        {
            // --- เริ่มต้นรอบใหม่ (2-1-0-5) ---
            
            // เลข 2: ..---
            yield return PlayMorseSequence("..---");
            yield return SetLightState(false, letterPause);

            // เลข 1: .----
            yield return PlayMorseSequence(".----");
            yield return SetLightState(false, letterPause);

            // เลข 0: -----
            yield return PlayMorseSequence("-----");
            yield return SetLightState(false, letterPause);

            // เลข 5: .....
            yield return PlayMorseSequence(".....");

            // --- จบรอบแล้ว! บังคับดับไฟแช่ยาว 7 วินาทีเต็มตรงนี้ ---
            yield return SetLightState(false, wordPause);
        }
    }

    IEnumerator PlayMorseSequence(string morseCode)
    {
        foreach (char symbol in morseCode)
        {
            if (symbol == '.')
            {
                yield return SetLightState(true, dotDuration);
            }
            else if (symbol == '-')
            {
                yield return SetLightState(true, dashDuration);
            }
            
            // ดับไฟช่วงสั้นๆ คั่นแต่ละขีด/จุด
            yield return SetLightState(false, symbolPause);
        }
    }

    IEnumerator SetLightState(bool isOn, float duration)
    {
        if (targetLight != null)
        {
            targetLight.color = lightColor;
            targetLight.enabled = isOn;
        }

        if (targetMaterial != null)
        {
            if (isOn)
            {
                targetMaterial.SetColor("_BaseColor", lightColor); // สำหรับ URP
                targetMaterial.SetColor("_EmissionColor", lightColor);
                targetMaterial.EnableKeyword("_EMISSION");
            }
            else
            {
                targetMaterial.SetColor("_BaseColor", offColor);
                targetMaterial.SetColor("_EmissionColor", offColor);
            }
        }

        yield return new WaitForSeconds(duration);
    }
}