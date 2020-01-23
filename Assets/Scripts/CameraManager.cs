using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
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
            GameManager.instance.CameraManager = this;
        }
        //DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        MoveToCursor();
    }

    public void MoveToCursor()
    {
        transform.SetPositionAndRotation(GameManager.instance.CursorPosition(), Quaternion.identity);
        transform.Translate(zShift);
    }
}
