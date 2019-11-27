using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _Loader : MonoBehaviour
{
    //prefab to instantiate
    public GameObject gameManager;

    // Start is called before the first frame update
    void Awake()
    {
        if (_GameManager.instance == null)
        {
            Instantiate(gameManager);
        }
    }
}
