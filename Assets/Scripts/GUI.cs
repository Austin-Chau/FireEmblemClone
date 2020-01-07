using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUI : MonoBehaviour
{
    public GameObject textPrefab;
    public GameObject bodyTextPrefab;
    public GameObject menuEntryLabelPrefab;
    public GameObject turnTextObject;
    public GameObject actionTextObject;
    private GameObject commandMenuObject;
    private GameObject turnBannerObject;
    private GameObject mainMenuObject;

    private MenuGroup mainMenuGroup;

    public Material UITextDarkenedMaterial;

    #region Menu Controls
    private Stack<MenuGroup> suspendedMenuGroups = new Stack<MenuGroup>();
    private MenuGroup currentMenuGroup;
    private Menu currentFocusedMenu;

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

        mainMenuGroup = GenerateMainMenu();

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

    public void MoveCursor(AdjacentDirection _direction)
    {
        if (_direction == AdjacentDirection.None || currentFocusedMenu == null) //|| current menu isn't ready
        {
            persistantInputDirection = AdjacentDirection.None;
            menuTimer = menuTimerMax;
            return;
        }

        if (!StepScrollingBuffer(currentFocusedMenu.GetBufferScrolling(_direction), _direction))
            return;

        Menu tempMenu = currentFocusedMenu.GetAdjacentMenu(_direction);
        if (tempMenu != null && tempMenu != currentFocusedMenu)
        {
            currentFocusedMenu.SetSelected(false);
            currentFocusedMenu = tempMenu;
            currentFocusedMenu.SetSelected(true);
        }
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
        currentFocusedMenu.SetSelected(false);
        currentMenuGroup.SetActive(false);
        currentMenuGroup.reverseCallback();

        if (suspendedMenuGroups.Count > 0)
        {
            currentMenuGroup = suspendedMenuGroups.Pop();
            currentFocusedMenu = currentMenuGroup.GetInitialMenu();
            currentMenuGroup.SetForeground(true);
            currentFocusedMenu.SetSelected(true);
        }
        else
        {
            currentMenuGroup = null;
            currentFocusedMenu = null;
        }
    }
    public void ForwardMenu(MenuGroup _newMenuGroup)
    {
        if (currentMenuGroup == _newMenuGroup)
        {
            return;
        }

        if (currentMenuGroup != null)
        {
            currentMenuGroup.SetForeground(false);
            suspendedMenuGroups.Push(currentMenuGroup);
        }
        currentMenuGroup = _newMenuGroup;
        currentFocusedMenu = currentMenuGroup.GetInitialMenu();
        currentMenuGroup.SetActive(true);
        currentFocusedMenu.SetSelected(true);
    }

    public void ActivateCursor()
    {
        if (currentFocusedMenu.SelectEntry())
        {
            currentMenuGroup.SetActive(false);

            if (suspendedMenuGroups.Count > 0)
            {
                currentMenuGroup = suspendedMenuGroups.Pop();
                currentFocusedMenu = currentMenuGroup.GetInitialMenu();
                currentMenuGroup.SetForeground(true);
                currentFocusedMenu.SetSelected(true);
            }
            else
            {
                currentMenuGroup = null;
                currentFocusedMenu = null;
            }
        }
    }

    public void StartMainMenu()
    {
        ForwardMenu(mainMenuGroup);
    }

    public void StartCommandMenu(List<Tuple<string, Action>> _listOfEntries, Action _reverseCallback)
    {
        foreach (Transform child in commandMenuObject.transform)
        {
            Destroy(child.gameObject);
        }

        MenuGroup commandMenuGroup = new MenuGroup(_reverseCallback);

        Menu[] tempArray = new Menu[_listOfEntries.Count];
        int i = 0;

        foreach (Tuple<string, Action> entry in _listOfEntries)
        {
            GameObject tempEntry = Instantiate(menuEntryLabelPrefab, commandMenuObject.transform, false);
            Func<bool> tempFunc = () => { entry.Item2(); return true; };
            Menu tempMenu = new CommandMenuEntryLabel(tempEntry, entry.Item1, tempFunc);
            tempMenu.SetActive(true);
            tempArray[i] = tempMenu;

            if (i > 0)
            {
                tempArray[i - 1].SetAdjacentMenu(AdjacentDirection.Down, tempArray[i]);
                tempArray[i].SetAdjacentMenu(AdjacentDirection.Up, tempArray[i - 1]);
            }
            if (i == _listOfEntries.Count - 1)
            {
                tempArray[i].SetAdjacentMenu(AdjacentDirection.Down, tempArray[0]);
                tempArray[0].SetAdjacentMenu(AdjacentDirection.Up, tempArray[i]);
            }

            commandMenuGroup.Add(tempMenu);
            i++;
        }
        currentMenuGroup = commandMenuGroup;
        currentFocusedMenu = commandMenuGroup.GetInitialMenu();
        commandMenuObject.SetActive(true);
        currentFocusedMenu.SetSelected(true);
    }

    private string[] mainMenuLabels = { "Overview", "Controls" };

    public MenuGroup GenerateMainMenu()
    {
        MenuGroup tempGroup = new MenuGroup(() => { });
        GameObject leftColumn = mainMenuObject.transform.Find("LeftColumn").gameObject;
        GameObject rightColumn = mainMenuObject.transform.Find("RightColumn").gameObject;
        Menu[] tempArray = new Menu[mainMenuLabels.Length];
        int i = 0;

        foreach (string _string in mainMenuLabels)
        {
            GameObject tempEntryObject = Instantiate(menuEntryLabelPrefab, leftColumn.transform, false);
            GameObject tempEntryStagingAreaObject = Instantiate(bodyTextPrefab, rightColumn.transform, false);

            Menu tempMenu = new MainMenuEntryLabel(tempEntryObject, _string, tempEntryStagingAreaObject);
            tempArray[i] = tempMenu;
            Debug.Log(i.ToString() + mainMenuLabels.Length.ToString());
            if (i > 0)
            {
                Debug.Log(i);
                tempArray[i - 1].SetAdjacentMenu(AdjacentDirection.Down, tempArray[i]);
                tempArray[i].SetAdjacentMenu(AdjacentDirection.Up, tempArray[i - 1]);
            }
            if (i == mainMenuLabels.Length - 1)
            {
                Debug.Log(i);
                tempArray[i].SetAdjacentMenu(AdjacentDirection.Down, tempArray[0]);
                tempArray[0].SetAdjacentMenu(AdjacentDirection.Up, tempArray[i]);
            }

            tempGroup.Add(tempMenu);

            i++;
        }
        tempGroup.SetActive(false);
        return tempGroup;
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
