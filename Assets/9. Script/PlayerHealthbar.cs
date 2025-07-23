using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthbar : MonoBehaviour
{
    public Slider healthSlider;

    public void SetHealth(float health)
    {
        healthSlider.value = health;
    }
}
