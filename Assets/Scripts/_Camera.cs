using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _Camera : MonoBehaviour
{

    //prefab to instantiate
    public GameObject gameManager;

    public float panSensitivity = 5f;

    //create the gamemanager
    void Awake()
    {
        if (_GameManager.instance == null)
        {
            Instantiate(gameManager);
        }
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal") * Time.deltaTime;
        float y = Input.GetAxisRaw("Vertical") * Time.deltaTime;

        transform.Translate(x*panSensitivity, y*panSensitivity,0);
    }
}
