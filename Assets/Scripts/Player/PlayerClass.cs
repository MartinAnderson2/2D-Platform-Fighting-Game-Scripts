using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerClass {
    public GameObject player;
    public bool isInAir;
    public bool isWalking;
    public Player playerMovement;

    public bool isStunned;
    public Animator anim;
    public Rigidbody2D rigidbody;


    public PlayerClass(GameObject _player, bool _isInAir, Animator _anim, Rigidbody2D _rigidbody, bool _isStunned, bool _isWalking, Player playerMovement) {
        player = _player;
        isInAir = _isInAir;
        anim = _anim;
        rigidbody = _rigidbody;
        isStunned = _isStunned;
        isWalking = _isWalking;
        this.playerMovement = playerMovement;
    }

}

// A class which stores the various constants that the player uses
public static class ConstantPlayerValues {
    // The amount of damage each attack deals
    public static Dictionary<PlayerAttack, float> attackDamages = new() {
        { PlayerAttack.none, 0},
        { PlayerAttack.neutralLight, 0.25f},
        { PlayerAttack.sideLight, 0.5f},
        { PlayerAttack.downLight, 0.5f},
        { PlayerAttack.neutralHeavy, 0.5f},
        { PlayerAttack.upHeavy, 1},
        { PlayerAttack.alternateUpHeavy, 1},
        { PlayerAttack.sideGroundHeavy, 2},
        { PlayerAttack.sideAirHeavy, 1.5f},
        { PlayerAttack.downGroundHeavy, 1.5f},
        { PlayerAttack.downAirHeavy, 2.5f}
    };

    // The amount of knockbach each attack applies
    public static Dictionary<PlayerAttack, float> attackKnockbacks = new() {
        { PlayerAttack.none, 0},
        { PlayerAttack.neutralLight, 0.5f},
        { PlayerAttack.sideLight, 1f},
        { PlayerAttack.downLight, 0.5f},
        { PlayerAttack.neutralHeavy, 2f},
        { PlayerAttack.upHeavy, 1f},
        { PlayerAttack.alternateUpHeavy, 1f},
        { PlayerAttack.sideGroundHeavy, 3f},
        { PlayerAttack.sideAirHeavy, 3f},
        { PlayerAttack.downGroundHeavy, 1f},
        { PlayerAttack.downAirHeavy, 2f}
    };

    // A dictionary to convert strings into the two different player setups
    public static Dictionary<string, PlayerSetup> playerSetups = new() {
        { "playerVsPlayer", PlayerSetup.playerVsPlayer },
        { "playerVsAI", PlayerSetup.playerVsAI }
    };

    // A dictionary to convert strings into the three characters one can play
    public static Dictionary<string, Characters> characterTypes = new() {
        {"Knight", Characters.knight },
        {"Scientist", Characters.scientist },
        {"Astronaut", Characters.astronaut }
    };

    // A dictionary to convert strings into four control types for two players (or more)
    public static Dictionary<string, MultiplayerControl> multiplayerControls = new() {
        {"leftSideKeyboard", MultiplayerControl.WASD },
        {"rightSideKeyboard", MultiplayerControl.arrowKeys },
        {"controller", MultiplayerControl.controller }
    };
}

// The two directions players can be facing
public enum Direction {
    right,
    left
}

// All of the player's possible states
public enum PlayerState {
    onGround,
    climbing,
    inAir
}

// All of the attacks the player could be performing
public enum PlayerAttack {
    none,
    neutralLight,
    sideLight,
    downLight,

    neutralHeavy,
    upHeavy,
    alternateUpHeavy,

    sideGroundHeavy,
    sideAirHeavy,

    downGroundHeavy,
    downAirHeavy,
}

// The three different sets of characters (each of which has their own animations) that the player can choose
public enum Characters {
    knight,
    scientist,
    astronaut
}

// The 'mode' of the game. Whether there are two players or a player and an AI
public enum PlayerSetup {
    playerVsPlayer,
    playerVsAI
}

public enum MultiplayerControl {
    WASD,
    arrowKeys,
    controller
}

public enum WhichPlayer {
    none,
    playerOne,
    playerTwo
}