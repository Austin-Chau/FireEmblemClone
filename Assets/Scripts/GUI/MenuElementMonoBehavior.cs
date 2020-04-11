using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuElementMonoBehavior : MonoBehaviour, IMenuElement
{
    public Func<bool> confirmElementCallback;

    protected Dictionary<AdjacentDirection, MenuBufferingType> bufferingDict = new Dictionary<AdjacentDirection, MenuBufferingType>();

    //Adjacent menus (4 elements, up right down left) to be set in the unity inspector
    public MainMenuElement[] predefinedAdjacentMenus;
    //A bool to be set in the inspector to indicate this element needs special initilizing
    public bool needsInitialization;

    public bool Active { get; protected set; }
    public bool Selected { get; protected set; }
    public bool Foreground { get; protected set; }

    private readonly AdjacentDirection[] adjacentDirections = { AdjacentDirection.Up, AdjacentDirection.Right, AdjacentDirection.Down, AdjacentDirection.Left };
    private Dictionary<AdjacentDirection, IMenuElement> adjacentMenus = new Dictionary<AdjacentDirection, IMenuElement>();

    public void Start()
    {
        if (!needsInitialization)
        {
            return;
        }
        Debug.Log(gameObject.name);
        Debug.Log(predefinedAdjacentMenus.Length);
        for (int i = 0; i < 4; i++)
        {
            adjacentMenus[adjacentDirections[i]] = predefinedAdjacentMenus[i];
        }
    }

    public bool ConfirmElement()
    {
        bool? value = confirmElementCallback?.Invoke();
        return value ?? false;
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
        gameObject.SetActive(_active);
    }

    public virtual void SetSelected(bool _selected)
    {
        return;
    }

    public virtual void SetForeground(bool _foreground)
    {
        gameObject.transform.Find("Text").GetComponent<Text>().material = _foreground ? null : GameManager.instance.GUIManager.UITextDarkenedMaterial;
        Foreground = _foreground;
        return;
    }
}
