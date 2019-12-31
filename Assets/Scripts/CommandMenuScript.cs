using System;
using System.Collections.Generic;
using UnityEngine;

public class CommandMenuScript : MonoBehaviour, InteractableGUIMenu
{
    [SerializeField] private List<Tuple<GameObject, Action>> listOfEntries;
    private Tuple<int, int> suspendedCursorPosition = new Tuple<int,int>(0,0);
    private Action reverseCallback;

    private bool isSetup;

    public void ReverseCallback()
    {
        reverseCallback?.Invoke();
    }
    public void SetSuspendedCursorPosition(Tuple<int, int> _cursorPosition)
    {
        Debug.Log(_cursorPosition);
        suspendedCursorPosition = _cursorPosition;
    }
    public Tuple<int, int> GetSuspendedCursorPosition()
    {
        Debug.Log(suspendedCursorPosition);
        return suspendedCursorPosition;
    }

    public bool IsNotReady()
    {
        return listOfEntries.Count == 0 || !isSetup;
    }

    public MenuBufferingType BufferScrolling(AdjacentDirection _direction)
    {
        return MenuBufferingType.Full;
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        isSetup = true;
    }

    public Tuple<int,int> ShiftCursor(Tuple<int,int> _cursorPosition, AdjacentDirection direction)
    {
        int y = _cursorPosition.Item2;

        switch (direction)
        {
            case AdjacentDirection.Up:
                y--;
                break;
            case AdjacentDirection.Down:
                y++;
                break;
            case AdjacentDirection.Left:
                return _cursorPosition;
            case AdjacentDirection.Right:
                return _cursorPosition;
        }

        if (y < 0)
        {
            y = listOfEntries.Count - 1;
        }
        else if (y >= listOfEntries.Count)
        {
            y = 0;
        }

        if (y != _cursorPosition.Item2)
        {
            SetEntryActive(_cursorPosition, false);
            Tuple<int, int> newTuple = new Tuple<int, int>(0, y);
            SetEntryActive(newTuple, true);
            return newTuple;
        }
        return _cursorPosition;
    }

    public void SetEntryActive(Tuple<int,int> _position, bool _isActive)
    {
        listOfEntries[_position.Item2].Item1.GetComponent<CommandMenuEntryScript>().Active = _isActive;
    }

    public void SetMenuActive(bool _active)
    {
        gameObject.SetActive(_active);
        if (_active)
        {
            isSetup = false;
            Debug.Log(listOfEntries[0].Item1);
            listOfEntries[0].Item1.GetComponent<CommandMenuEntryScript>().Active = true;
        }
    }

    public void SetMenuForeground(bool _foreground)
    {
        foreach (Tuple<GameObject,Action> tuple in listOfEntries)
        {
            tuple.Item1.GetComponent<CommandMenuEntryScript>().Foreground = _foreground;
        }
    }

    public void Initialize(List<Tuple<GameObject, Action>> _listOfEntries, Action _reverseCallback)
    {
        listOfEntries = _listOfEntries;
        reverseCallback = _reverseCallback;
    }

    public bool SelectEntry(Tuple<int,int> _position)
    {
        listOfEntries[_position.Item2].Item2();
        return true;
    }
}