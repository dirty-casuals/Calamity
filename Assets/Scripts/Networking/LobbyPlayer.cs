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
    public List<GameObject> eyes = new List<GameObject>( );
    public static int numPlayers = 0;

    public Color playerColor = Color.white;

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
        if (SceneManager.GetActiveScene( ).name != LobbyManager.s_Singleton.lobbyScene)
            return;

        InputField obj = (EventSystem.current.currentSelectedGameObject != null) ? EventSystem.current.currentSelectedGameObject.GetComponent<InputField>( ) : null;

        if (isLocalPlayer && (obj == null || !obj.isFocused)) {
            int localIdx = playerControllerId + 1;
            if (!readyToBegin && Input.GetButtonDown( "Fire" + localIdx )) {
                if (readyButton.IsActive( ) && readyButton.IsInteractable( )) {
                    ActivateEyes( );
                }
                SendReadyToBeginMessage( );
            }
        }
    }

    private void ActivateEyes( ) {
        NetworkInstanceId id = netId;
        if (id.ToString( ) == "1") {
            eyes[ 0 ].SetActive( true );
        }
        if (id.ToString( ) == "2") {
            eyes[ 1 ].SetActive( true );
        }
    }

    private void SetupLocalPlayer( ) {
        readyButton.transform.GetChild( 0 ).GetComponent<Text>( ).text = "JOIN";
        readyButton.interactable = true;

        readyButton.onClick.RemoveAllListeners( );
        readyButton.onClick.AddListener( OnReadyClicked );
    }

    private void SetupOtherPlayer( ) {
        readyButton.interactable = false;
        readyButton.gameObject.SetActive( false );
        OnClientReady( false );
    }

    public override void OnClientReady( bool readyState ) {
        if (readyState) {
            Text textComponent = readyButton.transform.GetChild( 0 ).GetComponent<Text>( );
            textComponent.text = "READY";
            ActivateEyes( );
            readyButton.interactable = false;
        } else {
            Text textComponent = readyButton.transform.GetChild( 0 ).GetComponent<Text>( );
            textComponent.text = isLocalPlayer ? "JOIN" : "...";
            readyButton.interactable = isLocalPlayer;
        }
    }

    public void OnReadyClicked( ) {
        SendReadyToBeginMessage( );
    }

    public void RpcToggleJoinButton( bool enabled ) {
        readyButton.gameObject.SetActive( enabled );
        waitingPlayerButton.gameObject.SetActive( !enabled );
    }

    [ClientRpc]
    public void RpcUpdateCountdown( int countdown ) {
        LobbyManager.s_Singleton.infoPanel.Display( "Match Starting in " + countdown, "", null, false );

        if (countdown == 0) {
            LobbyManager.s_Singleton.infoPanel.gameObject.SetActive( false );
        }
    }
}