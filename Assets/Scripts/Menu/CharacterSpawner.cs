using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    private SelectionScreenScript selectionScreenScript;

    [SerializeField] private GameObject spawnPosition1;
    [SerializeField] private GameObject spawnPosition2;

    [SerializeField] private int playerOneLayer;
    [SerializeField] private int playerTwoLayer;

    [SerializeField] private GameObject knight;
    [SerializeField] private GameObject scientist;
    [SerializeField] private GameObject astronaut;
    [SerializeField] private GameObject AI;

    private GameObject GetCharacter(Characters character) {
        return character switch {
            Characters.knight => knight,
            Characters.scientist => scientist,
            Characters.astronaut => astronaut,
            _ => null,
        };
    }

    // Start is called before the first frame update
    void Start()
    {
        selectionScreenScript = SelectionScreenScript.instance;

        // If there are 2 players
        if (selectionScreenScript.playerSetup == PlayerSetup.playerVsPlayer) {
            // Create player one
            GameObject playerOne = Instantiate(GetCharacter(selectionScreenScript.playerOneCharacter), spawnPosition1.transform.position, Quaternion.identity);
            // Create player two
            GameObject playerTwo = Instantiate(GetCharacter(selectionScreenScript.playerTwoCharacter), spawnPosition2.transform.position, Quaternion.identity);

            // Get player scripts
            Player playerOneScript = playerOne.GetComponent<Player>();
            Player playerTwoScript = playerTwo.GetComponent<Player>();

            // Set the player setup on the player scripts
            playerOneScript.playerSetup = selectionScreenScript.playerSetup;
            playerTwoScript.playerSetup = selectionScreenScript.playerSetup;

            // Tell each player script which player they are when they are spawned in
            playerOneScript.player = WhichPlayer.playerOne;
            playerTwoScript.player = WhichPlayer.playerTwo;

            // Set player one's controls
            // Cycle through the movement (and attacking) controls to determine which one is selected and set the appropriate value on the player script
            switch (selectionScreenScript.playerOneControls) {
                case MultiplayerControl.WASD:
                    playerOneScript.wasd = true;
                    break;
                case MultiplayerControl.arrowKeys:
                    playerOneScript.arrowKeys = true;
                    break;
                case MultiplayerControl.controller:
                    playerOneScript.controllerOne = true;
                    break;
                default:
                    Debug.Log("No controls selected for player one! selectionScreenScript.playerOneControls: " + selectionScreenScript.playerOneControls);
                    break;
            }
            // Check if the player has the mouse selected for attacks. If they do, set the appropriate value on the player script
            if (selectionScreenScript.playerWhoHasMouseSelected == WhichPlayer.playerOne) {
                playerOneScript.mouse = true;
            }
            
            // Set player two's controls
            // Cycle through the movement (and attacking) controls to determine which one is selected and set the appropriate value on the player script
            switch (selectionScreenScript.playerTwoControls) {
                case MultiplayerControl.WASD:
                    playerTwoScript.wasd = true;
                    break;
                case MultiplayerControl.arrowKeys:
                    playerTwoScript.arrowKeys = true;
                    break;
                case MultiplayerControl.controller:
                    playerTwoScript.controllerTwo = true;
                    break;
                default:
                    Debug.Log("No controls selected for player two! selectionScreenScript.playerTwoControls: " + selectionScreenScript.playerTwoControls);
                    break;
            }
            // Check if the player has the mouse selected for attacks. If they do, set the appropriate value on the player script
            if (selectionScreenScript.playerWhoHasMouseSelected == WhichPlayer.playerTwo) {
                playerTwoScript.mouse = true;
            }

            // Set player one's collision layers to player one
            playerOne.layer = playerOneLayer;
            GameObject playerOneChild = playerOne.transform.GetChild(0).gameObject;
            playerOneChild.layer = playerOneLayer;
            playerOneChild.transform.GetChild(0).gameObject.layer = playerOneLayer;

            // Set player two's collision layers to player two
            playerTwo.layer = playerTwoLayer;
            GameObject playerTwoChild = playerTwo.transform.GetChild(0).gameObject;
            playerTwoChild.layer = playerTwoLayer;
            playerTwoChild.transform.GetChild(0).gameObject.layer = playerTwoLayer;
        }
        // If there is 1 player and an AI
        else if (selectionScreenScript.playerSetup == PlayerSetup.playerVsAI) {
            // Spawn the player
            GameObject player = Instantiate(GetCharacter(selectionScreenScript.onePlayerCharacter), spawnPosition1.transform.position, Quaternion.identity);
            // Spawn the AI
            GameObject aI = Instantiate(AI, spawnPosition2.transform.position, Quaternion.identity);

            // Get player scripts
            Player playerScript = player.GetComponent<Player>();
            AI aIScript = aI.GetComponent<AI>();

            // Set the player setup on the player script
            playerScript.playerSetup = selectionScreenScript.playerSetup;

            // Set the AI's difficulty
            aIScript.AiDifficulty = selectionScreenScript.aIDifficulty;

            // Set the player's controls
            playerScript.wasd = selectionScreenScript.wasd;
            playerScript.arrowKeys = selectionScreenScript.arrowKeys;
            playerScript.allControllers = selectionScreenScript.controller;
            playerScript.mouse = selectionScreenScript.mouse;

            // Place the player on the correct layer
            player.layer = playerOneLayer;
            GameObject playerChild = player.transform.GetChild(0).gameObject;
            playerChild.layer = playerOneLayer;
            playerChild.transform.GetChild(0).gameObject.layer = playerOneLayer;
        }

        Destroy(selectionScreenScript.gameObject);
    }
}
