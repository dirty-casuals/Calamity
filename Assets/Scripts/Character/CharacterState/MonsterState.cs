using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class MonsterState : PlayerState {
    
    private CalamityFirstPersonController firstPersonController;
    private bool playerControllerDisabled;

    public MonsterState( GameObject playerBody ) : base( playerBody ) {
        movementSpeed = 7.0f;
        character.tag = "Monster";
    }
}