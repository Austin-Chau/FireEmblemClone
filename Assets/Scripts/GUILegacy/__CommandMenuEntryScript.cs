using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandMenuElementScript : MonoBehaviour
{
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
            transform.Find("Text").GetComponent<Text>().material = value ? null : GameManager.instance.GUIManager.UITextDarkenedMaterial;
            foreground = value;
        }
    }
    private bool foreground;
}

