using UnityEngine;
using System.Collections;
using RAIN.Core;
using RAIN.Entities;
using System;

public class AiMonsterComponentEnabler : ComponentEnabler {

    public override void OnStartServer( ) {
        characterType = "AIMonster";
        SetupComponents( );
    }

    protected override void SetupTypeLists( ) {
        monoBehavioursToEnable = new Type[ ]
        {
            typeof( AnimTriggers ),
            typeof( CharacterStateHandler ),
        };

        behavioursToEnable = new Type[ ]
        {
        };

        collidersToEnable = new Type[ ]
        {
            typeof( CapsuleCollider )
        };
    }
}
