using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

namespace UnityStandardAssets.Network {
    //Player entry in the lobby. Handle selecting color/setting name & getting ready for the game
    //Any LobbyHook can then grab it and pass those value to the game player prefab (see the Pong Example in the Samples Scenes)
    public class LobbyPlayer : NetworkLobbyPlayer {
        static Color[ ] Colors = new Color[ ] { Color.magenta, Color.red, Color.cyan, Color.blue, Color.green, Color.yellow };

        //used on server to avoid assigning the same color to two player
        static List<int> _colorInUse = new List<int>( );

        public Button colorButton;
        public InputField nameInput;
        public Button readyButton;
        public Button waitingPlayerButton;
        public List<GameObject> eyes = new List<GameObject>( );

        [SyncVar( hook = "OnMyName" )]
        public string playerName = "";
        public Color playerColor = Color.white;

        private static Color JoinColor = new Color( 255.0f / 255.0f, 0.0f, 101.0f / 255.0f, 1.0f );
        private static Color NotReadyColor = new Color( 34.0f / 255.0f, 44 / 255.0f, 55.0f / 255.0f, 1.0f );
        private static Color ReadyColor = new Color( 0.0f, 204.0f / 255.0f, 204.0f / 255.0f, 1.0f );
        private static Color TransparentColor = new Color( 0, 0, 0, 0 );

        private static Color OtherPlayerColor = new Color( 250.0f / 255.0f, 250.0f / 255.0f, 250.0f / 255.0f, 1.0f );
        private static Color LocalPlayerColor = new Color( 180.0f / 255.0f, 180.0f / 255.0f, 180.0f / 255.0f, 1.0f );

        public override void OnClientEnterLobby( ) {
            base.OnClientEnterLobby( );
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
            //setup the player data on UI. The value are SyncVar so the player
            //will be created with the right value currently on server
            OnMyName( playerName );
        }

        private void Update( ) {
            if (SceneManager.GetActiveScene( ).name != LobbyManager.s_Singleton.lobbyScene)
                return;

            InputField obj = (EventSystem.current.currentSelectedGameObject != null) ? EventSystem.current.currentSelectedGameObject.GetComponent<InputField>( ) : null;

            if (isLocalPlayer && (obj == null || !obj.isFocused)) {
                int localIdx = playerControllerId + 1;
                if (!readyToBegin && Input.GetButtonDown( "Fire" + localIdx )) {
                    if (readyButton.IsActive( ) && readyButton.IsInteractable( ))
                        ActivateEyes( );
                    SendReadyToBeginMessage( );
                }
            }
        }

        private void ActivateEyes( ) {
            NetworkInstanceId id = netId;
            if (id.ToString( ) == "1") {
                eyes[ 0 ].SetActive( true );
            }
            if(id.ToString() == "2") {
                eyes[ 1 ].SetActive( true );
            }
        }

        private void SetupLocalPlayer( ) {
            nameInput.interactable = true;
            readyButton.transform.GetChild( 0 ).GetComponent<Text>( ).text = "JOIN";
            readyButton.interactable = true;
            //have to use child count of player prefab already setup as "this.slot" is not set yet
            if (playerName == "") {
                CmdNameChanged( "Player" + LobbyPlayerList._instance.playerListContentTransform.childCount );
            }
            nameInput.onEndEdit.RemoveAllListeners( );
            nameInput.onEndEdit.AddListener( OnNameChanged );

            colorButton.onClick.RemoveAllListeners( );
            colorButton.onClick.AddListener( OnColorClicked );

            readyButton.onClick.RemoveAllListeners( );
            readyButton.onClick.AddListener( OnReadyClicked );
        }

        private void SetupOtherPlayer( ) {
            nameInput.interactable = false;
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

        ///===== callback from sync var
        public void OnMyName( string newName ) {
            playerName = newName;
            nameInput.text = playerName;
        }

        //===== UI Handler

        //Note that those handler use Command function, as we need to change the value on the server not locally
        //so that all client get the new value throught syncvar
        public void OnColorClicked( ) {
            CmdColorChange( );
        }

        public void OnReadyClicked( ) {
            SendReadyToBeginMessage( );
        }

        public void OnNameChanged( string str ) {
            CmdNameChanged( str );
        }

        //====== Client RPC
        public void RpcToggleJoinButton( bool enabled ) {
            readyButton.gameObject.SetActive( enabled );
            waitingPlayerButton.gameObject.SetActive( !enabled );
        }

        [ClientRpc]
        public void RpcUpdateCountdown( int countdown ) {
            LobbyManager.s_Singleton.infoPanel.Display( "Match Starting in " + countdown, "", null, false );

            if (countdown == 0)
                LobbyManager.s_Singleton.infoPanel.gameObject.SetActive( false );
        }

        //====== Server Command

        [Command]
        public void CmdColorChange( ) {
            int idx = System.Array.IndexOf( Colors, playerColor );

            int inUseIdx = _colorInUse.IndexOf( idx );

            if (idx < 0) idx = 0;

            idx = (idx + 1) % Colors.Length;

            bool alreadyInUse = false;

            do {
                alreadyInUse = false;
                for (int i = 0; i < _colorInUse.Count; ++i) {
                    if (_colorInUse[ i ] == idx) {//that color is already in use
                        alreadyInUse = true;
                        idx = (idx + 1) % Colors.Length;
                    }
                }
            }
            while (alreadyInUse);

            if (inUseIdx >= 0) {//if we already add an entry in the colorTabs, we change it
                _colorInUse[ inUseIdx ] = idx;
            } else {
                _colorInUse.Add( idx );
            }

            playerColor = Colors[ idx ];
        }

        [Command]
        public void CmdNameChanged( string name ) {
            playerName = name;
        }

        public void OnDestroy( ) {
            int idx = System.Array.IndexOf( Colors, playerColor );

            if (idx < 0)
                return;

            for (int i = 0; i < _colorInUse.Count; ++i) {
                if (_colorInUse[ i ] == idx) {//that color is already in use
                    _colorInUse.RemoveAt( i );
                    break;
                }
            }
        }
    }
}
