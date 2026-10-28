
using UnityEngine;
using System.Collections;

public class BlinkingLight : MonoBehaviour
{
    public Light targetLight;
    public float onTime = 0.3f;
    public float offTime = 0.3f;

    void Start()
    {
        if (targetLight == null)
        {
            targetLight = GetComponent<Light>();
        }

        StartCoroutine(Blink());
    }

    IEnumerator Blink()
    {
        while (true)
        {
            targetLight.enabled = true;
            yield return new WaitForSeconds(onTime);

            targetLight.enabled = false;
            yield return new WaitForSeconds(offTime);
        }
    }
}
