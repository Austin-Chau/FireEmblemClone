using System;
using System.Collections.Generic;

public class MenuContainer
{
    private List<IMenuElement> menus = new List<IMenuElement>();
    public Action ReverseCallback { get; private set; }
    public IMenuElement CurrentMenu;

    public MenuContainer(Action _reverseCallback)
    {
        ReverseCallback = _reverseCallback;
    }
    public MenuContainer(Action _reverseCallback, List<IMenuElement> _menus)
    {
        ReverseCallback = _reverseCallback;
        foreach(IMenuElement menu in _menus)
        {
            menus.Add(menu);
        }
    }

    public void Add(IMenuElement _menu)
    {
        menus.Add(_menu);
    }

    public IMenuElement GetInitialMenu()
    {
        return menus[0];
    }

    public void SetActive(bool _active)
    {
        foreach (IMenuElement menu in menus)
        {
            menu.SetActive(_active);
        }
    }

    public void SetForeground(bool _foreground)
    {
        foreach (IMenuElement menu in menus)
        {
            menu.SetForeground(_foreground);
        }
    }
}
