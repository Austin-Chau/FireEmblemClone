using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModuloCorrect;

public class MainMenuScript : MonoBehaviour
{
    public GameObject MainMenuEntryPrefab;
    private Tuple<int, int> suspendedCursorPosition = new Tuple<int, int>(0, 0);
    private Dictionary<string,string> menuData = new Dictionary<string,string>()
    {
        { "Overview", "Lorem ipsum dolor sit amet, eleifend vestibulum vestibulum elit pede ut aliquam, id orci integer vel egestas at elit, nisl duis justo ultrices vivamus congue, amet integer sapien sed viverra lacus vivamus. Gravida orci duis purus vitae fusce torquent, mauris ullamcorper amet amet ac condimentum, laoreet netus velit aliquam justo suspendisse nunc, purus praesent ligula nec gravida quisque eget, eu sed maecenas sodales sed mauris. Vestibulum tellus duis sem nam iaculis, fusce feugiat egestas rutrum interdum interdum id, urna urna aliquet in facilisis pellentesque nec, neque sit soluta volutpat justo vitae, ac eros viverra quisque nunc integer eros. Lorem mattis felis lorem vitae vitae aliquam, consectetuer penatibus eget ut nisl suscipit aliquet, donec convallis consequat consequat vitae purus. Maecenas sodales in sociis sem mauris quo, sollicitudin fermentum lacus aliquam luctus laudantium quis, sit curabitur nulla adipiscing duis neque, nulla eu wisi sit vivamus vestibulum. At at eget consectetuer neque vestibulum ante, libero in lacinia interdum volutpat arcu vulputate, sit lectus vestibulum nec sed a. Et porta leo nunc donec in nulla, quis amet consequat quam non et, pede amet fermentum tellus magna mauris in. Sed interdum rutrum sed gravida nec, vel libero pharetra ac dolor sed consequat, nibh imperdiet morbi varius sed et, lorem diam habitant nec curabitur suspendisse, a arcu nullam etiam felis eget diam. Purus est turpis nonummy elit aliquet, commodo pellentesque curabitur pellentesque vel sapien curabitur, bibendum dis commodo phasellus mauris nunc mauris, at fringilla bibendum sit massa gravida, habitant aliquam lectus et nunc est. Interdum justo elementum dolor et sem, ac pellentesque et hac laoreet elit ut, pellentesque feugiat viverra erat volutpat ridiculus, quisque pellentesque dui senectus sit ipsum, erat dolor nisl velit arcu aenean. Porta sollicitudin ipsum erat ac et, nec libero ut et in diam, metus vestibulum praesent donec integer blandit quis, proin eros metus porta in donec. Placerat in neque vitae urna vivamus perferendis, ipsum potenti mauris neque enim ipsum sapien, sem eget nascetur tortor cras at, nostra adipiscing dolor nonummy porttitor pulvinar ultrices." },
        { "Controls", "ControlsBody" }
    };
    public List<GameObject> listOfEntries = new List<GameObject>();
    private GameObject rightColumn;
    private GameObject leftColumn;
    private bool isSetup;
    private MenuBufferingType bufferScrolling = MenuBufferingType.Full;

    public bool IsNotReady()
    {
        return listOfEntries.Count == 0 || !isSetup;
    }

    public MenuBufferingType BufferScrolling(AdjacentDirection _direction)
    {
        if (_direction == AdjacentDirection.Right || _direction == AdjacentDirection.Left)
        {
            return MenuBufferingType.Full;
        }
        return bufferScrolling;
    }

    public void ReverseCallback()
    {
        return;
    }

    public void SetSuspendedCursorPosition(Tuple<int, int> _cursorPosition)
    {
        suspendedCursorPosition = _cursorPosition;
    }
    public Tuple<int, int> GetSuspendedCursorPosition()
    {
        return suspendedCursorPosition;
    }

    private void Start()
    {
        leftColumn = transform.Find("LeftColumn").gameObject;
        rightColumn = transform.Find("RightColumn").gameObject;
        rightColumn.transform.Find("Text").GetComponent<Text>().material = GameManager.instance.GUI.UITextDarkenedMaterial;

        foreach (string tempName in menuData.Keys)
        {
            GameObject newObject = Instantiate(MainMenuEntryPrefab,leftColumn.transform);
            newObject.GetComponent<MainMenuEntryScript>().Initialize(tempName, menuData[tempName], rightColumn);
            newObject.name = tempName;
            listOfEntries.Add(newObject);
        }

        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        isSetup = true;
    }

    public Tuple<int, int> ShiftCursor(Tuple<int, int> _cursorPosition, AdjacentDirection direction)
    {
        int x = _cursorPosition.Item1;
        int y = _cursorPosition.Item2;
        Tuple<int, int> returnValue = _cursorPosition;

        switch (direction)
        {
            case AdjacentDirection.Up:
                y--;
                break;
            case AdjacentDirection.Down:
                y++;
                break;
            case AdjacentDirection.Left:
                x--;
                break;
            case AdjacentDirection.Right:
                x++;
                break;
        }
        y = CustomMath.mod(y,listOfEntries.Count);
        x = CustomMath.mod(x, 2);
        returnValue = new Tuple<int, int>(x, y);
        if (x != _cursorPosition.Item1)
        {
            bufferScrolling = (x == 1) ? MenuBufferingType.Initial : MenuBufferingType.Full;
            rightColumn.transform.Find("Text").GetComponent<Text>().material = (x == 1) ? null : GameManager.instance.GUI.UITextDarkenedMaterial;
        }
        if (y != _cursorPosition.Item2)
        {
            switch (x)
            {
                case 0:
                    SetEntryActive(_cursorPosition, false);
                    SetEntryActive(returnValue, true);
                    break;
                case 1:
                    Vector3 shift = new Vector3(0,0,0);
                    if (direction == AdjacentDirection.Up)
                    {
                        shift = new Vector3(0, 6, 0);
                    }
                    else if (direction == AdjacentDirection.Down)
                    {
                        shift = new Vector3(0, -6, 0);
                    }
                    rightColumn.transform.Find("Text").localPosition = rightColumn.transform.Find("Text").localPosition + shift;
                    break;
            }
        }
        return returnValue;
    }

    public void SetEntryActive(Tuple<int, int> _position, bool _isActive)
    {
        listOfEntries[_position.Item2].GetComponent<MainMenuEntryScript>().Active = _isActive;
    }

    public void SetMenuActive(bool _isActive)
    {
        gameObject.SetActive(_isActive);
        if (_isActive)
        {
            isSetup = false;
            listOfEntries[0].GetComponent<MainMenuEntryScript>().Active = true;
            rightColumn.transform.Find("Text").GetComponent<Text>().material = GameManager.instance.GUI.UITextDarkenedMaterial;
        }
    }

    public void SetMenuForeground(bool _foreground)
    {
        foreach (GameObject tempGameObject in listOfEntries)
        {
            tempGameObject.GetComponent<CommandMenuEntryScript>().Foreground = _foreground;
        }
        rightColumn.transform.Find("Text").GetComponent<Text>().material = GameManager.instance.GUI.UITextDarkenedMaterial;
    }

    public bool SelectEntry(Tuple<int, int> _position)
    {
        return false;
    }
}