using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitHealthBar : MonoBehaviour
{
    Slider slider;
    int maxHealth;

    //Used to move healthbar on canvas with unit in world space
    Unit unit;

    const float unitOffset = 20f;
    /// <summary>
    /// Initialize health numbers with health at max
    /// </summary>
    /// <param name="maxHealth">Max HP for health bar</param>
    public void Initialize(int maxHealth, Unit _unit)
    {
        slider = GetComponent<Slider>();
        slider.maxValue = maxHealth;
        slider.value = maxHealth;
        unit = _unit;
        transform.parent = GameObject.Find("UnitHealthBars").transform;
    }

    /// <summary>
    /// Initialize health numbers with health starting at less than max
    /// </summary>
    /// <param name="maxHealth">Max HP for health bar</param>
    /// <param name="startingHealth">Starting HP for Healthbar</param>
    public void Initialize(int maxHealth, int startingHealth, Unit _unit)
    {
        slider = GetComponent<Slider>();
        slider.maxValue = maxHealth;
        slider.value = startingHealth;
        unit = _unit;
        transform.parent = GameObject.Find("UnitHealthBars").transform;
    }

    private void Update()
    {
        if(unit != null)
        {
            transform.position = UnityEngine.Camera.main.WorldToScreenPoint(unit.transform.position) + new Vector3(0, unitOffset, 0);
        }
    }

    public void ChangeHealthbar(int health)
    {
        if (slider.maxValue >= health &&
            slider.minValue <= health)
            slider.value = health;
        else if (health > slider.maxValue)
            slider.value = slider.maxValue;
        else
            slider.value = 0;
    }
}
