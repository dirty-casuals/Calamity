using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

public class LobbyPlayer : NetworkLobbyPlayer {

    public Button readyButton;
    public Button waitingPlayerButton;
    public Button startButton;
    public static int numPlayers = 0;

    public override void OnClientEnterLobby( ) {
        base.OnClientEnterLobby( );
        numPlayers = numPlayers + 1;
        LobbyPlayerList._instance.AddPlayer( this );
        LobbyPlayerList._instance.DisplayDirectServerWarning( isServer && LobbyManager.s_Singleton.matchMaker == null );
        StartCoroutine( SetupLobby( ) );
    }

    private IEnumerator SetupLobby( ) {
        yield return new WaitForEndOfFrame( );
        if (isLocalPlayer) {
            SetupLocalPlayer( );
        } else {
            SetupOtherPlayer( );
        }
    }

    private void Update( ) {
        //if (SceneManager.GetActiveScene( ).name != LobbyManager.s_Singleton.lobbyScene) {
        //    return;
        //}

        //InputField obj = (EventSystem.current.currentSelectedGameObject != null) ? EventSystem.current.currentSelectedGameObject.GetComponent<InputField>( ) : null;

        //if (isLocalPlayer && (obj == null || !obj.isFocused)) {
        //    int localIdx = playerControllerId + 1;
        //    if (!readyToBegin && Input.GetButtonDown( "Fire" + localIdx )) {
        //        if (readyButton.IsActive( ) && readyButton.IsInteractable( )) {
        //            ActivateEyes( );
        //        }
        //        SendReadyToBeginMessage( );
        //    }
        //}
    }

    private void ActivateEyes( ) {
        LobbyManager.s_Singleton.ActivateEyes( );
    }

    private void SetupLocalPlayer( ) {
        readyButton.transform.GetChild( 0 ).GetComponent<Text>( ).text = "JOIN";
        readyButton.interactable = true;

        readyButton.onClick.RemoveAllListeners( );
        readyButton.onClick.AddListener( delegate { OnReadyClicked( ); } );

        startButton.onClick.RemoveAllListeners( );
        startButton.onClick.AddListener( LobbyManager.s_Singleton.OnStartClicked );
    }

    private void SetupOtherPlayer( ) {
        readyButton.interactable = false;
        readyButton.gameObject.SetActive( false );
        startButton.interactable = false;
        startButton.gameObject.SetActive( false );
        OnClientReady( false );
    }

    public override void OnClientReady( bool readyState ) {
        if (readyState) {
            Text textComponent = readyButton.transform.GetChild( 0 ).GetComponent<Text>( );
            textComponent.text = "READY";
            // Activate already called on server player
            if (!isLocalPlayer) {
                ActivateEyes( );
            }
            readyButton.interactable = false;
        } else {
            Text textComponent = readyButton.transform.GetChild( 0 ).GetComponent<Text>( );
            textComponent.text = isLocalPlayer ? "JOIN" : "...";
            readyButton.interactable = isLocalPlayer;
        }
    }

    public void OnReadyClicked( ) {
        ActivateEyes( );
        SendReadyToBeginMessage( );
    }

    public void RpcToggleJoinButton( bool enabled ) {
        readyButton.gameObject.SetActive( enabled );
        waitingPlayerButton.gameObject.SetActive( !enabled );
    }

    public void ToggleStartButton( bool enabled ) {
        startButton.gameObject.SetActive( enabled );
        startButton.interactable = enabled;
    }

    [ClientRpc]
    public void RpcUpdateCountdown( int countdown ) {
        LobbyManager.s_Singleton.infoPanel.Display( "Match Starting in " + countdown, "", null, false );

        if (countdown == 0) {
            LobbyManager.s_Singleton.infoPanel.gameObject.SetActive( false );
        }
    }
}