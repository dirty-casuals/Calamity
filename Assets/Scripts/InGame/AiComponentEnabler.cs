using UnityEngine;
using System.Collections;
using RAIN.Core;
using RAIN.Entities;
using System;

public class AiComponentEnabler : ComponentEnabler {

    public override void OnStartServer( ) {
        characterType = "AI";
        SetupComponents( );
    }

    protected override void SetupTypeLists( ) {
        monoBehavioursToEnable = new Type[ ]
        {
            typeof( AnimTriggers ),
            typeof( CharacterStateHandler ),
            typeof( PlayerController ),
            typeof( PlayerInventory ),
            typeof( EntityRig ),
            typeof( AIRig )
        };

        behavioursToEnable = new Type[ ]
        {
            typeof( Animator )
        };

        collidersToEnable = new Type[ ]
        {
            typeof( CapsuleCollider )
        };
    }
}
