using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GamePlayerPlaceholder : NetworkBehaviour {

    public override void OnStartLocalPlayer( ) {
        StartOnGameHandlerReady( );
    }

    private void StartOnGameHandlerReady( ) {
        CmdRequestPlayerSpawn( GetComponent<NetworkIdentity>( ).netId );
    }

    [Command]
    private void CmdRequestPlayerSpawn( NetworkInstanceId netId ) {
        StartCoroutine( RequestOnGameHandlerReady( netId ) );
    }

    private IEnumerator RequestOnGameHandlerReady( NetworkInstanceId netId ) {
        GameObject gameHandlerObject = GameObject.FindGameObjectWithTag( "GameHandler" );
        while (gameHandlerObject == null) {
            yield return new WaitForEndOfFrame( );
            gameHandlerObject = GameObject.FindGameObjectWithTag( "GameHandler" );
        }

        GameHandler gameHandler = gameHandlerObject.GetComponent<GameHandler>( );
        while (!gameHandler.isActiveAndEnabled || !gameHandler.isReadyForPlayerSpawns) {
            yield return new WaitForEndOfFrame( );
        }

        gameHandler.RequestPlayerSpawn( netId );
    }
}
