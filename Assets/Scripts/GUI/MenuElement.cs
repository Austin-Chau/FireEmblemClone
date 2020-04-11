using System;
public interface IMenuElement
{
    /// <summary>
    /// Performs a callback for when the menu is selected. Returns whether or not the menu group should be closed, so the GUImanager knows immediately.
    /// </summary>
    bool ConfirmElement();

    /// <summary>
    /// Gets what kind of menu scrolling buffering should be performed in the given direction.
    /// </summary>
    MenuBufferingType GetBufferScrolling(AdjacentDirection _direction);

    /// <summary>
    /// Gets the menuelement in the specified directon.
    /// </summary>
    IMenuElement GetAdjacentMenuElement(AdjacentDirection _direction);

    /// <summary>
    /// Sets the menuelement in the given direction.
    /// </summary>
    void SetAdjacentMenu(AdjacentDirection _direction, IMenuElement _menu);

    /// <summary>
    /// Sets whether or not this menuelement is active (the gameobject's visibility)
    /// </summary>
    /// <param name="_active"></param>
    void SetActive(bool _active);

    /// <summary>
    /// Sets whether or not the cursor is hovering over this element.
    /// </summary>
    void SetSelected(bool _selected);

    /// <summary>
    /// Sets whether or not this element is in the currently 'most active' menucontainer (mostly used as visual effect).
    /// </summary>
    void SetForeground(bool _foreground);
}
