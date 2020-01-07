using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu
{
    protected GameObject attachedGameObject;
    protected Func<bool> selectEntryCallback;

    protected Dictionary<AdjacentDirection, MenuBufferingType> bufferingDict = new Dictionary<AdjacentDirection, MenuBufferingType>();

    public bool Active { get; protected set; }
    public bool Selected { get; protected set; }
    public bool Foreground { get; protected set; }

    private Dictionary<AdjacentDirection, Menu> adjacentMenus = new Dictionary<AdjacentDirection, Menu>();

    public Menu(GameObject _attachedGameObject)
    {
        attachedGameObject = _attachedGameObject;
    }

    public bool SelectEntry()
    {
        Debug.Log("menu selected");
        return (bool)selectEntryCallback?.Invoke();
    }

    public MenuBufferingType GetBufferScrolling(AdjacentDirection _direction)
    {
        if (!bufferingDict.ContainsKey(_direction))
        {
            return MenuBufferingType.Full;
        }
        return bufferingDict[_direction];
    }

    public Menu GetAdjacentMenu(AdjacentDirection _direction)
    {
        if (!adjacentMenus.ContainsKey(_direction))
        {
            return this;
        }
        return adjacentMenus[_direction];
    }

    public void SetAdjacentMenu(AdjacentDirection _direction, Menu _menu)
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
        attachedGameObject.transform.Find("Text").GetComponent<Text>().material = _foreground ? null : GameManager.instance.GUI.UITextDarkenedMaterial;
        Foreground = _foreground;
        return;
    }
}

public class CommandMenuEntryLabel : Menu
{
    public CommandMenuEntryLabel(GameObject _attachedGameObject, string _labelText, Func<bool> _callback) : base(_attachedGameObject)
    {
        attachedGameObject.transform.Find("Text").GetComponent<Text>().text = _labelText;
        selectEntryCallback = _callback;
        return;
    }

    public override void SetSelected(bool _selected)
    {
        attachedGameObject.transform.Find("Text").localPosition = new Vector3(_selected ? -30 : 0, 0, 0);
        Selected = _selected;
    }
}

public class MainMenuEntryLabel : Menu
{
    private GameObject stagingAreaObject;

    public MainMenuEntryLabel(GameObject _attachedGameObject, string _labelText, GameObject _stagingAreaObject) : base(_attachedGameObject)
    {
        attachedGameObject.transform.Find("Text").GetComponent<Text>().text = _labelText;
        stagingAreaObject = _stagingAreaObject;
        //temp:
        stagingAreaObject.GetComponent<Text>().text = _labelText + " Body";
    }

    public override void SetSelected(bool _selected)
    {
        attachedGameObject.transform.Find("Text").localPosition = new Vector3(_selected ? -30 : 0, 0, 0);
        stagingAreaObject.SetActive(_selected);
        Selected = _selected;
    }

    public override void SetActive(bool _active)
    {
        attachedGameObject.SetActive(_active);

        if (!_active)
            stagingAreaObject.SetActive(false);
    }
}

public class MenuGroup
{
    private List<Menu> menus = new List<Menu>();
    public Action reverseCallback { get; private set; }

    public MenuGroup(Action _reverseCallback)
    {
        reverseCallback = _reverseCallback;
    }

    public void Add(Menu _menu)
    {
        menus.Add(_menu);
    }

    public Menu GetInitialMenu()
    {
        return menus[0];
    }

    public void SetActive(bool _active)
    {
        foreach(Menu menu in menus)
        {
            menu.SetActive(_active);
        }
    }

    public void SetForeground(bool _foreground)
    {
        foreach(Menu menu in menus)
        {
            menu.SetForeground(_foreground);
        }
    }
}