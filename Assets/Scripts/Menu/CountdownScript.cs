using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountdownScript : MonoBehaviour
{
    private List<Player> players = new List<Player>();
    private AI aI;

    private bool thereIsAnAI = false;

    private float startingRealtime;
    private bool startingRealtimeSet = false;
    [SerializeField] private float countdownLength = 3;
    [SerializeField] private bool enableCountdown = true;

    [SerializeField] private TextMeshProUGUI countdownText;
    private float currentCountdownNumber;

    // Awake is called when the script instance is being loaded
    private void Awake() {
        if (!enableCountdown) {
            Destroy(gameObject);
        }
        else countdownText.enabled = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Find the or both of the players
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player")) {
            // Store a reference to the player script
            players.Add(player.GetComponent<Player>());
        }

        // Find the AI, if there is one
        GameObject aIObject = GameObject.FindGameObjectWithTag("AI");
        // Check that there is an AI
        if (aIObject) {
            // Try and get the AI script from the AI
            if (aIObject.TryGetComponent<AI>(out AI aIScript)) {
                // Store a reference to the AI script
                aI = aIScript;
                // Store the fact that there is an AI and a player (as opposed to two players)
                thereIsAnAI = true;
            }
            // Log a warning if a script cannot be found, for some reason
            else Debug.LogWarning("Object tagged with AI did not have the AI script on it (Name of object: " + aIObject.name + ")");
        }
        // Set the AI to be false just to be safe
        else thereIsAnAI = false;

        // If there are three or more characters, log an error
        if (thereIsAnAI && players.Count > 1) {
            Debug.LogError("There are more than the maximum number of characters");
        }

        /*Prevent Movement*/

        // Set the time scale to 0, which should prevent all movement
        Time.timeScale = 0;
        // Freeze the player(s)'s movement, just to be certain that they cannot do anything during the countdown
        if (thereIsAnAI) {
            players.First().freezeMovement = true;
        }
        else {
            foreach (Player player in players) {
                player.freezeMovement = true;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!startingRealtimeSet) {
            startingRealtime = Time.realtimeSinceStartup;
            startingRealtimeSet = true;
            currentCountdownNumber = countdownLength;
            countdownText.text = currentCountdownNumber.ToString();
            StartCoroutine(CountDown());
        }

        // If the cooldown is over
        if (Time.realtimeSinceStartup >= countdownLength + startingRealtime) {
            // Set the time scale back to normal
            Time.timeScale = 1;
            // Unfreeze the player(s)'s movement
            if (thereIsAnAI) {
                players.First().freezeMovement = false;
            }
            else {
                foreach (Player player in players) {
                    player.freezeMovement = false;
                }
            }
        }
    }

    private IEnumerator CountDown() {
        yield return new WaitForSecondsRealtime(1);
        currentCountdownNumber--;
        countdownText.text = currentCountdownNumber.ToString();
        if (currentCountdownNumber >= 1) {
            StartCoroutine(CountDown());
        }
        else {
            countdownText.enabled = false;

            // Destroy the countdown script since it is no longer necessary and is a waste of performance
            Destroy(gameObject);
        }
    }
}
