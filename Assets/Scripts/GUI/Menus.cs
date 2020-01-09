using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuElement
{
    protected GameObject attachedGameObject;
    protected Func<bool> confirmEntryCallback;

    protected Dictionary<AdjacentDirection, MenuBufferingType> bufferingDict = new Dictionary<AdjacentDirection, MenuBufferingType>();

    public bool Active { get; protected set; }
    public bool Selected { get; protected set; }
    public bool Foreground { get; protected set; }

    private Dictionary<AdjacentDirection, MenuElement> adjacentMenus = new Dictionary<AdjacentDirection, MenuElement>();

    public MenuElement(GameObject _attachedGameObject)
    {
        attachedGameObject = _attachedGameObject;
    }

    /// <summary>
    /// Performs a possible callback for when this menu element is selected.
    /// </summary>
    /// <returns>Returns whether or not this action implies the menu should be closed.</returns>
    public bool ConfirmEntry()
    {
        return (bool)confirmEntryCallback?.Invoke();
    }

    /// <summary>
    /// Gets what kind of menu scrolling buffering should be performed in the given direction.
    /// </summary>
    public MenuBufferingType GetBufferScrolling(AdjacentDirection _direction)
    {
        if (!bufferingDict.ContainsKey(_direction))
        {
            return MenuBufferingType.Full;
        }
        return bufferingDict[_direction];
    }

    /// <summary>
    /// Gets the menuelement in the specified directon.
    /// </summary>
    public MenuElement GetAdjacentMenuElement(AdjacentDirection _direction)
    {
        if (!adjacentMenus.ContainsKey(_direction))
        {
            return this;
        }
        return adjacentMenus[_direction];
    }

    /// <summary>
    /// Sets the menuelement in the given direction.
    /// </summary>
    public void SetAdjacentMenu(AdjacentDirection _direction, MenuElement _menu)
    {
        adjacentMenus[_direction] = _menu;
    }

    /// <summary>
    /// Sets whether or not this menuelement is active (the gameobject's visibility)
    /// </summary>
    /// <param name="_active"></param>
    public virtual void SetActive(bool _active)
    {
        attachedGameObject.SetActive(_active);
    }

    /// <summary>
    /// Sets whether or not the cursor is hovering over this element.
    /// </summary>
    public virtual void SetSelected(bool _selected)
    {
        return;
    }

    /// <summary>
    /// Sets whether or not this element is in the currently 'most active' menucontainer.
    /// </summary>
    public virtual void SetForeground(bool _foreground)
    {
        attachedGameObject.transform.Find("Text").GetComponent<Text>().material = _foreground ? null : GameManager.instance.GUIManager.UITextDarkenedMaterial;
        Foreground = _foreground;
        return;
    }
}
