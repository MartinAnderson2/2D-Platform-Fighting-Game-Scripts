using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectableButtons : MonoBehaviour
{
    [SerializeField] private Button button;
    public Color pressedColour;

    private ColorBlock defaultColours;
    private ColorBlock selectedColours;
    [SerializeField] private bool toggled = false;

    public bool Toggled {
        get {
            return toggled;
        }
        set {
            toggled = value;

            if (toggled) {
                button.colors = selectedColours;
            }
            else {
                button.colors = defaultColours;
            }
        }
    }

    private void Awake() {
        if (!button) {
            button = GetComponent<Button>();
            Debug.Log("Button Reference not set");
        }

        defaultColours = button.colors;
        selectedColours = defaultColours;

        selectedColours.normalColor = pressedColour;
        selectedColours.selectedColor = pressedColour;
        selectedColours.pressedColor = pressedColour;
        selectedColours.highlightedColor = pressedColour;

        if (toggled) {
            Toggled = true;
        }
    }

    public void Toggle() {
        Toggled = !Toggled;
    }
}
