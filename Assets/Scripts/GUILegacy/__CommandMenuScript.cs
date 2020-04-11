using System;
using System.Collections.Generic;
using UnityEngine;

public class CommandMenuScript : MonoBehaviour
{
    [SerializeField] private List<Tuple<GameObject, Action>> listOfElements;
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
        return listOfElements.Count == 0 || !isSetup;
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
            y = listOfElements.Count - 1;
        }
        else if (y >= listOfElements.Count)
        {
            y = 0;
        }

        if (y != _cursorPosition.Item2)
        {
            SetElementActive(_cursorPosition, false);
            Tuple<int, int> newTuple = new Tuple<int, int>(0, y);
            SetElementActive(newTuple, true);
            return newTuple;
        }
        return _cursorPosition;
    }

    public void SetElementActive(Tuple<int,int> _position, bool _isActive)
    {
        listOfElements[_position.Item2].Item1.GetComponent<CommandMenuElementScript>().Active = _isActive;
    }

    public void SetMenuActive(bool _active)
    {
        gameObject.SetActive(_active);
        if (_active)
        {
            isSetup = false;
            Debug.Log(listOfElements[0].Item1);
            listOfElements[0].Item1.GetComponent<CommandMenuElementScript>().Active = true;
        }
    }

    public void SetMenuForeground(bool _foreground)
    {
        foreach (Tuple<GameObject,Action> tuple in listOfElements)
        {
            tuple.Item1.GetComponent<CommandMenuElementScript>().Foreground = _foreground;
        }
    }

    public void Initialize(List<Tuple<GameObject, Action>> _listOfElements, Action _reverseCallback)
    {
        listOfElements = _listOfElements;
        reverseCallback = _reverseCallback;
    }

    public bool SelectElement(Tuple<int,int> _position)
    {
        listOfElements[_position.Item2].Item2();
        return true;
    }
}