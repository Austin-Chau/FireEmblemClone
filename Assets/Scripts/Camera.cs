using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{

    //prefab to instantiate
    public GameObject gameManager;

    public float panSensitivity = 5f;
    private Vector3 zShift = new Vector3(0, 0, -10);

    //create the gamemanager
    void Awake()
    {
        if (GameManager.instance == null)
        {
            Instantiate(gameManager);
        }
    }

    void Update()
    {
        transform.SetPositionAndRotation(GameManager.instance.cursorPosition, Quaternion.identity);
        transform.Translate(zShift);
    }
}
