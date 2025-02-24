using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectionScreenScript : MonoBehaviour
{
    public static SelectionScreenScript instance;

    private void Awake() {
        if (instance == null) {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }

        // Two Player Warning Messages
        twoPlayerWarningMessageAnimator = twoPlayerWarningMessage.GetComponent<Animator>();
        twoPlayerWarningMessageCanvasGroup = twoPlayerWarningMessage.GetComponent<CanvasGroup>();

        twoPlayerOnlyMouseSelectedMessageAnimator = twoPlayerOnlyMouseSelectedMessage.GetComponent<Animator>();
        twoPlayerOnlyMouseSelectedMessageCanvasGroup = twoPlayerOnlyMouseSelectedMessage.GetComponent<CanvasGroup>();

        // One Player Warning Messages
        onePlayerWarningMessageAnimator = onePlayerWarningMessage.GetComponent<Animator>();
        onePlayerWarningMessageCanvasGroup = onePlayerWarningMessage.GetComponent<CanvasGroup>();

        onePlayerOnlyMouseWarningMessageAnimator = onePlayerOnlyMouseWarningMessage.GetComponent<Animator>();
        onePlayerOnlyMouseWarningMessageCanvasGroup = onePlayerOnlyMouseWarningMessage.GetComponent<CanvasGroup>();

        // Map Screen Warning Messages
        mapWarningMessageAnimator = mapWarningMessage.GetComponent<Animator>();
        mapWarningMessageCanvasGroup = mapWarningMessage.GetComponent<CanvasGroup>();
    }

    /*
     * Player Setup Screen
     */
    public PlayerSetup playerSetup = PlayerSetup.playerVsAI;

    public void SetPlayerSetup(string newPlayerSetupString) {
        if (ConstantPlayerValues.playerSetups.TryGetValue(newPlayerSetupString, out PlayerSetup newPlayerSetup)) {
            playerSetup = newPlayerSetup;
        }
        else Debug.Log("PlayerSetup button set incorrectly. Tried to parse: " + newPlayerSetupString);
    }

    /*
     * First Back Button (After the Main Menu)
     */
    public void BackToMainMenu() {
        SceneManager.LoadScene("Main Menu");
    }

    /*
     * Character 1 Selection Screen
     */
    public Characters playerOneCharacter;

    public void SelectCharacterOne(string newCharacterName) {
        if (ConstantPlayerValues.characterTypes.TryGetValue(newCharacterName, out Characters newCharacter)) {
            playerOneCharacter = newCharacter;
        }
        else Debug.Log("Character One selection button set incorrectly. Tried to parse: " + newCharacterName);
    }

    /*
     * Character 2 Selection Screen
     */
    public Characters playerTwoCharacter;

    public void SelectCharacterTwo(string newCharacterName) {
        if (ConstantPlayerValues.characterTypes.TryGetValue(newCharacterName, out Characters newCharacter)) {
            playerTwoCharacter = newCharacter;
        }
        else Debug.Log("Character Two selection button set incorrectly. Tried to parse: " + newCharacterName);
    }

    /*
     * Two Player Control Selection Screen
     */
    public MultiplayerControl playerOneControls;
    public MultiplayerControl playerTwoControls;
    public WhichPlayer playerWhoHasMouseSelected = WhichPlayer.none;

    public void SelectPlayerOneControls(string newControlsString) {
        if (newControlsString == "mouse") {
            if (playerOneMouseButtonScript.Toggled)
                playerWhoHasMouseSelected = WhichPlayer.playerOne;
            else
                playerWhoHasMouseSelected = WhichPlayer.none;
        }
        else if (ConstantPlayerValues.multiplayerControls.TryGetValue(newControlsString, out MultiplayerControl newPlayerOneControls)) {
            playerOneControls = newPlayerOneControls;
        }
        else Debug.Log("Player One Control selection button set incorrectly. Tried to parse: " + newControlsString);
    }

    public void SelectPlayerTwoControls(string newControlsString) {
        if (newControlsString == "mouse") {
            if (playerTwoMouseButtonScript.Toggled)
                playerWhoHasMouseSelected = WhichPlayer.playerTwo;
            else
                playerWhoHasMouseSelected = WhichPlayer.none;
        }
        else if (ConstantPlayerValues.multiplayerControls.TryGetValue(newControlsString, out MultiplayerControl newPlayerTwoControls)) {
            playerTwoControls = newPlayerTwoControls;
        }
        else Debug.Log("Player Two Control selection button set incorrectly. Tried to parse: " + newControlsString);
    }

    // Lists which store the controll buttons which allow the characters to move and attack
    [SerializeField] private List<SelectableButtons> playerOneControlButtons;
    [SerializeField] private List<SelectableButtons> playerTwoControlButtons;
    // References to the current screen and to the next screen so that the back button on the next screen works
    [SerializeField] private GameObject twoPlayerControlSelectionScreen;
    [SerializeField] private GameObject mapSelectionScreen;
    // The "Use Mouse for Attack" button scripts (which contain the information as to whether or not they are set to true)
    [SerializeField] private SelectableButtons playerOneMouseButtonScript;
    [SerializeField] private SelectableButtons playerTwoMouseButtonScript;
    // No controls selected for one or more players warning message
    [SerializeField] private GameObject twoPlayerWarningMessage;
    private CanvasGroup twoPlayerWarningMessageCanvasGroup;
    private Animator twoPlayerWarningMessageAnimator;
    // Controls selected for one player but the other player has only selected the mouse warning message
    [SerializeField] private GameObject twoPlayerOnlyMouseSelectedMessage;
    private CanvasGroup twoPlayerOnlyMouseSelectedMessageCanvasGroup;
    private Animator twoPlayerOnlyMouseSelectedMessageAnimator;

    public void ConfirmTwoPlayerControlSelection() {
        // Check how many controls player one has selected (this should never go above 1)
        int playerOneControlsSelected = 0;
        foreach (SelectableButtons button in playerOneControlButtons) {
            if (button.Toggled) {
                playerOneControlsSelected++;
            }
        }
        if (playerOneControlsSelected > 1) Debug.LogWarning("More than 1 control selected for player one. Controls being applied: " + playerOneControls.ToString());

        // Check how many controls player two has selected (this should never go above 1)
        int playerTwoControlsSelected = 0;
        foreach (SelectableButtons button in playerTwoControlButtons) {
            if (button.Toggled) {
                playerTwoControlsSelected++;
            }
        }
        if (playerTwoControlsSelected > 1) Debug.LogWarning("More than 1 control selected for player two. Controls being applied: " + playerTwoControls.ToString());

        // Check that each player has selected 1, and no more or less than 1, control
        if (playerOneControlsSelected == 1 && playerTwoControlsSelected == 1) {
            // Make the back button on the map screen send the player back to this screen
            screenToGoBackTo = twoPlayerControlSelectionScreen;
            // Swap to the map screen
            mapSelectionScreen.SetActive(true);
            twoPlayerControlSelectionScreen.SetActive(false);

            // Hide the warning message in case it was visible when the confirmation button was pressed
            twoPlayerWarningMessageCanvasGroup.alpha = 0;
            twoPlayerOnlyMouseSelectedMessageCanvasGroup.alpha = 0;
        }
        // If one of the players has selected a control and the other has only selected mouse for attacks (since the one who has selected mouse for attacks will be unable to move)
        else if (playerOneControlsSelected == 1 && playerTwoMouseButtonScript.Toggled || playerTwoControlsSelected == 1 && playerOneMouseButtonScript.Toggled) {
            // Stop any in progress warning messages so that they don't make the message disappear before it should if the button is pressed multiple times in quick succession
            StopCoroutine(ShowWarningMessage(twoPlayerOnlyMouseSelectedMessageAnimator));
            // Show an error message stating that controls have not been selected for each player and then hide it a second later
            StartCoroutine(ShowWarningMessage(twoPlayerOnlyMouseSelectedMessageAnimator));
        }
        else {
            // Stop any in progress warning messages so that they don't make the message disappear before it should if the button is pressed multiple times in quick succession
            StopCoroutine(ShowWarningMessage(twoPlayerWarningMessageAnimator));
            // Show an error message stating that controls have not been selected for each player and then hide it a second later
            StartCoroutine(ShowWarningMessage(twoPlayerWarningMessageAnimator));
        }
    }

    /*
     * One Player Character Selection Screen
     */
    public Characters onePlayerCharacter;

    public void SelectOnePlayerCharacter(string newCharacterName) {
        if (ConstantPlayerValues.characterTypes.TryGetValue(newCharacterName, out Characters newCharacter)) {
            onePlayerCharacter = newCharacter;
        }
        else Debug.Log("Character One selection button set incorrectly. Tried to parse: " + newCharacterName);
    }

    /*
     * AI Difficulty Selection Screen
     */
    public AiDifficulties aIDifficulty;

    private Dictionary<string, AiDifficulties> stringToDifficulty = new() {
        { "Easy", AiDifficulties.Easy},
        { "Medium", AiDifficulties.Medium},
        { "Hard", AiDifficulties.Hard },
        { "GodLike", AiDifficulties.GodLike}
    };
    
    public void SelectAiDifficulty(string newAiDifficulty) {
        stringToDifficulty.TryGetValue(newAiDifficulty, out aIDifficulty);
    }

    /*
     * One Player Control Selection Screen
     */
    public bool wasd = true;
    public void ToggleWASDControl() {
        wasd = !wasd;
    }

    public bool arrowKeys = true;
    public void ToggleArrowKeysControl() {
        arrowKeys = !arrowKeys;
    }

    public bool mouse = true;
    public void ToggleMouseControl() {
        mouse = !mouse;
    }

    public bool controller = true;

    public void ToggleControllerControl() {
        controller = !controller;
    }

    public void SelectAllOnePlayerControls() {
        wasd = true;
        arrowKeys = true;
        mouse = true;
        controller = true;
    }

    [SerializeField] private GameObject onePlayerControlSelectionScreen;
    [SerializeField] private GameObject onePlayerWarningMessage;
    private CanvasGroup onePlayerWarningMessageCanvasGroup;
    private Animator onePlayerWarningMessageAnimator;
    [SerializeField] private GameObject onePlayerOnlyMouseWarningMessage;
    private CanvasGroup onePlayerOnlyMouseWarningMessageCanvasGroup;
    private Animator onePlayerOnlyMouseWarningMessageAnimator;

    public void ConfirmonePlayerControlSelection() {
        // Check that the player has selected at least one control type which will allow them to both move and attack
        if (wasd || arrowKeys || controller) {
            // Make the back button on the map screen send the player back to this screen
            screenToGoBackTo = onePlayerControlSelectionScreen;
            // Swap to the map screen
            mapSelectionScreen.SetActive(true);
            onePlayerControlSelectionScreen.SetActive(false);
            // Hide the warning message in case it was visible when the confirmation button was pressed
            onePlayerWarningMessageCanvasGroup.alpha = 0;
            onePlayerOnlyMouseWarningMessageCanvasGroup.alpha = 0;
        }
        // If the player has the mouse (and because of the previous if statement) none of the other controls selected, then send a special message
        else if (mouse) {
            // Stop any in progress warning messages so that they don't make the message disappear before it should if the button is pressed multiple times in quick succession
            StopCoroutine(ShowWarningMessage(onePlayerOnlyMouseWarningMessageAnimator));
            // Show an error message stating that controls have not been selected for each player and then hide it a second later
            StartCoroutine(ShowWarningMessage(onePlayerOnlyMouseWarningMessageAnimator));
        }
        else {
            // Stop any in progress warning messages so that they don't make the message disappear before it should if the button is pressed multiple times in quick succession
            StopCoroutine(ShowWarningMessage(onePlayerWarningMessageAnimator));
            // Show an error message stating that controls have not been selected for each player and then hide it a second later
            StartCoroutine(ShowWarningMessage(onePlayerWarningMessageAnimator));
        }
    }

    /*
     * Map Menu Back Button
     */
    public GameObject screenToGoBackTo;

    public void MapButtonGoBackToLastMenu() {
        screenToGoBackTo.SetActive(true);
    }

    /*
     * Map Selection Screen
     */
    private string mapToGoTo;

    public void SelectMapToGoTo(string newMapToGoTo) {
        mapToGoTo = newMapToGoTo;
    }

    [SerializeField] private List<SelectableButtons> mapButtons;
    [SerializeField] private GameObject mapWarningMessage;
    private CanvasGroup mapWarningMessageCanvasGroup;
    private Animator mapWarningMessageAnimator;

    public void StartGame() {
        // Check how many controls player one has selected (this should never go above 1)
        int mapsSelected = 0;
        foreach (SelectableButtons button in mapButtons) {
            if (button.Toggled) {
                mapsSelected++;
            }
        }
        if (mapsSelected > 1) Debug.LogWarning("More than 1 control selected for player one. # of maps selected: " + mapsSelected);

        // Check that one map, and no more or less than one map, has been selected
        if (mapsSelected == 1) {
            // Send the player(s) to the game
            SceneManager.LoadScene(mapToGoTo);

            // Hide the warning message in case it was visible when the confirmation button was pressed
            mapWarningMessageCanvasGroup.alpha = 0;
        }
        else {
            // Stop any in progress warning messages so that they don't make the message disappear before it should if the button is pressed multiple times in quick succession
            StopCoroutine(ShowWarningMessage(mapWarningMessageAnimator));
            // Show an error message stating that controls have not been selected for each player and then hide it a second later
            StartCoroutine(ShowWarningMessage(mapWarningMessageAnimator));
        }
    }

    // A coroutine which shows a warning message and then hides it one second later
    private IEnumerator ShowWarningMessage(Animator animator) {
        animator.SetTrigger("FadeIn");
        yield return new WaitForSeconds(1);
        animator.SetTrigger("FadeOut");
    }
}
