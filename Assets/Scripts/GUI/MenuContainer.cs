using System;
using System.Collections.Generic;

public class MenuContainer
{
    private List<MenuElement> menus = new List<MenuElement>();
    public Action ReverseCallback { get; private set; }
    public MenuElement CurrentMenu;

    public MenuContainer(Action _reverseCallback)
    {
        ReverseCallback = _reverseCallback;
    }

    public void Add(MenuElement _menu)
    {
        menus.Add(_menu);
    }

    public MenuElement GetInitialMenu()
    {
        return menus[0];
    }

    public void SetActive(bool _active)
    {
        foreach (MenuElement menu in menus)
        {
            menu.SetActive(_active);
        }
    }

    public void SetForeground(bool _foreground)
    {
        foreach (MenuElement menu in menus)
        {
            menu.SetForeground(_foreground);
        }
    }
}
