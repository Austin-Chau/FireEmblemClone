using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuEntryScript : MonoBehaviour
{
    private string bodyText = "Lorem Ipsum";
    private string labelText = "Null";
    public GameObject destinationGameObject;

    public void Initialize(string _labelText, string _bodyText, GameObject _destinationGameObject)
    {
        labelText = _labelText;
        bodyText = _bodyText;
        destinationGameObject = _destinationGameObject;
        transform.Find("Text").gameObject.GetComponent<Text>().text = _labelText;
    }

    public bool Active
    {
        get
        {
            return active;
        }
        set
        {
            transform.Find("Text").localPosition = new Vector3(value ? -30 : 0, 0, 0);
            active = value;
            if (value)
            {
                destinationGameObject.transform.Find("Text").GetComponent<Text>().text = bodyText;
                destinationGameObject.transform.Find("Text").localPosition = new Vector3(0, 0, 0);
            }
        }
    }
    private bool active;

    public bool Foreground
    {
        get
        {
            return foreground;
        }
        set
        {
            transform.Find("Text").GetComponent<Text>().material = value ? null : GameManager.instance.GUI.UITextDarkenedMaterial;
            foreground = value;
        }
    }
    private bool foreground;

    private void OnDisable()
    {
        transform.Find("Text").localPosition = new Vector3(0, 0, 0);
        active = false;
    }
}

