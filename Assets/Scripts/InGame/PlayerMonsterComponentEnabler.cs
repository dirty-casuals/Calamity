using UnityEngine;
using System.Collections;
using System;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.ImageEffects;
using RAIN.Entities;

public class PlayerMonsterComponentEnabler : ComponentEnabler {

    public override void OnStartLocalPlayer( ) {
        characterType = "Monster";
        SetupComponents( );
    }

    protected override void SetupTypeLists( ) {
        monoBehavioursToEnable = new Type[ ]
        {
            typeof( AnimTriggers ),
            typeof( CharacterStateHandler ),
            typeof( PlayerController ),
            typeof( CalamityFirstPersonController ),
            typeof( BloomOptimized ),
            typeof( CalamityFirstPersonCamera ),
            typeof( EntityRig )
        };

        behavioursToEnable = new Type[ ]
        {
            typeof( Animator ),
            typeof( Camera )
        };

        collidersToEnable = new Type[ ]
        {
            typeof( CharacterController )
        };
    }
}
