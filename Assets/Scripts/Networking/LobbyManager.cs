using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;
using System.Collections;

public class LobbyManager : NetworkLobbyManager {
    public static LobbyManager s_Singleton;

    public LobbyTopPanel topPanel;
    public RectTransform mainMenuPanel;
    public RectTransform lobbyPanel;
    public LobbyInfoPanel infoPanel;
    public Button backButton;
    public Text statusInfo;
    public Text hostInfo;
    public GameObject background;
    public bool isMatchmaking = false;

    public delegate void BackButtonDelegate( );
    public BackButtonDelegate backDelegate;

    protected RectTransform currentPanel;
    protected static float _matchStartCountdown = 5.0f;
    protected bool _disconnectServer = false;
    protected ulong _currentMatchID;
    protected LobbyHook lobbyHooks;

    [SerializeField]
    private GameObject[ ] eyes;
    private int clientsConnected = 0;

    private void Awake( ) {
        if (FindObjectsOfType<LobbyManager>( ).Length > 1)
            Destroy( gameObject );
    }

    private void Start( ) {
        s_Singleton = this;
        lobbyHooks = GetComponent<LobbyHook>( );
        currentPanel = mainMenuPanel;

        backButton.gameObject.SetActive( true );
        GetComponent<Canvas>( ).enabled = true;

        DontDestroyOnLoad( gameObject );

        SetServerInfo( "Offline", "None" );
    }

    public override void OnLobbyClientSceneChanged( NetworkConnection conn ) {
        if (!conn.playerControllers[ 0 ].unetView.isLocalPlayer)
            return;

        if (SceneManager.GetActiveScene( ).name == lobbyScene) {
            if (topPanel.isInGame) {
                ChangeTo( lobbyPanel );
                if (isMatchmaking) {
                    if (conn.playerControllers[ 0 ].unetView.isServer) {
                        backDelegate = StopHostClbk;
                    } else {
                        backDelegate = StopClientClbk;
                    }
                } else {
                    if (conn.playerControllers[ 0 ].unetView.isClient) {
                        backDelegate = StopHostClbk;
                    } else {
                        backDelegate = StopClientClbk;
                    }
                }
            } else {
                ChangeTo( mainMenuPanel );
            }

            topPanel.ToggleVisibility( true );
            topPanel.isInGame = false;
            background.SetActive( true );
        } else {
            ChangeTo( null );

            Destroy( GameObject.Find( "MainMenuUI(Clone)" ) );

            backDelegate = StopGameClbk;
            topPanel.isInGame = true;
            topPanel.ToggleVisibility( false );
            background.SetActive( false );
        }
    }

    public void ChangeTo( RectTransform newPanel ) {
        if (currentPanel != null) {
            currentPanel.gameObject.SetActive( false );
        }

        if (newPanel != null) {
            newPanel.gameObject.SetActive( true );
        }

        currentPanel = newPanel;

        if (currentPanel != mainMenuPanel) {
            backButton.gameObject.SetActive( true );
        } else {
            backButton.gameObject.SetActive( false );
            SetServerInfo( "Offline", "None" );
            isMatchmaking = false;
        }
    }

    public void DisplayIsConnecting( ) {
        var _this = this;
        infoPanel.Display( "Connecting...", "Cancel", ( ) => { _this.backDelegate( ); } );
    }

    public void SetServerInfo( string status, string host ) {
        statusInfo.text = status;
        hostInfo.text = host;
    }

    public void GoBackButton( ) {
        backDelegate( );
    }

    public void SimpleBackClbk( ) {
        ChangeTo( mainMenuPanel );
    }

    public void StopHostClbk( ) {
        if (isMatchmaking) {
            this.matchMaker.DestroyMatch( (NetworkID)_currentMatchID, OnMatchDestroyed );
            _disconnectServer = true;
        } else {
            StopHost( );
        }


        ChangeTo( mainMenuPanel );
    }

    public void StopClientClbk( ) {
        StopClient( );

        if (isMatchmaking) {
            StopMatchMaker( );
        }

        ChangeTo( mainMenuPanel );
    }

    public void StopServerClbk( ) {
        StopServer( );
        ChangeTo( mainMenuPanel );
    }

    public void StopGameClbk( ) {
        SendReturnToLobby( );
        ChangeTo( lobbyPanel );
    }

