using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CommandMenuElement : MenuElement
{
    public CommandMenuElement(GameObject _attachedGameObject, string _labelText, Func<bool> _callback) : base(_attachedGameObject)
    {
        attachedGameObject.transform.Find("Text").GetComponent<Text>().text = _labelText;
        confirmEntryCallback = _callback;
        return;
    }

    public override void SetSelected(bool _selected)
    {
        attachedGameObject.transform.Find("Text").localPosition = new Vector3(_selected ? -30 : 0, 0, 0);
        Selected = _selected;
    }
}
