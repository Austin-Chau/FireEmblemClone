using System;
using System.Collections.Generic;
using UnityEngine;

public class CommandMenuScript : MonoBehaviour
{
    [SerializeField] private int commandIndex;
    [SerializeField] private bool menuNotYetMoved = true;
    [SerializeField] private List<Tuple<GameObject, Action>> listOfEntries;
    [SerializeField] private const int firstDelayTimerMax = 30;
    [SerializeField] private const int delayTimerMax = 5;
    [SerializeField] private int firstDelayTimer = firstDelayTimerMax;
    [SerializeField] private int delayTimer = delayTimerMax;

    private void Awake()
    {
        gameObject.SetActive(false);
    }
    private void Update()
    {
        float y = Input.GetAxisRaw("Vertical") * Time.deltaTime;
        if (Mathf.Abs(y) > Mathf.Epsilon && listOfEntries.Count > 0)
        {
            listOfEntries[commandIndex].Item1.transform.parent.GetComponent<CommandMenuEntryScript>().Active = false;
            if (firstDelayTimer > 0)
            {
                firstDelayTimer--;
            }
            if (delayTimer > 0)
            {
                delayTimer--;
            }
            if (delayTimer <= 0 && firstDelayTimer <= 0 || menuNotYetMoved)
            {
                menuNotYetMoved = false;
                delayTimer = delayTimerMax;
                ShiftCursor(y > 0 ? CursorDirections.Up : CursorDirections.Down);
            }
            listOfEntries[commandIndex].Item1.transform.parent.GetComponent<CommandMenuEntryScript>().Active = true;
        }
        else
        {
            menuNotYetMoved = true;
            firstDelayTimer = firstDelayTimerMax;
            delayTimer = 0;
        }
        if (Input.GetButtonDown("confirm"))
        {
            listOfEntries[commandIndex].Item2();
            gameObject.SetActive(false);
        }
    }
    private void ShiftCursor(CursorDirections direction)
    {
        Debug.Log("shifting" + direction);
        switch (direction)
        {
            case CursorDirections.Up:
                commandIndex--;
                break;
            case CursorDirections.Down:
                commandIndex++;
                break;
            case CursorDirections.Left:
                break;
            case CursorDirections.Right:
                break;
        }

        if (commandIndex < 0)
        {
            commandIndex = listOfEntries.Count - 1;
        }
        else if (commandIndex >= listOfEntries.Count)
        {
            commandIndex = 0;
        }
    }

    private void OnEnable()
    {
        commandIndex = 0;
        listOfEntries[commandIndex].Item1.transform.Translate(new Vector3(-30, 0, 0));
    }

    public void SetEntries(List<Tuple<GameObject, Action>> _listOfEntries)
    {
        listOfEntries = _listOfEntries;
    }
}

public enum CursorDirections
{
    None,
    Up,
    Down,
    Left,
    Right
}