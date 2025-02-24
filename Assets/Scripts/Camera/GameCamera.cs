using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public class GameCamera : MonoBehaviour {
    private Camera cameraReference;
    [SerializeField] private List<Transform> characters;

    [SerializeField] private float movementSpeed;
    [SerializeField] private float scalingSpeed;
    [SerializeField] private float margin;

    /*[SerializeField] private Range cameraOutlineLocation;
    [SerializeField] private bool updateCameraOutlineLocation;
    [SerializeField] private bool inBounds;*/

    [SerializeField] private Range cameraBounds;
    [SerializeField] private float startingCameraSize;
    [SerializeField] private Vector2 startingCameraLocation;
    [SerializeField] private float maximumCameraSize;
    [SerializeField] private float minimumCameraSize;

    // A bounding box for the camera's current outline location minus the buffer that is only updated once per frame
    private Bounds cameraBufferOutline;
    public Bounds CameraBufferOutline {
        get {
            if (!cameraBufferUpdatedThisFrame) {
                cameraBufferOutline = new Bounds(transform.position, new Vector2((CameraHalfHeight * CameraAspectRatio - margin) * 2, (CameraHalfHeight - margin) * 2)); // Set the size to the camera size minus the margins
                cameraBufferUpdatedThisFrame = true;
            }
            return cameraBufferOutline;
        }
    }
    private bool cameraBufferUpdatedThisFrame = false;

    private float cameraHalfHeight;
    private float CameraHalfHeight {
        get {
            if (!cameraHalfHeightUpdated) {
                cameraHalfHeight = cameraReference.orthographicSize;
                cameraHalfHeightUpdated = true;
            }
            return cameraHalfHeight;
        }
        set {
            cameraHalfHeight = value;
            cameraReference.orthographicSize = value;
        }
    }
    private bool cameraHalfHeightUpdated = false;

    private float cameraAspectRatio;
    public float CameraAspectRatio {
        get {
            if (!cameraAspectUpdated) {
                cameraAspectRatio = cameraReference.aspect;
                cameraAspectUpdated = true;
            }
            return cameraAspectRatio;
        }
    }
    private bool cameraAspectUpdated = false;

    // Start is called before the first frame update
    void Start() {
        cameraReference = GetComponent<Camera>();

        foreach (GameObject character in GameObject.FindGameObjectsWithTag("Player"))
            characters.Add(character.transform.GetChild(0));
        foreach (GameObject aI in GameObject.FindGameObjectsWithTag("AI"))
            characters.Add(aI.transform.GetChild(0));

        CameraHalfHeight = startingCameraSize;
        transform.position = startingCameraLocation;
    }

    // Update is called once per frame
    void Update() {
        ResetUpdatedThisFrame();

        // Scale camera
        float smallestDistanceIn = CameraBufferOutline.extents.x + CameraBufferOutline.extents.y; // The closest inside a character is to the margins. This value is larger than the maximum value (not squared and square rooted to save performance) in order to check it has been changed
        float largestDistanceOut = 0; // The furthest outside a character is from the margins
        foreach (Transform character in characters) {
            if (character == null) continue;
            Vector3 characterPosition = character.position;

            if (CameraBufferOutline.Contains(characterPosition)) { // If the character is within the bounds
                float distanceFromMargins = CameraBufferOutline.SqrDistance(characterPosition); // Get their distance from the bounds
                // If the character is closer to the bounds than the previous characters, update the distance
                if (distanceFromMargins < smallestDistanceIn)
                    smallestDistanceIn = distanceFromMargins;
            }
            else { // If the character is outside the bounds
                float distanceFromMargins = CameraBufferOutline.SqrDistance(characterPosition); // Get their distance from the bounds
                // If the character is further from the bounds than the previous characters, update the distance
                if (distanceFromMargins > largestDistanceOut)
                    largestDistanceOut = distanceFromMargins;
            }
        }

        if (largestDistanceOut != 0) {
            CameraHalfHeight = Mathf.Lerp(CameraHalfHeight, CameraHalfHeight + largestDistanceOut, scalingSpeed * Time.deltaTime);
        }
        else if (smallestDistanceIn != CameraBufferOutline.extents.x + CameraBufferOutline.extents.y) {
            CameraHalfHeight = Mathf.Lerp(CameraHalfHeight, CameraHalfHeight - smallestDistanceIn, scalingSpeed * Time.deltaTime);
        }


        // Old Scaling Code:

        // Scale camera
        /*bool anyOutOfBounds = false;
        float xDistance = 0;
        float yDistance = 0;

        foreach (Transform character in characters) {
            if (character == null) continue;
            Vector3 characterPosition = character.position;

            if (CameraBufferOutline.Contains(characterPosition)) { // If the character is in the bounds

            }
            else { // If the character is outside of the bounds
                anyOutOfBounds = true;
                float distanceFromX = Mathf.Abs(characterPosition.x) - CameraBufferOutline.max.x;
                float distanceFromY = Mathf.Abs(characterPosition.y) - CameraBufferOutline.max.y;

                if (distanceFromX > xDistance)
                    xDistance = distanceFromX;
                if (distanceFromY > yDistance)
                    yDistance = distanceFromY;
            }
        }
        if (!anyOutOfBounds) {
            Debug.Log("All in bounds");
        }
        else {
            if (xDistance * CameraAspectRatio > yDistance)
                CameraHalfHeight = Mathf.Clamp(Mathf.Lerp(CameraHalfHeight * CameraAspectRatio - margin, CameraHalfHeight * CameraAspectRatio + xDistance, scalingSpeed * Time.deltaTime) / CameraAspectRatio + margin, minimumCameraSize, maximumCameraSize);
            else
                CameraHalfHeight = Mathf.Clamp(Mathf.Lerp(CameraHalfHeight - margin, CameraHalfHeight + yDistance, scalingSpeed * Time.deltaTime) + margin, minimumCameraSize, maximumCameraSize);
        }*/

        // Move camera
        Vector2 middlePosition = MiddleOfCharacters();
        middlePosition.x = Mathf.Clamp(middlePosition.x, cameraBounds.left + CameraHalfHeight * CameraAspectRatio,
            cameraBounds.right - CameraHalfHeight * CameraAspectRatio);
        middlePosition.y = Mathf.Clamp(middlePosition.y, cameraBounds.bottom + CameraHalfHeight,
            cameraBounds.top - CameraHalfHeight);
        transform.position = Vector2.Lerp(transform.position, middlePosition, movementSpeed * Time.deltaTime);
    }

    private Vector2 MiddleOfCharacters() {
        if (characters.Count == 0) return transform.position; // There are no players
        Vector2 middleOfPlayers = Vector2.zero;
        int livePlayers = 0;

        foreach (Transform character in characters) {
            if (character == null || !character.gameObject.activeInHierarchy) continue;

            middleOfPlayers += (Vector2)character.position;
            livePlayers++;
        }

        //Return the average position
        return middleOfPlayers / livePlayers;
    }

    private void ResetUpdatedThisFrame() {
        cameraBufferUpdatedThisFrame = false;
        cameraHalfHeightUpdated = false;
        cameraAspectUpdated = false;
    }

    /*private void OnValidate() {
        if (updateCameraOutlineLocation) {
            cameraOutlineLocation = new Range(-CameraHalfHeight * cameraAspect + transform.position.x,
                CameraHalfHeight + transform.position.y,
                CameraHalfHeight * cameraAspect + transform.position.x,
                -CameraHalfHeight + transform.position.y);

            updateCameraOutlineLocation = false;
        }

        if (inBounds) {
            if (IsCameraInBounds()) {
                Debug.Log("In bounds");
            }
            else Debug.LogWarning("Not in bounds");

            inBounds = false;
        }
    }*/

    private void OnDrawGizmos() {
        Gizmos.DrawWireCube(cameraBufferOutline.center, cameraBufferOutline.size);
    }

    private bool IsCameraInBounds() {
        return -CameraHalfHeight * CameraAspectRatio + transform.position.x >= cameraBounds.left &&
                CameraHalfHeight + transform.position.y <= cameraBounds.top &&
                CameraHalfHeight * CameraAspectRatio + transform.position.x <= cameraBounds.right &&
                -CameraHalfHeight + transform.position.y >= cameraBounds.bottom;
    }

    public void RemovePlayer(GameObject player) {
        characters.Remove(player.transform);
    }
}

[Serializable]
public class Range {
    public float left;
    public float top;
    public float right;
    public float bottom;

    public Range(float left, float top, float right, float bottom) {
        this.left = left;
        this.top = top;
        this.right = right;
        this.bottom = bottom;
    }

    public static Range zero = new(0, 0, 0, 0);
}