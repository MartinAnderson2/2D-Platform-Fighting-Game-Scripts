using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    /*References*/
    // Set at runtime
    private Rigidbody2D rigidbodyReference;
    private Animator animatorReference;
    private Transform playerTransform;
    [SerializeField] private BoxCollider2D boxColliderReference;

    /*Values*/
    // Game states
    public PlayerSetup playerSetup;
    public WhichPlayer player = WhichPlayer.none;
    public bool wasd = false;
    public bool arrowKeys = false;
    public bool controllerOne = false;
    public bool controllerTwo = false;
    public bool allControllers = false;
    public bool mouse = false;

    // Player states
    [SerializeField] private Characters playerCharacter;
    [SerializeField] private PlayerState playerCurrentState; // The player's current state
    public PlayerState PlayerCurrentState {
        get {
            return playerCurrentState;
        }
    }

    [SerializeField] private PlayerAttack playerCurrentAttack = PlayerAttack.none; // The player's current attack
    public PlayerAttack PlayerCurrentAttack {
        get {
            return playerCurrentAttack;
        }
    }

    private Sector playerSector = Sector.safe;
    public Sector PlayerSector {
        get {
            return playerSector;
        }
    }

    [SerializeField] private Direction playerDirection = Direction.right;
    public Direction PlayerDirection { 
        get {
            return playerDirection;
        }
    }
    public bool freezeMovement = false;
    private bool walking = false;
    public bool Walking {
        get {
            return walking;
        }
    }
    private bool newWalking = false;

    // Important Player values
    [SerializeField] private float maximumKnockbackMultiplier = 3;
    [SerializeField] private float damageTaken = 0;
    [SerializeField] private float maximumDamage = 50;
    public float DamageTaken {
        get {
            return damageTaken;
        }
        private set {
            damageTaken = Mathf.Clamp(value, 0, maximumDamage);
        }
    }
    private float timeLastDamaged = 0; // This is not actually the time that the player was last damamged but rather the time that the attack which most recently damaged them started
    [SerializeField, Range(0f, 10f)] private float walkSpeed = 1;
    public float WalkSpeed { get {
            return walkSpeed;
        }
    }
    [SerializeField] private float minimumDistanceFromGround;
    public LayerMask groundLayer;
    private float defaultGravityScale;
    private bool atDefaultGravityScale = true;
    [SerializeField] private float slidingSpeed;

    // Knockback
    [SerializeField] private float timeLastKnockedBack; // The time the player last took damage and got knocked back
    [SerializeField] private float knockbackMovementCooldown; // The time for which the player cannot move after being knocked back (in order to make sure they actually get knocked back)
    [SerializeField] private float verticalToHorizontalKnockback; // The ratio of vertical knockback to horizontal knockback (vertical knockback divided by horizontal knockback)
    [SerializeField] private bool takeKnockbackOnGround = true; // A variable which decides whether or not players can instantly change their direction when they touch the ground
    public enum DirectionForceApplied {
        right,
        left,
        none
    }
    [SerializeField] private DirectionForceApplied directionForceRecentlyApplied = DirectionForceApplied.none; // A variable which stores the direction in which the player was pushed in order to prevent the player from instantly changing their velocity (which they can only do when they are within their walk speed) after a force has been applied to them

    // Input
    /// <summary>
    /// Use HorizontalInput instead of horizontalInput to ensure that the value has been updated
    /// </summary>
    private float horizontalInput;
    private float HorizontalInput {
        get {
            // Check that the input was not already updated this frame
            if (!horizontalInputUpdated) {
                // Set the horizontal Input to 0 so that it is known if it has been updated by any of the controls
                horizontalInput = 0;

                // If the player has the WASD controls selected
                if (wasd) {
                    // Set the player's movement to 1 if they are pressing D, -1 if they are pressing A, or 0 if they are pressing neither
                    horizontalInput = Input.GetAxisRaw("WASDHorizontal");
                }
                // If the player has the Arrow Keys selected as controls
                if (arrowKeys) {
                    // If no other controls are selected or if they are not pressed
                    if (horizontalInput == 0) {
                        // Set horizontal Input to 1 if they are pressing Right Arrow, -1 if they are pressing Left Arrow, or 0 if they are pressing neither
                        horizontalInput = Input.GetAxisRaw("ArrowKeysHorizontal");
                    }
                    // If other controls have been pressed
                    else {
                        // Add the horizontal Input value of the Arrow Keys to the WASD input value, but do not go over 1 or under -1. So if WASD is moving right (+1) and Arrow Keys are moving left (-1) then vertical input is 0.
                        horizontalInput = Mathf.Clamp(horizontalInput + Input.GetAxisRaw("ArrowKeysHorizontal"), -1, 1);
                    }
                }
                // If there is only one player and they have selected the Controller controls
                if (allControllers) {
                    // If no other controls are selected or if they are not pressed
                    if (horizontalInput == 0) {
                        // Set the horizontal Input to 1 if they are moving the joystick or gamepad right, -1 if they are moving them left, or 0 if they are not moving either
                        horizontalInput = Input.GetAxisRaw("AllControllersHorizontal");
                    }
                    // If other controls have been pressed
                    else {
                        // Add the horizontal Input value of the Controllers to the Arrow Keys and or WASD input value, but do not go over 1 or under -1. So if WASD and Arrow Keys are moving upwards (+1) but Controllers are moving downards (-1) then vertical input is 0.
                        horizontalInput = Mathf.Clamp(horizontalInput + Input.GetAxisRaw("AllControllersHorizontal"), -1, 1);
                    }
                }
                else if (controllerOne) {
                    // If no other controls are selected or if they are not pressed
                    if (horizontalInput == 0) {
                        // Set the horizontal Input to 1 if they are moving the joystick or gamepad right, -1 if they are moving them left, or 0 if they are not moving either
                        horizontalInput = Input.GetAxisRaw("ControllerOneHorizontal");
                    }
                    // If other controls have been pressed
                    else {
                        // Add the horizontal Input value of the Controller One to the Arrow Keys and or WASD input value, but do not go over 1 or under -1. So if WASD and Arrow Keys are moving upwards (+1) but Controller One is moving downards (-1) then vertical input is 0.
                        horizontalInput = Mathf.Clamp(horizontalInput + Input.GetAxisRaw("ControllerOneHorizontal"), -1, 1);
                    }
                }
                else if (controllerTwo) {
                    // If no other controls are selected or if they are not pressed
                    if (horizontalInput == 0) {
                        // Set the horizontal Input to 1 if they are moving the joystick or gamepad right, -1 if they are moving them left, or 0 if they are not moving either
                        horizontalInput = Input.GetAxisRaw("ControllerTwoHorizontal");
                    }
                    // If other controls have been pressed
                    else {
                        // Add the horizontal Input value of the Controller Two to the Arrow Keys and or WASD input value, but do not go over 1 or under -1. So if WASD and Arrow Keys are moving upwards (+1) but Controller Two is moving downards (-1) then vertical input is 0.
                        horizontalInput = Mathf.Clamp(horizontalInput + Input.GetAxisRaw("ControllerTwoHorizontal"), -1, 1);
                    }
                }
                // Ensure that the input is not updated again this frame
                horizontalInputUpdated = true;
            }
            // Since the input has been confirmed to have been updated, return its value
            return horizontalInput;
        }
    }
    private bool horizontalInputUpdated = false;

    /// <summary>
    /// Use VerticalInput instead of verticalInput to ensure that the value has been updated
    /// </summary>
    private float verticalInput;
    private float VerticalInput {
        get {
            // Check that the input was not already updated this frame
            if (!verticalInputUpdated) {
                // Set the vertical Input to 0 so that it is known if it has been updated by any of the controls
                verticalInput = 0;

                // If the player has selected the WASD controls
                if (wasd) {
                    // Set the vertical Input to 1 if they are pressing W, -1 if they are pressing S, or 0 if they are not pressing either
                    verticalInput = Input.GetAxisRaw("WASDVertical");
                }
                // If the player has selected the Arrow Keys as controls
                if (arrowKeys) {
                    // If no other controls are selected or if they are not pressed
                    if (verticalInput == 0) {
                        // Set the vertical Input to 1 if they are pressing Up Arrow, -1 if they are pressing Down Arrow, or 0 if they are not pressing either
                        verticalInput = Input.GetAxisRaw("ArrowKeysVertical");
                    }
                    // If other controls have been pressed
                    else {
                        // Add the vertical input value of the Arrow Keys to the WASD input value, but do not go over 1 or under -1. So if WASD is moving upwards (+1) and Arrow Keys are moving downards (-1) then vertical input is 0.
                        verticalInput = Mathf.Clamp(verticalInput + Input.GetAxisRaw("ArrowKeysVertical"), -1, 1);
                    }                    
                }
                // If there is only one player and they have selected the Controller controls
                if (allControllers) {
                    // If no other controls are selected or if they are not pressed
                    if (verticalInput == 0) {
                        // Set the vertical Input to 1 if they are pressing A on an Xbox controller or X on a PS Controller or if they are moving up on the joystick, -1 if they are moving down on the joystick, or 0 if they are doing none of the above
                        verticalInput = Input.GetAxisRaw("AllControllersVertical");
                    }
                    // If other controls have been pressed
                    else {
                        // Add the vertical Input value of the Controllers to the Arrow Keys and or WASD input value, but do not go over 1 or under -1. So if WASD and Arrow Keys are moving upwards (+1) but Controllers are moving downards (-1) then vertical input is 0.
                        verticalInput = Mathf.Clamp(verticalInput + Input.GetAxisRaw("AllControllersVertical"), -1, 1);
                    }
                }
                // If there are two players and this is player One
                else if (controllerOne) {
                    // If no other controls are selected or if they are not pressed
                    if (verticalInput == 0) {
                        // Set the vertical Input to 1 if they are pressing A on an Xbox controller or X on a PS Controller or if they are moving up on the joystick, -1 if they are moving down on the joystick, or 0 if they are doing none of the above
                        verticalInput = Input.GetAxisRaw("ControllerOneVertical");
                    }
                    // If other controls have been pressed
                    else {
                        // Add the vertical Input value of Controller One to the Arrow Keys and or WASD input value, but do not go over 1 or under -1. So if WASD and Arrow Keys are moving upwards (+1) but Controller One is moving downards (-1) then vertical input is 0.
                        verticalInput = Mathf.Clamp(verticalInput + Input.GetAxisRaw("ControllerOneVertical"), -1, 1);
                    }
                }
                // If there are two players and this is player Two
                else if (controllerTwo) {
                    // If no other controls are selected or if they are not pressed
                    if (verticalInput == 0) {
                        // Set the vertical Input to 1 if they are pressing A on an Xbox controller or X on a PS Controller or if they are moving up on the joystick, -1 if they are moving down on the joystick, or 0 if they are doing none of the above
                        verticalInput = Input.GetAxisRaw("ControllerTwoVertical");
                    }
                    // If other controls have been pressed
                    else {
                        // Add the vertical Input value of Controller Two to the Arrow Keys and or WASD input value, but do not go over 1 or under -1. So if WASD and Arrow Keys are moving upwards (+1) but Controller Two is moving downards (-1) then vertical input is 0.
                        verticalInput = Mathf.Clamp(verticalInput + Input.GetAxisRaw("ControllerTwoVertical"), -1, 1);
                    }
                }
                // Ensure that the input is not updated again this frame
                verticalInputUpdated = true;
            }
            // Since the input has been confirmed to have been updated, return its value
            return verticalInput;
        }
    }
    private bool verticalInputUpdated = false;

    /// <summary>
    /// Use LightAttackPressed instead of lightAttackPressed to ensure that the value has been updated
    /// </summary>
    private bool lightAttackPressed;
    private bool LightAttackPressed {
        get {
            // Check that the input was not already updated this frame
            if (!lightAttackPressedUpdated) {
                // Set lightAttackPressed to false so that it is known if it has been updated by any of the controls
                lightAttackPressed = false;

                // If the player has the WASD controls selected
                if (wasd) {
                    // Set lightAttackPressed to true if the player is pressing E. Otherwise it is set to false
                    lightAttackPressed = Input.GetButton("WASDLightAttack");
                }
                // If the player has the Arrow Keys selected as controls
                if (arrowKeys) {
                    // If no other controls are selected or if they have not been pressed
                    if (!lightAttackPressed) {
                        // Set lightAttackPressed to true if the player is pressing L. Otherwise it is set to false
                        lightAttackPressed = Input.GetButton("ArrowKeysLightAttack");
                    }
                    // Do nothing if light Attack has been pressed by another control since there is no way for this control to cancel that out
                }
                // If the player has the Mouse controls selected
                if (mouse) {
                    // If no other controls are selected or if they have not been pressed
                    if (!lightAttackPressed) {
                        // Set lightAttackPressed to true if the player is pressing the left mouse button. Otherwise it is set to false
                        lightAttackPressed = Input.GetButton("MouseLightAttack");
                    }
                    // Do nothing if light Attack has been pressed by another control since there is no way for this control to cancel that out
                }
                // If there is only one player and they have selected the Controller controls
                if (allControllers) {
                    // If no other controls are selected or if they are not pressed
                    if (!lightAttackPressed) {
                        // Set ligthAttackPressed to true if they are pressing X on an Xbox controller or Square on a PS Controller or if they are pressing Right Bumper, or false if they are doing none of the above
                        lightAttackPressed = Input.GetButton("AllControllersLightAttack");
                    }
                    // Do nothing if light Attack has been pressed by another control since there is no way for this control to cancel that out
                }
                else if (controllerOne) {
                    // If no other controls are selected or if they are not pressed
                    if (!lightAttackPressed) {
                        // Set ligthAttackPressed to true if they are pressing X on an Xbox controller or Square on a PS Controller or if they are pressing Right Bumper, or false if they are doing none of the above
                        lightAttackPressed = Input.GetButton("ControllerOneLightAttack");
                    }
                    // Do nothing if light Attack has been pressed by another control since there is no way for this control to cancel that out
                }
                else if (controllerTwo) {
                    // If no other controls are selected or if they are not pressed
                    if (!lightAttackPressed) {
                        // Set ligthAttackPressed to true if they are pressing X on an Xbox controller or Square on a PS Controller or if they are pressing Right Bumper, or false if they are doing none of the above
                        lightAttackPressed = Input.GetButton("ControllerTwoLightAttack");
                    }
                    // Do nothing if light Attack has been pressed by another control since there is no way for this control to cancel that out
                }
                // Ensure that the input is not updated again this frame
                lightAttackPressedUpdated = true;
            }
            // Since the input has been confirmed to have been updated, return its value
            return lightAttackPressed;
        }
    }
    private bool lightAttackPressedUpdated = false;

    /// <summary>
    /// Use HeavyAttackPressed instead of heavyAttackPressed to ensure that the value has been updated
    /// </summary>
    private bool heavyAttackPressed;
    private bool HeavyAttackPressed {
        get {
            // Check that the input was not already updated this frame
            if (!heavyAttackUpdated) {
                // Set heavyAttackPressed to false so that it is known if it has been updated by any of the controls
                heavyAttackPressed = false;

                // If the player has the WASD controls selected
                if (wasd) {
                    // Set heavyAttackPressed to true if the player is pressing q, otherwise it is set to false
                    heavyAttackPressed = Input.GetButton("WASDHeavyAttack");
                }
                // If the player has the Arrow Keys selected as controls
                if (arrowKeys) {
                    // If no other controls are selected or if they have not been pressed
                    if (!heavyAttackPressed) {
                        // Set heavyAttackPressed to true if the player is pressing J. Otherwise it is set to false
                        heavyAttackPressed = Input.GetButton("ArrowKeysHeavyAttack");
                    }
                    // Do nothing if heavy Attack has been pressed by another control since there is no way for this control to cancel that out
                }
                // If the player has the Mouse controls selected
                if (mouse) {
                    // If no other controls are selected or if they have not been pressed
                    if (!heavyAttackPressed) {
                        // Set heavyAttackPressed to true if the player is pressing the right mouse button. Otherwise it is set to false
                        heavyAttackPressed = Input.GetButton("MouseHeavyAttack");
                    }
                    // Do nothing if heavy Attack has been pressed by another control since there is no way for this control to cancel that out
                }
                // If there is only one player and they have selected the Controller controls
                if (allControllers) {
                    // If no other controls are selected or if they are not pressed
                    if (!heavyAttackPressed) {
                        // Set heavyAttackPressed to true if they are pressing Y on an Xbox controller or Triangle on a PS Controller or if they are pressing Right Trigger, or false if they are doing none of the above
                        heavyAttackPressed = Input.GetButton("AllControllersHeavyAttack");
                    }
                    // Do nothing if heavy Attack has been pressed by another control since there is no way for this control to cancel that out
                }
                else if (controllerOne) {
                    // If no other controls are selected or if they are not pressed
                    if (!heavyAttackPressed) {
                        // Set heavyAttackPressed to true if they are pressing Y on an Xbox controller or Triangle on a PS Controller or if they are pressing Right Trigger, or false if they are doing none of the above
                        heavyAttackPressed = Input.GetButton("ControllerOneHeavyAttack");
                    }
                    // Do nothing if heavy Attack has been pressed by another control since there is no way for this control to cancel that out
                }
                else if (controllerTwo) {
                    // If no other controls are selected or if they are not pressed
                    if (!heavyAttackPressed) {
                        // Set heavyAttackPressed to true if they are pressing Y on an Xbox controller or Triangle on a PS Controller or if they are pressing Right Trigger, or false if they are doing none of the above
                        heavyAttackPressed = Input.GetButton("ControllerTwoHeavyAttack");
                    }
                    // Do nothing if heavy Attack has been pressed by another control since there is no way for this control to cancel that out
                }
                // Ensure that the input is not updated again this frame
                heavyAttackUpdated = true;
            }
            // Since the input has been confirmed to have been updated, return its value
            return heavyAttackPressed;
        }
    }
    private bool heavyAttackUpdated = false;

    /// <summary>
    /// This variable does not tell you whether or not the player is blocking. Use BlockingPressed instead of blockingPressed to ensure that the value has been updated
    /// </summary>
    private bool blockingPressed;
    private bool BlockingPressed {
        get {
            // Check that the input was not already updated this frame
            if (!blockingPressedUpdated) {
                // Set blockingPressed to false so that it is known if it has been updated by any of the controls
                blockingPressed = false;

                // If the player has the WASD controls selected
                if (wasd) {
                    // Set blockingPressed to true if the player is pressing Z or S this frame. Otherwise it is set to false
                    blockingPressed = Input.GetButton("WASDBlock");
                }
                // If the player has the Arrow Keys selected as controls
                if (arrowKeys) {
                    // If no other controls are selected or if they have not been pressed
                    if (!blockingPressed) {
                        // Set blockingPressed to true if the player is pressing K or Down Arrow. Otherwise it is set to false
                        blockingPressed = Input.GetButton("ArrowKeysBlock");
                    }
                    // Do nothing if blocking has been pressed by another control since there is no way for this control to cancel that out
                }
                // If there is only one player and they have selected the Controller controls
                if (allControllers) {
                    // If no other controls are selected or if they are not pressed
                    if (!blockingPressed) {
                        // Set blockingPressed to true if they are pressing B on an Xbox controller or Circle on a PS Controller or if they are pressing Left Bumper, or false if they are doing none of the above
                        blockingPressed = Input.GetButton("AllControllersBlock");
                    }
                    // Do nothing if blocking Pressed has been pressed by another control since there is no way for this control to cancel that out
                }
                else if (controllerOne) {
                    // If no other controls are selected or if they are not pressed
                    if (!blockingPressed) {
                        // Set blockingPressed to true if they are pressing B on an Xbox controller or Circle on a PS Controller or if they are pressing Left Bumper, or false if they are doing none of the above
                        blockingPressed = Input.GetButton("ControllerOneBlock");
                    }
                    // Do nothing if blocking Pressed has been pressed by another control since there is no way for this control to cancel that out
                }
                else if (controllerTwo) {
                    // If no other controls are selected or if they are not pressed
                    if (!blockingPressed) {
                        // Set blockingPressed to true if they are pressing B on an Xbox controller or Circle on a PS Controller or if they are pressing Left Bumper, or false if they are doing none of the above
                        blockingPressed = Input.GetButton("ControllerTwoBlock");
                    }
                    // Do nothing if blocking Pressed has been pressed by another control since there is no way for this control to cancel that out
                }
                // Ensure that the input is not updated again this frame
                blockingPressedUpdated = true;
            }

            // Since the input has been confirmed to have been updated, return its value
            return blockingPressed;
        }
    }
    private bool blockingPressedUpdated = false;

    /// <summary>
    /// This variable does not tell you whether or not the player is pressing the ability button. Use AbilityInput instead of AbilityInput to ensure that the value has been updated
    /// </summary>
    private bool abilityPressed;
    private bool AbilityPressed {
        get {
            // Check that the input was not already updated this frame
            if (!abilityPressedUpdated) {
                // Set blockingPressed to false so that it is known if it has been updated by any of the controls
                abilityPressed = false;

                // If the player has the WASD controls selected
                if (wasd) {
                    // Set blockingPressed to true if the player is pressing Z or S this frame. Otherwise it is set to false
                    abilityPressed = Input.GetButton("WASDAbility");
                }
                // If the player has the Arrow Keys selected as controls
                if (arrowKeys) {
                    // If no other controls are selected or if they have not been pressed
                    if (!abilityPressed) {
                        // Set blockingPressed to true if the player is pressing K or Down Arrow. Otherwise it is set to false
                        abilityPressed = Input.GetButton("ArrowKeysAbility");
                    }
                    // Do nothing if blocking has been pressed by another control since there is no way for this control to cancel that out
                }
                // If there is only one player and they have selected the Controller controls
                if (allControllers) {
                    // If no other controls are selected or if they are not pressed
                    if (!abilityPressed) {
                        // Set blockingPressed to true if they are pressing B on an Xbox controller or Circle on a PS Controller or if they are pressing Left Bumper, or false if they are doing none of the above
                        abilityPressed = Input.GetButton("AllControllersAbility");
                    }
                    // Do nothing if blocking Pressed has been pressed by another control since there is no way for this control to cancel that out
                }
                else if (controllerOne) {
                    // If no other controls are selected or if they are not pressed
                    if (!abilityPressed) {
                        // Set blockingPressed to true if they are pressing B on an Xbox controller or Circle on a PS Controller or if they are pressing Left Bumper, or false if they are doing none of the above
                        abilityPressed = Input.GetButton("ControllerOneAbility");
                    }
                    // Do nothing if blocking Pressed has been pressed by another control since there is no way for this control to cancel that out
                }
                else if (controllerTwo) {
                    // If no other controls are selected or if they are not pressed
                    if (!abilityPressed) {
                        // Set blockingPressed to true if they are pressing B on an Xbox controller or Circle on a PS Controller or if they are pressing Left Bumper, or false if they are doing none of the above
                        abilityPressed = Input.GetButton("ControllerTwoAbility");
                    }
                    // Do nothing if blocking Pressed has been pressed by another control since there is no way for this control to cancel that out
                }
                // Ensure that the input is not updated again this frame
                abilityPressedUpdated = true;
            }

            // Since the input has been confirmed to have been updated, return its value
            return abilityPressed;
        }
    }
    private bool abilityPressedUpdated = false;

    //private bool facingRightXFlip; // Whether the sprite renderer is Flipped on the X-Axis when the player is facing the right
    //private bool weaponFacingRightPositive; // When the weapon is facing the right whether its X position is positive

    /*Jumping*/
    private bool hasDoubleJumped = false;
    public bool HasDoubleJumped { get; }
    // Using boxcasting so this method of ground detection should no longer be necessary
    /*private Vector2 lastUpdateVelocity = Vector3.zero;
    private Vector2 acceleration = Vector3.zero;
    [SerializeField, Range(0f, 10f)] private float verticalAccelerationThreshold = 0.01f; // The amount of acceleration the player can have and still be able to jump*/
    [SerializeField, Range(0f, 60f)] private float jumpCooldown = 0.5f; // The minimum delay between the first jump and a double jump
    [SerializeField, Range(0f, 60f)] private float wallJumpControlFreeze = 0.1f; // The length of time for which horizontal controls are frozen after wall jumping in order to make the player actually get launched out
    [SerializeField, Range(0f, 25f)] float jumpVelocity = 1;
    [SerializeField, Range(0f, 25f)] float climbVelocity; // Upwards impulse when climbing
    [SerializeField, Range(0f, 25f)] float flipVelocity; // Sideways impulse when climbing
    private float timeLastJumped = 0; // The time (in terms of game time (Time.time)
    [SerializeField] private float timeLastWallJumped = -1000000000000000; // The time (in terms of game time (Time.time)

    /*Attacking*/
    private float timeLastAttacking;
    public float TimeLastAttacking { 
        get {
            return timeLastAttacking;
        }
    }

    [SerializeField] private bool attacking;
    public bool Attacking {
        get {
            if (!attackingUpdatedThisFrame) {
                if (animatorReference.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) {
                    attacking = true;
                    newWalking = false;

                    if (playerCurrentAttack == PlayerAttack.none) Debug.LogWarning("Player is attacking with no known attack");
                }
                else {
                    attacking = false;
                    // Hopefully obsolete, but a bit cleaner and more readable
                    playerCurrentAttack = PlayerAttack.none;
                }
                attackingUpdatedThisFrame = true;
            }
            return attacking;
        }
    }
    private bool attackingUpdatedThisFrame = false;
    public bool hasUsedUpHeavy = false;

    /*Blocking*/
    private bool blocking = false;
    public bool Blocking {
        get {
            return blocking;
        }
    }
    [SerializeField] private float blockingMaxLength; // The maximum time the player can block for
    [SerializeField] private float blockingFailedCooldown; // The cooldown between unsuccessful blocks
    [SerializeField] private float blockingSucceededCooldown; // The cooldown between successful blocks
    private float timeLastBlockEnded = -1000000000000000; // The time (Time.time) at which the player last stopped blocking
    private float timeLastBlockStarted = 0;
    public bool lastBlockSuccessful = false; // If the last time the player blocked they blocked one or more attacks (to make the cooldown longer)

    void Awake() {
        rigidbodyReference = GetComponent<Rigidbody2D>();
        animatorReference = GetComponentInChildren<Animator>();
        boxColliderReference = GetComponentInChildren<BoxCollider2D>();
        playerTransform = transform.GetChild(0);

        defaultGravityScale = rigidbodyReference.gravityScale;

        playerCurrentState = PlayerState.onGround;
        playerCurrentAttack = PlayerAttack.none;

        if (playerDirection == Direction.right) {
            playerTransform.eulerAngles = Vector3.zero;
        }
        else if (playerDirection == Direction.left) {
            playerTransform.eulerAngles = new Vector3(0, -180, 0);
        }

        Application.targetFrameRate = 144;
    }

    // Update is called once per frame
    void Update() {
        OnGround(); // Check if the player is on the ground or in the air
        if (!freezeMovement) // Prevent the player from moving at the beginning of the match
        {  
            OnWall(); // Check if the player is climbing a wall (assuming they aren't on the ground);
            Jump(); // Make the player jump if they can and have pressed the key
            Move(); // Move the player if they can and are trying to move
            ChangeDirection(); // Swap the player's direction if they are now moving in a different direction
            Block(); // Make the player block if they are not on cooldown and are pressing the correct key. Otherwise stop them from blocking
            Attack(); // Check if the player can attack and if they are doing a combo attack and begin that attack
        }
        ResetVariables(); // Allows the variables which should only update once per frame to be updated again
    }

    // OnGround is called on Update in order to determine whether or not the player is on the ground
    private void OnGround() {
        // If there is nothing below the player
        if (Physics2D.BoxCast(boxColliderReference.bounds.center, boxColliderReference.bounds.size, 0, Vector2.down, minimumDistanceFromGround, groundLayer).collider == null) {
            // If the player is not already in the air, make them in the air and reset their gravity scale
            if (playerCurrentState != PlayerState.inAir) {
                rigidbodyReference.gravityScale = defaultGravityScale;
                playerCurrentState = PlayerState.inAir;
            }
        }
        // If there is something right below the player and they are not already known to be on the ground
        else if (playerCurrentState != PlayerState.onGround) {
            playerCurrentState = PlayerState.onGround;

            // Allow the player to use their air moves again, including double jumping and an up air heavy
            hasDoubleJumped = false;
            hasUsedUpHeavy = false;
        }
    }

    // Check if the player is next to a wall
    private void OnWall() {
        // Check that the player isn't on the ground and just ran into a wall next to them
        if (playerCurrentState == PlayerState.onGround) return;

        // If the player is facing to left, make their direction the left, otherwise make their direction the right
        Vector2 directionPlayerFacing = Vector2.right;
        if (PlayerDirection == Direction.left) directionPlayerFacing = Vector2.left;

        // Check if the player is right next to a wall they are facing (climbing)
        if (Physics2D.BoxCast(boxColliderReference.bounds.center, boxColliderReference.bounds.size, 0, directionPlayerFacing, minimumDistanceFromGround, groundLayer).collider != null) {
            // If the player was in the air and is now climbing a wall, allow them to use their air moves again
            if (PlayerCurrentState == PlayerState.inAir) {
                hasDoubleJumped = false;
                hasUsedUpHeavy = false;
            }
            
            // Turn off the walking animation while climbing
            newWalking = false;

            // Make the player slide down slower, but not jump up higher, by checking that they are travelling downwards
            if (rigidbodyReference.velocity.y <= 0 && atDefaultGravityScale) {
                rigidbodyReference.gravityScale = 0;
                atDefaultGravityScale = false;
                rigidbodyReference.velocity = new Vector2(rigidbodyReference.velocity.x, -slidingSpeed);
            }
            else if (!atDefaultGravityScale) {
                rigidbodyReference.gravityScale = defaultGravityScale;
                atDefaultGravityScale = true;
            }

            // If the player isn't already climbing, make them fall slower and make them climb
            if (playerCurrentState != PlayerState.climbing) {
                playerCurrentState = PlayerState.climbing;
            }
        }
        else if (!atDefaultGravityScale) {
            rigidbodyReference.gravityScale = defaultGravityScale;
            atDefaultGravityScale = true;
        }
    }


    // Jump is called on Update and launches the player upwards if they are on solid ground and have not jumped recently
    private void Jump() {
        // Check that the player is not within the jump cooldown
        if (Time.time < (timeLastJumped + jumpCooldown)) return;

        // Check that at least one physics frame has passed since the player jumped
        if (Time.time - timeLastJumped < Time.fixedDeltaTime) return;

        // Check that the player is pressing the jump key
        if (VerticalInput <= 0) return;

        // If the player is on the ground air then do a regular jump
        if (playerCurrentState == PlayerState.onGround) {
            // Reset the player's Y velocity so that they can jump the correct height
            rigidbodyReference.velocity = rigidbodyReference.velocity.x * Vector2.right;

            // Make the player jump
            rigidbodyReference.AddForce(jumpVelocity * Vector2.up, ForceMode2D.Impulse);

            // Reset the jump cooldown
            timeLastJumped = Time.time;
        }
        // If the player is in the air then double jump
        else if (playerCurrentState == PlayerState.inAir) {
            // Check that they have not already used their double jump
            if (!hasDoubleJumped) {
                // Reset the player's Y velocity so that they can jump the correct height (whilst keeping the x velocity)
                rigidbodyReference.velocity = rigidbodyReference.velocity.x * Vector2.right;


                // Make the player jump
                rigidbodyReference.AddForce(jumpVelocity * Vector2.up , ForceMode2D.Impulse);

                // Reset the jump cooldown
                timeLastJumped = Time.time;

                // Ensure that the player cannot double jump again until the touch the ground
                hasDoubleJumped = true;
            }
        }
        // If they are climbing then jump outwards as well as updards. (Note: there are only three player states so this is effectively an else statement since hasdoublejumped is nested)
        else if (playerCurrentState == PlayerState.climbing) {
            // Reset the player's velocity (X and Y) so that they can flip correctly
            rigidbodyReference.velocity = Vector2.zero;
            // Reset the player's gravity so that they fall correctly
            rigidbodyReference.gravityScale = defaultGravityScale;

            // Make the player jump
            rigidbodyReference.AddForce(climbVelocity * Vector2.up, ForceMode2D.Impulse);

            // Make the player jump outwards from a wall on their right
            if (PlayerDirection == Direction.right) {
                rigidbodyReference.AddForce(flipVelocity * Vector2.left, ForceMode2D.Impulse);
                //directionForceRecentlyApplied = DirectionForceApplied.left;
            }
            // Make the player jump outwards from a wall on their left
            else {
                rigidbodyReference.AddForce(flipVelocity * Vector2.right, ForceMode2D.Impulse);
                //directionForceRecentlyApplied = DirectionForceApplied.right;
            }

            // Reset the jump cooldown
            timeLastJumped = Time.time;
            timeLastWallJumped = Time.time;
        }
    }

    private void Move() {
        // If the player is climbing, prevent them from moving towards the wall they are climbing so that they can't stop themselves from sliding
        if (playerCurrentState == PlayerState.climbing) {
            if (WalkSpeed > 0 && PlayerDirection == Direction.right) return;
            if (WalkSpeed < 0 && PlayerDirection == Direction.left) return;
        }

        // If the player just wall jumped, prevent them from destroying their velocity
        if (timeLastWallJumped + wallJumpControlFreeze > Time.time) return;

        // If the player just got knocked back then make sure that they can't move before their acceleration has been applied
        if (timeLastKnockedBack + knockbackMovementCooldown > Time.time) return;

        // If the player has reached zero speed since they were knocked back then allow them to instantly change their speed
        if (directionForceRecentlyApplied == DirectionForceApplied.right && rigidbodyReference.velocity.x <= 0) {
            directionForceRecentlyApplied = DirectionForceApplied.none;
        }
        else if (directionForceRecentlyApplied == DirectionForceApplied.left && rigidbodyReference.velocity.x >= 0) {
            directionForceRecentlyApplied = DirectionForceApplied.none;
        }

        // If the player is pressing right
        if (HorizontalInput > 0) {
            // Check that the player is moving slower than their maximum rightwards speed since their speed should not increase further than that
            if (rigidbodyReference.velocity.x < WalkSpeed) {
                // If the player is going slower than their minimum speed or if they have been knocked back recently then steadily bring them up to their speed
                if (rigidbodyReference.velocity.x < -WalkSpeed || directionForceRecentlyApplied == DirectionForceApplied.left) {
                    if (takeKnockbackOnGround || !takeKnockbackOnGround && playerCurrentState == PlayerState.inAir) {
                        rigidbodyReference.velocity = new Vector2(rigidbodyReference.velocity.x + walkSpeed * Time.deltaTime, rigidbodyReference.velocity.y);
                    }
                    else rigidbodyReference.velocity = new(walkSpeed, rigidbodyReference.velocity.y);
                }
                // Otherwise set their speed to the speed they are trying to travel at
                else rigidbodyReference.velocity = new(walkSpeed, rigidbodyReference.velocity.y);

                // If the player is trying to move right and they are going slower than the maximum rightwards speed then disable the knockback effect
                if (directionForceRecentlyApplied == DirectionForceApplied.right) {
                    directionForceRecentlyApplied = DirectionForceApplied.none;
                }
            }
        }
        // If the player is pressing left
        else if (HorizontalInput < 0) {
            // If the player is going faster than their minimum speed
            if (rigidbodyReference.velocity.x > -WalkSpeed) {
                // If the player is going faster than their maximum leftwards speed or if they have been knocked back recently then steadily bring them down to their maximum speed
                if (rigidbodyReference.velocity.x > WalkSpeed || directionForceRecentlyApplied == DirectionForceApplied.right) {
                    if (takeKnockbackOnGround || !takeKnockbackOnGround && playerCurrentState == PlayerState.inAir) {
                        rigidbodyReference.velocity = new Vector2(rigidbodyReference.velocity.x - walkSpeed * Time.deltaTime, rigidbodyReference.velocity.y);
                    }
                    else rigidbodyReference.velocity = new(-walkSpeed, rigidbodyReference.velocity.y);
                }
                // Otherwise set their speed to the speed they are trying to travel at
                else rigidbodyReference.velocity = new(-walkSpeed, rigidbodyReference.velocity.y);

                // If the player is trying to move left and they are going slower than the maximum leftwards speed then disable the knockback effect
                if (directionForceRecentlyApplied == DirectionForceApplied.left) {
                    directionForceRecentlyApplied = DirectionForceApplied.none;
                }
            }
        }
        // If the player is not trying to move in any direction
        else {
            // If the player is travelling right, slow them down
            if (rigidbodyReference.velocity.x > WalkSpeed) {
                if (takeKnockbackOnGround || !takeKnockbackOnGround && playerCurrentState == PlayerState.inAir) {
                    rigidbodyReference.velocity = new Vector2(rigidbodyReference.velocity.x - walkSpeed * Time.deltaTime, rigidbodyReference.velocity.y);
                }
                else rigidbodyReference.velocity = new Vector2(0, rigidbodyReference.velocity.y);
            }
            // If the player is travelling left, slow them down
            else if (rigidbodyReference.velocity.x < -WalkSpeed) {
                if (takeKnockbackOnGround || !takeKnockbackOnGround && playerCurrentState == PlayerState.inAir) {
                    rigidbodyReference.velocity = new Vector2(rigidbodyReference.velocity.x - -walkSpeed * Time.deltaTime, rigidbodyReference.velocity.y);
                }
                else rigidbodyReference.velocity = new Vector2(0, rigidbodyReference.velocity.y);
            }
            // if the player is travelling within the acceptable range then immediately set their velocity to zero
            else {
                rigidbodyReference.velocity = new Vector2(0, rigidbodyReference.velocity.y);
                newWalking = false;
            }
        }
    }

    private void ChangeDirection() {
        // If the player is trying to move in the opposite direction they are facing and they are not currently attacking or climbing
        if (HorizontalInput > 0 && playerDirection == Direction.left && CanChangeDirection()) {

            // Update the current facing direction with a two-value enum
            playerDirection = Direction.right;

            // Make the player face right, which is the default direction
            playerTransform.eulerAngles = Vector3.zero; 
        }
        // If the player is trying to move in the opposite direction they are facing and they are not currently attacking or climbing
        else if (HorizontalInput < 0 && playerDirection == Direction.right && CanChangeDirection()) {

            // Update the current facing direction with a two-value enum
            playerDirection = Direction.left;

            // Make the player face left, which is flipped from the default
            playerTransform.eulerAngles = new Vector3(0, -180, 0);
        }
    }
    private void Block() {
        if (blocking) {
            // If the player is attacking or doing something else that should prevent them from blocking, stop blocking
            if (!CanChangeDirection()) blocking = false;
            // If the player has been blocking for too long
            if (timeLastBlockStarted + blockingMaxLength < Time.time) blocking = false; 
            // If the player's last block did not block anything then check that the normal cooldown has occured before allowing them to block again
            if (timeLastBlockEnded + blockingFailedCooldown > Time.time) blocking = false;
            // If the player's last block successfully blocked something then check that the longer cooldown has occured before allowing them to block again
            if (lastBlockSuccessful && timeLastBlockEnded + blockingSucceededCooldown > Time.time) blocking = false;
            // If the player isn't pressing the block key, stop making them block
            if (!BlockingPressed) blocking = false;

            if (!blocking) {
                animatorReference.SetBool("blocking", false);
                timeLastBlockEnded = Time.time;
            }
        }
        else {
            // If the player is attacking or doing something else that should prevent them from blocking, don't let them block
            if (!CanChangeDirection()) return;
            // If the player is still on cooldown since the last time they blocked, don't let them block
            if (timeLastBlockEnded + blockingFailedCooldown > Time.time) return;
            if (lastBlockSuccessful && timeLastBlockEnded + blockingSucceededCooldown > Time.time) return;
            // If the player is not pressing the block key, don't make the player block
            if (!BlockingPressed) return;

            blocking = true;
            newWalking = false;
            lastBlockSuccessful = false;
            animatorReference.SetBool("blocking", true);
            timeLastBlockStarted = Time.time;
        }
    }

    private void Attack() {
        if (Attacking || blocking) return;

        if  (LightAttackPressed) {
            // Down attack
            if (VerticalInput < 0) {
                StartAttack(PlayerAttack.downLight);
            }
            // Side attack
            else if (HorizontalInput != 0) {
                StartAttack(PlayerAttack.sideLight);
            }
            // Neutral attack
            else {
                StartAttack(PlayerAttack.neutralLight);
            }
        }
        else if (HeavyAttackPressed) {
            // Up or down attack
            if (VerticalInput != 0) {
                // Up attack
                if (VerticalInput > 0) {
                    if (!hasUsedUpHeavy) {
                        StartAttack(PlayerAttack.upHeavy);

                        // Reset the player's Y velocity so that they can jump the correct height
                        if (rigidbodyReference.velocity.y < 0) {
                            rigidbodyReference.velocity = rigidbodyReference.velocity.x * Vector2.right;
                        }

                        // Prevent the player from using this move again until they touch the ground
                        hasUsedUpHeavy = true;
                    }
                    else {
                        // If the player does not have an up air heavy remaining, use the alternate up heavy
                        StartAttack(PlayerAttack.alternateUpHeavy);
                    }
                }
                // Down attack
                else {
                    if (PlayerCurrentState == PlayerState.onGround) {
                        StartAttack(PlayerAttack.downGroundHeavy);
                    }
                    else if (PlayerCurrentState == PlayerState.inAir) {
                        StartAttack(PlayerAttack.downAirHeavy);
                    }
                }
            }
            // Side attack
            else if (HorizontalInput != 0) {
                if (PlayerCurrentState == PlayerState.onGround) {
                    StartAttack(PlayerAttack.sideGroundHeavy);
                }
                else if (PlayerCurrentState == PlayerState.inAir) {
                    StartAttack(PlayerAttack.sideAirHeavy);
                }
            }
            // Neutral attack
            else {
                StartAttack(PlayerAttack.neutralHeavy);
            }
        }
    }

    private void StartAttack(PlayerAttack newAttack) {
        // Turn off the walking animation
        newWalking = false;

        // Set the current attack so that damage and knockback can be calculated and the AI knows which attack is being run
        playerCurrentAttack = newAttack;

        // Start the animation. Once the animation is done, it will go back to idle
        animatorReference.SetTrigger(newAttack.ToString());

        // Set the time at which the last attack was done to the current time. This value is used to ensure each attack only deals damage once
        timeLastAttacking = Time.time;
    }


    private void ResetVariables() {
        horizontalInputUpdated = false;
        verticalInputUpdated = false;
        lightAttackPressedUpdated = false;
        heavyAttackUpdated = false;
        attackingUpdatedThisFrame = false;
        blockingPressedUpdated = false;
        abilityPressedUpdated = false;

        // Turn the walking animation on or off as appropriate
        if (newWalking != walking) {
            walking = newWalking;
            animatorReference.SetBool("walking", walking);

        }
        // Make new walking working again next frame (it turns on unless there is a reason to turn it off)
        newWalking = true;
    }

    private bool CanChangeDirection() {
        return !Attacking;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        string collisionTag = collision.tag;

        if (collisionTag == "Weapon") {
            Transform attackerTransform = collision.transform.parent.parent; // Get the parent (the character) of the the parent of the weapon (the character sprite)
            // In worse English, it's the parent of the weapon, which is the sprite, and then the parent of that, which is what actually has the script


            // Try to get an AI script from the enemy which damaged the player
            if (attackerTransform.TryGetComponent<AI>(out AI AIScript)) {
                // If the player is blocking, don't take damage and make the cooldown until the next block a bit longer
                if (Blocking) {
                    lastBlockSuccessful = true;
                }
                // Make sure that the player is not being hit by the same attack twice (preventing double damamge)
                else if (timeLastDamaged < AIScript.TimeLastAttacking) {
                    // Get the attack the AI is performing
                    AIAttack attack = AIScript.AILastAttack;

                    // Get the appropriate amount of damamge for that attack
                    if (ConstantAIValues.attackDamages.TryGetValue(attack, out float damage)) {
                        DamageTaken += damage;
                    }

                    // Get the appropriate amount of knockback for that attack
                    if (ConstantAIValues.attackKnockbacks.TryGetValue(attack, out float knockback)) {
                        // Make the player take (exponentially) more knockback the more damamge they have taken (to a limit of 3 times (with a limit of 10 damage taken))
                        knockback *= Mathf.Pow(maximumKnockbackMultiplier, DamageTaken / maximumDamage);

                        // If the AI is facing the right
                        if (attackerTransform.GetChild(0).eulerAngles.y == 0) {
                            rigidbodyReference.AddForce(knockback * new Vector2(1, verticalToHorizontalKnockback), ForceMode2D.Impulse);
                            directionForceRecentlyApplied = DirectionForceApplied.left;
                            Debug.Log("Knocked back left");
                        }
                        // If the player is facing the left
                        else {
                            rigidbodyReference.AddForce(knockback * new Vector2(-1, verticalToHorizontalKnockback), ForceMode2D.Impulse);
                            directionForceRecentlyApplied = DirectionForceApplied.right;
                            Debug.Log("Knocked back right");
                        }

                        // Briefly prevent the player from moving so that the acceleration from the knockback is applied
                        if (knockback > 0) {
                            timeLastKnockedBack = Time.time;
                        }
                    }

                    // Make sure that the player cannot be hit by this same attack again (prevent weird hitbox issues)
                    timeLastDamaged = AIScript.TimeLastAttacking;
                }
            }
            // Try to get a player script from the enemy which damamged the player (and make sure that it isn't itself)
            else if (attackerTransform.TryGetComponent<Player>(out Player playerScript) && playerScript != this) {
                // If the player is blocking, don't take damage and make the cooldown until the next block a bit longer
                if (Blocking) {
                    lastBlockSuccessful = true;
                }
                // Make sure that the player is not being hit by the same attack twice (preventing double damamge)
                else if (timeLastDamaged < playerScript.TimeLastAttacking) {
                    // Get the current attack
                    PlayerAttack attack = playerScript.PlayerCurrentAttack;

                    // Log if the player that is meant to damage this player is not attacking. This shouldn't cause any issues but it would be indicative of an issue elsewhere
                    if (attack == PlayerAttack.none) Debug.LogWarning("Collided with weapon while no attack was being performed");

                    // Get the appropriate amount of damage for the attack from a dictionary and make the player take that much damamge
                    if (ConstantPlayerValues.attackDamages.TryGetValue(attack, out float damage)) {
                        DamageTaken += damage;
                    }

                    // Get the appropriate amount of knockback for the attack from a dictionary and knock them back that far
                    if (ConstantPlayerValues.attackKnockbacks.TryGetValue(attack, out float knockback)) {
                        // Make the player take (exponentially) more knockback the more damamge they have taken (to a limit of 3 times (with a limit of 10 damage taken))
                        knockback *= Mathf.Pow(maximumKnockbackMultiplier, DamageTaken / maximumDamage);

                        if (playerScript.PlayerDirection == Direction.right) {
                            rigidbodyReference.AddForce(knockback * new Vector2(1, verticalToHorizontalKnockback), ForceMode2D.Impulse);
                            directionForceRecentlyApplied = DirectionForceApplied.left;
                        }
                        else {
                            rigidbodyReference.AddForce(knockback * new Vector2(-1, verticalToHorizontalKnockback), ForceMode2D.Impulse);
                            directionForceRecentlyApplied = DirectionForceApplied.right;
                        }

                        // Briefly prevent the player from moving so that the acceleration from the knockback is applied
                        if (knockback > 0) {
                            timeLastKnockedBack = Time.time;
                        }
                    }

                    // Make sure that the player cannot be hit by this same attack again (prevent weird hitbox issues)
                    timeLastDamaged = playerScript.TimeLastAttacking;
                }
            }
        }
        else if (collisionTag == "Safe") {
            playerSector = Sector.safe;
        }
        else if (collisionTag == "Alarm")
        {
            playerSector = Sector.alarm;
        }
        else if (collisionTag == "Warning") {
            playerSector = Sector.warning;
        }
        else if (collisionTag == "Danger") {
            playerSector = Sector.danger;
        }
    }
}