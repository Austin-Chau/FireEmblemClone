using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CommandMenuElement : MenuElementMonoBehavior, IMenuElement
{
    public override void SetSelected(bool _selected)
    {
        gameObject.transform.Find("Text").localPosition = new Vector3(_selected ? -30 : 0, 0, 0);
        Selected = _selected;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_label">The text for the label of the element </param>
    /// <param name="_confirmCallback">The callback performed when the element is selected</param>
    public void InitiateProperties(string _label, Func<bool> _confirmCallback)
    {
        gameObject.transform.Find("Text").GetComponent<Text>().text = _label;
        confirmElementCallback = _confirmCallback;
    }
}
