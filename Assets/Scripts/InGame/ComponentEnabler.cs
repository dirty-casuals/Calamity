using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public abstract class ComponentEnabler : NetworkBehaviour {

    protected string characterType;

    protected Type[ ] monoBehavioursToEnable;

    protected Type[ ] behavioursToEnable;

    protected Type[ ] collidersToEnable;
   
    protected void SetupComponents( ) {
        SetupTypeLists( );

        for (int i = 0; i < monoBehavioursToEnable.Length; i += 1) {
            Type componentType = monoBehavioursToEnable[ i ];
            MonoBehaviour monoBehaviour = GetComponentInChildren( componentType, true ) as MonoBehaviour;

            if (monoBehaviour == null) {
                Debug.Log( componentType.ToString( ) + " not present on player" );
            } else {
                monoBehaviour.enabled = true;
            }
        }

        for (int i = 0; i < behavioursToEnable.Length; i += 1) {
            Type componentType = behavioursToEnable[ i ];
            Behaviour behaviour = GetComponentInChildren( componentType, true ) as Behaviour;

            if (behaviour == null) {
                Debug.Log( componentType.ToString( ) + " not present on " + characterType );
            } else {
                behaviour.enabled = true;
            }
        }

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
