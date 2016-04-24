using UnityEngine;
using System.Collections;

public class SingleSpawner : Spawner {

    private GameObject character = null;

    public override void StartSpawn( ) {
        if (character != null) {
            ReenableCurrentCharacter( );
            return;
        }
        CreateMonster( );
    }

    public override void UpdateChildrenToSpawnPosition( ) {
        character.gameObject.transform.position = gameObject.transform.position;
    }

    protected override void ToggleCharacterState( bool currentState ) {
        if (currentState) {
            character.SetActive( true );
            gameObject.GetComponentInChildren<CharacterStateHandler>( ).currentState.RevivePlayer( );
        } else {
            character.SetActive( false );
        }
    }

    private void CreateMonster( ) {
        GameObject spawn = CreateAndGetCharacter( );
        character = spawn;
    }
}
