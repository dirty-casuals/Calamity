using UnityEngine;
using System.Collections;

public class SingleSpawner : Spawner {

    private GameObject character = null;

    public override void StartSpawn( ) {
        if ( character != null ) {
            ReenableCurrentCharacter( );
            return;
        }
        CreateMonster( );
    }

    protected override void ToggleCharacterState( bool currentState ) {
        character.SetActive( currentState );
    }

    private void CreateMonster( ) {
        GameObject spawn = CreateAndGetCharacter( );
        character = spawn;
    }
}
