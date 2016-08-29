using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public abstract class ComponentEnabler : NetworkBehaviour {

    protected string characterType;

    protected Type[ ] monoBehavioursToEnable;
    protected Type[ ] behavioursToEnable;
    protected Type[ ] collidersToEnable;

    protected void EnableBehaviour( Type componentType ) {
        Behaviour behaviour = GetComponentInChildren( componentType, true ) as Behaviour;

        if (behaviour == null) {
            Debug.Log( componentType.ToString( ) + " not present on player" );
        } else {
            behaviour.enabled = true;
        }
    }


    protected void EnableBehaviours( Type[ ] componentTypes ) {
        for (int i = 0; i < componentTypes.Length; i += 1) {
            Type componentType = componentTypes[ i ];
            EnableBehaviour( componentType );
        }
    }


    protected void SetupComponents( ) {
        SetupTypeLists( );

        EnableBehaviours( monoBehavioursToEnable );
        EnableBehaviours( behavioursToEnable );

        // Can't make a fully generic method for enabling things as Component lacks enable field
        for (int i = 0; i < collidersToEnable.Length; i += 1) {
            Type componentType = collidersToEnable[ i ];
            Collider behaviour = GetComponentInChildren( componentType, true ) as Collider;

            if (behaviour == null) {
                Debug.Log( componentType.ToString( ) + " not present on " + characterType );
            } else {
                behaviour.enabled = true;
            }
        }
    }

    protected abstract void SetupTypeLists( );
}
