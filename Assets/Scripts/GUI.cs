using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface InteractableGUIMenu
{
    Tuple<int, int> ShiftCursor(Tuple<int, int> _cursorPosition, AdjacentDirection _direction);

    bool IsNotReady();
    Tuple<int, int> GetSuspendedCursorPosition();
    void SetSuspendedCursorPosition(Tuple<int, int> _cursorPosition);

    void SetMenuActive(bool _active);
    void SetMenuForeground(bool _foreground);
    void ReverseCallback();

    bool SelectEntry(Tuple<int, int> _position);
    MenuBufferingType BufferScrolling(AdjacentDirection _direction);
}

public interface StaticGUIMenu
{
    void SetMenuActive(bool _active);
}

public class GUI : MonoBehaviour
{
    public GameObject textPrefab;
    public GameObject menuEntryPrefab;
    public GameObject turnTextObject;
    public GameObject actionTextObject;
    private GameObject commandMenuObject;
    private GameObject turnBannerObject;
    private GameObject mainMenuObject;

    public Material UITextDarkenedMaterial;

    #region Menu Controls
    private Stack<InteractableGUIMenu> suspendedMenus = new Stack<InteractableGUIMenu>();
    private InteractableGUIMenu currentFocusedMenu;
    private Tuple<int, int> cursorPosition = new Tuple<int,int>(0,0);

    private AdjacentDirection persistantInputDirection = AdjacentDirection.None;
    private const int menuTimerMax = 30;
    private const int menuTimerDelay = 5;
    private int menuTimer = menuTimerMax;
    #endregion

    private Text actionText;
    private Text turnText;
    private const float turnBannerLength = 1.5f;
    public Unit SelectedUnit
    {
        get
        {
            return selectedUnit;
        }
        set
        {
            UpdateSelectedUnit(value);
            selectedUnit = value;
        }
    }

    public bool InANavigatableMenu()
    {
        return currentFocusedMenu != null;
    }

    private Unit selectedUnit;
    private Dictionary<Team, string> teamNames = new Dictionary<Team, string>
    {
        {Team.Player1, "Player One" },
        {Team.Player2, "Player Two" },
        {Team.Enemy, "Enemy" }
    };
    private Dictionary<ActionNames, string> actionNames = new Dictionary<ActionNames, string>
    {
        {ActionNames.Move, "Move" },
        {ActionNames.Attack, "Attack" }
    };
    private void Awake()
    {
        actionTextObject = transform.Find("ActionText").gameObject;
        actionText = actionTextObject.GetComponent<Text>();
        actionText.alignment = TextAnchor.MiddleLeft;

        turnTextObject = transform.Find("TurnText").gameObject;
        turnText = turnTextObject.GetComponent<Text>();
        turnText.alignment = TextAnchor.MiddleLeft;

        commandMenuObject = transform.Find("CommandMenu").gameObject;
        turnBannerObject = transform.Find("TurnBanner").gameObject;
        turnBannerObject.SetActive(false);
        mainMenuObject = transform.Find("MainMenu").gameObject;

    }
    public void UpdateSelectedUnit(Unit _unit)
    {
        if (_unit == null)
        {
            actionText.text = "No unit is currently selected.";
        }
        else
        {
            actionText.text = "A unit of " + teamNames[_unit.Team] + " is selected";
        }
    }

    public void UpdateCurrentTeam(Commander _commander)
    {
        if (_commander == null)
        {
            turnText.text = "No player's turn";
        }
        else
        {
            turnText.text = teamNames[_commander.Team] + "'s turn";
        }
    }
    public void SwitchMenuFocus(InteractableGUIMenu _newMenu)
    {
        if (currentFocusedMenu == _newMenu)
        {
            return;
        }

        if (currentFocusedMenu != null)
        {
            currentFocusedMenu.SetSuspendedCursorPosition(cursorPosition);
            currentFocusedMenu.SetMenuForeground(false);
            suspendedMenus.Push(currentFocusedMenu);
        }
        currentFocusedMenu = _newMenu;
        cursorPosition = currentFocusedMenu.GetSuspendedCursorPosition();
        currentFocusedMenu.SetMenuActive(true);
    }

