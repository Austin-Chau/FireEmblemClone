using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{
    public GameObject textPrefab;
    public GameObject bodyTextPrefab;
    public GameObject menuEntryLabelPrefab;
    public GameObject turnTextObject;
    public GameObject actionTextObject;
    private GameObject commandMenuObject;
    private GameObject turnBannerObject;
    private GameObject mainMenuObject;

    private MenuContainer mainMenuContainer;

    public Material UITextDarkenedMaterial;

    #region Menu Controls
    private readonly Stack<MenuContainer> suspendedMenuContainers = new Stack<MenuContainer>();
    private MenuContainer currentMenuContainer;

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

    /// <summary>
    /// Bool of if GUIManager has a menu in focus.
    /// </summary>
    public bool InANavigatableMenu()
    {
        return (currentMenuContainer != null && currentMenuContainer.CurrentMenu != null);
    }

    private Unit selectedUnit;
    private readonly Dictionary<Team, string> teamNames = new Dictionary<Team, string>
    {
        {Team.Player1, "Player One" },
        {Team.Player2, "Player Two" },
        {Team.Enemy, "Enemy" }
    };
    private readonly Dictionary<ActionNames, string> actionNames = new Dictionary<ActionNames, string>
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

        mainMenuContainer = GenerateMainMenu();

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

    /// <summary>
    /// Controls the movement from one menuelement to the next.
    /// </summary>
    public void MoveCursor(AdjacentDirection _direction)
    {
        if (_direction == AdjacentDirection.None || currentMenuContainer.CurrentMenu == null) //|| current menu isn't ready
        {
            persistantInputDirection = AdjacentDirection.None;
            menuTimer = menuTimerMax;
            return;
        }

        if (!StepScrollingBuffer(currentMenuContainer.CurrentMenu.GetBufferScrolling(_direction), _direction))
            return;

        MenuElement tempMenu = currentMenuContainer.CurrentMenu.GetAdjacentMenuElement(_direction);
        if (tempMenu != null && tempMenu != currentMenuContainer.CurrentMenu)
        {
            currentMenuContainer.CurrentMenu.SetSelected(false);
            currentMenuContainer.CurrentMenu = tempMenu;
            currentMenuContainer.CurrentMenu.SetSelected(true);
        }
    }

    /// <summary>
    /// Handles the timer and returns a bool whether or not the current input should be carried through or not,
    /// based on if it's too soon after the previous one.
    /// </summary>
    /// <param name="_bufferingType">What style of buffering should be performed.</param>
    /// <param name="_direction">Which direction is the movement going to occur in.</param>
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

    /// <summary>
    /// Goes back a MenuContainer in the stack.
    /// </summary>
    public void ReverseMenuContainer()
    {
        currentMenuContainer.CurrentMenu.SetSelected(false);
        currentMenuContainer.SetActive(false);
        currentMenuContainer.ReverseCallback();

        if (suspendedMenuContainers.Count > 0)
        {
            currentMenuContainer = suspendedMenuContainers.Pop();
            currentMenuContainer.SetForeground(true);
        }
        else
        {
            currentMenuContainer = null;
        }
    }

    /// <summary>
    /// Moves forward to a new MenuContainer, pushing the old one to the stack.
    /// </summary>
    /// <param name="_newMenuContainer">The new container that should be focused on.</param>
    public void ForwardMenu(MenuContainer _newMenuContainer)
    {
        if (currentMenuContainer == _newMenuContainer)
        {
            return;
        }

        if (currentMenuContainer != null)
        {
            currentMenuContainer.SetForeground(false);
            suspendedMenuContainers.Push(currentMenuContainer);
        }
        currentMenuContainer = _newMenuContainer;
        currentMenuContainer.CurrentMenu = currentMenuContainer.GetInitialMenu();
        currentMenuContainer.SetActive(true);
        currentMenuContainer.CurrentMenu.SetSelected(true);
    }

    public void ActivateCursor()
    {
        if (currentMenuContainer.CurrentMenu.ConfirmEntry())
        {
            currentMenuContainer.SetActive(false);

            if (suspendedMenuContainers.Count > 0)
            {
                currentMenuContainer = suspendedMenuContainers.Pop();
                currentMenuContainer.CurrentMenu = currentMenuContainer.GetInitialMenu();
                currentMenuContainer.SetForeground(true);
                currentMenuContainer.CurrentMenu.SetSelected(true);
            }
            else
            {
                currentMenuContainer = null;
            }
        }
    }

    public void StartMainMenu()
    {
        ForwardMenu(mainMenuContainer);
    }

    public void StartCommandMenu(List<Tuple<string, Action>> _listOfEntries, Action _reverseCallback)
    {
        foreach (Transform child in commandMenuObject.transform)
        {
            Destroy(child.gameObject);
        }

        MenuContainer commandMenuGroup = new MenuContainer(_reverseCallback);

        MenuElement[] tempArray = new MenuElement[_listOfEntries.Count];
        int i = 0;

        foreach (Tuple<string, Action> entry in _listOfEntries)
        {
            GameObject tempEntry = Instantiate(menuEntryLabelPrefab, commandMenuObject.transform, false);
            bool tempFunc() { entry.Item2(); return true; }
            MenuElement tempMenu = new CommandMenuElement(tempEntry, entry.Item1, tempFunc);
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
        currentMenuContainer = commandMenuGroup;
        currentMenuContainer.CurrentMenu = commandMenuGroup.GetInitialMenu();
        commandMenuObject.SetActive(true);
        currentMenuContainer.CurrentMenu.SetSelected(true);
    }

    private readonly string[] mainMenuLabels = { "Overview", "Controls" };

    public MenuContainer GenerateMainMenu()
    {
        MenuContainer tempGroup = new MenuContainer(() => { });
        GameObject leftColumn = mainMenuObject.transform.Find("LeftColumn").gameObject;
        GameObject rightColumn = mainMenuObject.transform.Find("RightColumn").gameObject;
        MenuElement[] tempArray = new MenuElement[mainMenuLabels.Length];
        int i = 0;

        foreach (string _string in mainMenuLabels)
        {
            GameObject tempEntryObject = Instantiate(menuEntryLabelPrefab, leftColumn.transform, false);
            GameObject tempEntryStagingAreaObject = Instantiate(bodyTextPrefab, rightColumn.transform, false);

            MenuElement tempMenu = new MainMenuElement(tempEntryObject, _string, tempEntryStagingAreaObject);
            tempArray[i] = tempMenu;

            if (i > 0)
            {
                tempArray[i - 1].SetAdjacentMenu(AdjacentDirection.Down, tempArray[i]);
                tempArray[i].SetAdjacentMenu(AdjacentDirection.Up, tempArray[i - 1]);
            }
            if (i == mainMenuLabels.Length - 1)
            {
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
