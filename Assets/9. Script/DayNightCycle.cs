using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public Light directionalLight;
    public float dayDuration = 120f; // Dagens längd i sekunder
    private float time;
    private float initialIntensity;

    void Start()
    {
        initialIntensity = directionalLight.intensity;
    }
    void Update()
    {
        time += Time.deltaTime;
        float dayFraction = time / dayDuration;
        float angle = Mathf.Lerp(0, 360, dayFraction);
        directionalLight.transform.rotation = Quaternion.Euler(new Vector3(angle, 0, 0));

        if (dayFraction >= 1)
        {
            time = 0; // Återställ tiden när en dag är över
        }
        UpdateLighting(dayFraction);
    }
    void UpdateLighting(float dayFraction)
    {
        if (dayFraction <= 0.25f || dayFraction >= 0.75f)
        {
            //Natt
            directionalLight.intensity = Mathf.Lerp(0, initialIntensity, Mathf.Sin(dayFraction * Mathf.PI));
        }
        else
        {
            // Dag
            directionalLight.intensity = initialIntensity;
        }
    }
}