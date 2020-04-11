using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{

    public GameObject GUIManagerObject;
    public GameObject textPrefab;
    public GameObject bodyTextPrefab;
    public GameObject menuElementLabelPrefab;

    //References to gameobjects that have already been placed (their text components will be grabbed)
    public GameObject turnTextObject;
    public GameObject tileTextObject;
    public GameObject actionTextObject;
    public GameObject stateTextObject;
    public GameObject turnBannerTextObject;

    //Other references to gameobjects
    public GameObject commandMenuObject;
    public GameObject mainMenuObject;
    private GameObject unitInspectorObject;

    [SerializeField]
    public GameObject commandMenuElementPrefab;
    [SerializeField]
    public MainMenuElement[] mainMenuElements;

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
    private Text tileText;
    private Text stateText;
    private Text turnBannerText;


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
    private readonly Dictionary<GameStates, string> gameStateNames = new Dictionary<GameStates, string>
    {
        {GameStates.None, "None" },
        {GameStates.GUIMenuing, "GUIMenuing" },
        {GameStates.UnitPathConclusion, "UnitPathConclusion" },
        {GameStates.UnitPathCreation, "UnitPathCreation" }
    };
    private readonly Dictionary<ActionNames, string> actionNames = new Dictionary<ActionNames, string>
    {
        {ActionNames.Move, "Move" },
        {ActionNames.Attack, "Attack" }
    };

    private void Awake()
    {
        actionText = actionTextObject.GetComponent<Text>();
        turnText = turnTextObject.GetComponent<Text>();
        tileText = tileTextObject.GetComponent<Text>();
        stateText = stateTextObject.GetComponent<Text>();
        turnBannerText = turnBannerTextObject.GetComponent<Text>();

        commandMenuObject = transform.Find("CommandMenu").gameObject;

        mainMenuObject = transform.Find("MainMenu").gameObject;

        unitInspectorObject = transform.Find("UnitInspector").gameObject;
        unitInspectorObject.SetActive(false);


        mainMenuContainer = GenerateMainMenu();

        GUIManagerObject = gameObject;
        DontDestroyOnLoad(gameObject);
    }

    public void UpdateSelectedTile(Tile _tile)
    {
        /*
        if (_tile.CurrentUnit != null)
        {
            UpdateSelectedUnit(_tile.CurrentUnit);
        }*/

        tileText.text = "Current tile is " + _tile.GridPosition + " with type " + _tile.type;
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

    public void UpdateHoveredUnit(Unit _unit)
    {
        if (_unit == null)
        {
            unitInspectorObject.SetActive(false);
        }
        else
        {
            unitInspectorObject.SetActive(true);
            unitInspectorObject.transform.Find("Name").GetComponent<Text>().text = _unit.Name;
            unitInspectorObject.transform.Find("Team").GetComponent<Text>().text = teamNames[_unit.Team];
            unitInspectorObject.transform.Find("Health").GetComponent<Text>().text = _unit.CurrentHealth + "/" + _unit.MaxHealth;
            unitInspectorObject.transform.Find("Strength").GetComponent<Text>().text = _unit.Strength.ToString();
            unitInspectorObject.transform.Find("Defence").GetComponent<Text>().text = _unit.Defence.ToString();
            unitInspectorObject.transform.Find("Movement").GetComponent<Text>().text = _unit.Movement.ToString();
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

    public void UpdateGameState(GameStates _state)
    {
        stateText.text = "GameState is: "+gameStateNames[_state];
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

        IMenuElement tempMenu = currentMenuContainer.CurrentMenu.GetAdjacentMenuElement(_direction);
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
        Action reverseCallback = currentMenuContainer.ReverseCallback;

        if (suspendedMenuContainers.Count > 0)
        {
            currentMenuContainer = suspendedMenuContainers.Pop();
            currentMenuContainer.SetForeground(true);
        }
        else
        {
            currentMenuContainer = null;
            GameManager.instance.ChangeGameState(GameStates.None);
        }

        reverseCallback();
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
        if (currentMenuContainer.CurrentMenu.ConfirmElement())
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

    public void StartCommandMenu(List<Tuple<string, Action>> _listOfElements, Action _reverseCallback)
    {
        GameManager.instance.ChangeGameState(GameStates.GUIMenuing);

        foreach (Transform child in commandMenuObject.transform)
        {
            Destroy(child.gameObject);
        }

        MenuContainer commandMenuContainer = new MenuContainer(_reverseCallback);

        IMenuElement[] tempArray = new IMenuElement[_listOfElements.Count];
        int i = 0;

        foreach (Tuple<string, Action> element in _listOfElements)
        {
            //GameObject tempElement = Instantiate(menuElementLabelPrefab, commandMenuObject.transform, false);
            bool tempCallback() { element.Item2(); return true; }

            //MenuElement tempMenu = new CommandMenuElement(tempElement, element.Item1, tempFunc);
            CommandMenuElement tempMenu = Instantiate(commandMenuElementPrefab, commandMenuObject.transform).GetComponent<CommandMenuElement>();
            tempMenu.InitiateProperties(element.Item1,tempCallback);

            tempMenu.SetActive(true);
            tempArray[i] = tempMenu;

            if (i > 0)
            {
                tempArray[i - 1].SetAdjacentMenu(AdjacentDirection.Down, tempArray[i]);
                tempArray[i].SetAdjacentMenu(AdjacentDirection.Up, tempArray[i - 1]);
            }
            if (i == _listOfElements.Count - 1)
            {
                tempArray[i].SetAdjacentMenu(AdjacentDirection.Down, tempArray[0]);
                tempArray[0].SetAdjacentMenu(AdjacentDirection.Up, tempArray[i]);
            }

            commandMenuContainer.Add(tempMenu);
            i++;
        }
        ForwardMenu(commandMenuContainer);

        /*
        currentMenuContainer = commandMenuContainer;
        currentMenuContainer.CurrentMenu = commandMenuContainer.GetInitialMenu();
        commandMenuObject.SetActive(true);
        currentMenuContainer.CurrentMenu.SetSelected(true);
        */
    }

    private readonly string[] mainMenuLabels = { "Overview", "Controls" };

    /// <summary>
    /// Creates a menucontainer that contains the mainmenu elements
    /// </summary>
    /// <returns></returns>
    public MenuContainer GenerateMainMenu()
    {
        MenuContainer tempContainer = new MenuContainer(() => { });

        for(int i = 0; i < mainMenuElements.Length; i++)
        {
            tempContainer.Add(mainMenuElements[i]);
        }

        return tempContainer;
    }

    public void TurnBanner(Commander _commander, Action _callback)
    {
        StartCoroutine(TurnBannerCoroutine("Start of " + teamNames[_commander.Team] + "'s turn.", _callback));
    }

    public void VictoryBanner(Commander _commander, Action _callback)
    {
        StartCoroutine(TurnBannerCoroutine(teamNames[_commander.Team] + " is victorious! Restarting game.", _callback));
    }

    private IEnumerator<WaitForSeconds> TurnBannerCoroutine(string _bannerText, Action _callback)
    {
        turnBannerText.gameObject.SetActive(true);
        turnBannerText.text = _bannerText;
        yield return new WaitForSeconds(turnBannerLength);
        turnBannerText.gameObject.SetActive(false);
        _callback();
    }
}