    public void MoveCursor(AdjacentDirection _direction)
    {
        if (_direction == AdjacentDirection.None || currentFocusedMenu == null || currentFocusedMenu.IsNotReady())
        {
            persistantInputDirection = AdjacentDirection.None;
            menuTimer = menuTimerMax;
            return;
        }

        if (!StepScrollingBuffer(currentFocusedMenu.BufferScrolling(_direction), _direction))
            return;

        cursorPosition = currentFocusedMenu.ShiftCursor(cursorPosition, _direction);
    }

    private bool StepScrollingBuffer(MenuBufferingType _bufferingType, AdjacentDirection _direction)
    {
        if (_direction == persistantInputDirection || persistantInputDirection == AdjacentDirection.None)
        {
            menuTimer--;

            if (menuTimer <= 0)
            {
                menuTimer = menuTimerDelay;
                return true;
            }
            if (persistantInputDirection == AdjacentDirection.None)
            {
                persistantInputDirection = _direction;
                return true;
            }
            if (_bufferingType == MenuBufferingType.Full)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        persistantInputDirection = AdjacentDirection.None;
        menuTimer = menuTimerMax;
        return false;
    }

    public void ReverseMenu()
    {
        if (currentFocusedMenu != null)
        {
            currentFocusedMenu.ReverseCallback();
            currentFocusedMenu.SetMenuActive(false);
        }

        if (suspendedMenus.Count > 0)
        {
            currentFocusedMenu = suspendedMenus.Pop();
            currentFocusedMenu.SetMenuForeground(true);
            cursorPosition = currentFocusedMenu.GetSuspendedCursorPosition();
        }
        else
        {
            currentFocusedMenu = null;
        }
    }

    public void ActivateCursor()
    {
        if (currentFocusedMenu.SelectEntry(cursorPosition))
        {
            currentFocusedMenu.SetMenuActive(false);
            currentFocusedMenu = null;
        }
    }

    public void StartMainMenu()
    {
        SwitchMenuFocus(mainMenuObject.GetComponent<MainMenuScript>());
    }

    public void StartCommandMenu(List<Tuple<string, Action>> ListOfEntries, Action _reverseCallback)
    {
        foreach (Transform child in commandMenuObject.transform)
        {
            Destroy(child.gameObject);
        }

        List<Tuple<GameObject, Action>> listOfEntries = new List<Tuple<GameObject, Action>>();

        foreach (Tuple<string, Action> entry in ListOfEntries)
        {
            GameObject tempEntry = Instantiate(menuEntryPrefab, new Vector3(0, 0, 0), Quaternion.identity, commandMenuObject.transform);
            GameObject tempEntryText = tempEntry.transform.Find("Text").gameObject;
            tempEntryText.GetComponent<Text>().text = entry.Item1;
            listOfEntries.Add(new Tuple<GameObject,Action>(tempEntry,entry.Item2));
        }
        cursorPosition = new Tuple<int, int>(0, 0);
        currentFocusedMenu = commandMenuObject.GetComponent<CommandMenuScript>();
        commandMenuObject.GetComponent<CommandMenuScript>().Initialize(listOfEntries, _reverseCallback);
        currentFocusedMenu.SetMenuActive(true);
    }

    public void TurnBanner(Commander _commander, Action _callback)
    {
        StartCoroutine(TurnBannerCoroutine("Start of " + teamNames[_commander.Team] + "'s turn.", _callback));
    }

    private IEnumerator<WaitForSeconds> TurnBannerCoroutine(string _bannerText, Action _callback)
    {
        turnBannerObject.SetActive(true);
        turnBannerObject.GetComponent<Text>().text = _bannerText;
        yield return new WaitForSeconds(turnBannerLength);
        turnBannerObject.SetActive(false);
        _callback();
    }
}