    public override void OnStartHost( ) {
        base.OnStartHost( );

        ChangeTo( lobbyPanel );
        backDelegate = StopHostClbk;
        SetServerInfo( "Hosting", networkAddress );
    }

    public override void OnClientConnect( NetworkConnection conn ) {
        base.OnClientConnect( conn );

        infoPanel.gameObject.SetActive( false );

        if (!NetworkServer.active) {
            ChangeTo( lobbyPanel );
            backDelegate = StopClientClbk;
            SetServerInfo( "Client", networkAddress );
        }
    }

    public override void OnMatchCreate( CreateMatchResponse matchInfo ) {
        base.OnMatchCreate( matchInfo );

        _currentMatchID = (System.UInt64)matchInfo.networkId;
    }

    public void OnMatchDestroyed( BasicResponse resp ) {
        if (_disconnectServer) {
            StopMatchMaker( );
            StopHost( );
        }
    }

    public void ActivateEyes( ) {
        eyes[ clientsConnected ].SetActive( true );
        clientsConnected = clientsConnected + 1;
    }

    public override GameObject OnLobbyServerCreateLobbyPlayer( NetworkConnection conn, short playerControllerId ) {
        GameObject obj = Instantiate( lobbyPlayerPrefab.gameObject ) as GameObject;

        LobbyPlayer newPlayer = obj.GetComponent<LobbyPlayer>( );

        newPlayer.RpcToggleJoinButton( numPlayers + 1 >= minPlayers );
        newPlayer.ToggleStartButton( numPlayers + 1 >= minPlayers );

        for (int i = 0; i < numPlayers; i += 1) {
            LobbyPlayer p = lobbySlots[ i ] as LobbyPlayer;

            if (p != null) {
                p.RpcToggleJoinButton( numPlayers + 1 >= minPlayers );
                p.ToggleStartButton( numPlayers + 1 >= minPlayers );
            }
        }

        return obj;
    }

    public override void OnLobbyServerDisconnect( NetworkConnection conn ) {
        for (int i = 0; i < numPlayers; ++i) {
            LobbyPlayer p = lobbySlots[ i ] as LobbyPlayer;

            if (p != null) {
                p.RpcToggleJoinButton( numPlayers >= minPlayers );
                p.ToggleStartButton( numPlayers >= minPlayers );
            }
        }

    }

    public override bool OnLobbyServerSceneLoadedForPlayer( GameObject lobbyPlayer, GameObject gamePlayer ) {
        if (lobbyHooks) {
            lobbyHooks.OnLobbyServerSceneLoadedForPlayer( this, lobbyPlayer, gamePlayer );
        }

        return true;
    }

    public void OnStartClicked( ) {
        StartCoroutine( ServerCountdownCoroutine( ) );
    }

    public override void OnLobbyServerPlayersReady( ) {
    }

    public IEnumerator ServerCountdownCoroutine( ) {
        float remainingTime = _matchStartCountdown;
        int floorTime = Mathf.FloorToInt( remainingTime );

        while (remainingTime > 0) {
            yield return null;

            remainingTime -= Time.deltaTime;
            int newFloorTime = Mathf.FloorToInt( remainingTime );

            if (newFloorTime != floorTime) {
                floorTime = newFloorTime;

                for (int i = 0; i < lobbySlots.Length; ++i) {
                    if (lobbySlots[ i ] != null) {
                        (lobbySlots[ i ] as LobbyPlayer).RpcUpdateCountdown( floorTime );
                    }
                }
            }
        }

        for (int i = 0; i < lobbySlots.Length; ++i) {
            if (lobbySlots[ i ] != null) {
                (lobbySlots[ i ] as LobbyPlayer).RpcUpdateCountdown( 0 );
            }
        }

        ServerChangeScene( playScene );
    }

    public override void OnClientDisconnect( NetworkConnection conn ) {
        base.OnClientDisconnect( conn );
        ChangeTo( mainMenuPanel );
    }

    public override void OnClientError( NetworkConnection conn, int errorCode ) {
        ChangeTo( mainMenuPanel );
        infoPanel.Display( "Cient error : " + (errorCode == 6 ? "timeout" : errorCode.ToString( )), "Close", null );
    }
}