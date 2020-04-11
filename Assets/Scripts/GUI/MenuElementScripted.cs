using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuElementScripted : IMenuElement
{
    protected GameObject attachedGameObject;
    protected Func<bool> confirmElementCallback;

    protected Dictionary<AdjacentDirection, MenuBufferingType> bufferingDict = new Dictionary<AdjacentDirection, MenuBufferingType>();

    public bool Active { get; protected set; }
    public bool Selected { get; protected set; }
    public bool Foreground { get; protected set; }

    private Dictionary<AdjacentDirection, IMenuElement> adjacentMenus = new Dictionary<AdjacentDirection, IMenuElement>();

    public MenuElementScripted(GameObject _attachedGameObject)
    {
        attachedGameObject = _attachedGameObject;
    }

    public bool ConfirmElement()
    {
        return (bool)confirmElementCallback?.Invoke();
    }

    public MenuBufferingType GetBufferScrolling(AdjacentDirection _direction)
    {
        if (!bufferingDict.ContainsKey(_direction))
        {
            return MenuBufferingType.Full;
        }
        return bufferingDict[_direction];
    }

    public IMenuElement GetAdjacentMenuElement(AdjacentDirection _direction)
    {
        if (!adjacentMenus.ContainsKey(_direction))
        {
            return this;
        }
        return adjacentMenus[_direction];
    }

    public void SetAdjacentMenu(AdjacentDirection _direction, IMenuElement _menu)
    {
        adjacentMenus[_direction] = _menu;
    }

    public virtual void SetActive(bool _active)
    {
        attachedGameObject.SetActive(_active);
    }

    public virtual void SetSelected(bool _selected)
    {
        return;
    }

    public virtual void SetForeground(bool _foreground)
    {
        attachedGameObject.transform.Find("Text").GetComponent<Text>().material = _foreground ? null : GameManager.instance.GUIManager.UITextDarkenedMaterial;
        Foreground = _foreground;
        return;
    }
}
