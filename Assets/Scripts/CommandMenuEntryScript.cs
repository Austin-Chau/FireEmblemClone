using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandMenuEntryScript : MonoBehaviour
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
}

