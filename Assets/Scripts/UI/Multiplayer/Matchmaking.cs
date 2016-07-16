using UnityEngine;

public class Matchmaking : MonoBehaviour {

    [HideInInspector]
    public LobbyManager lobbyManager;

    private void Start( ) {
        lobbyManager = GetComponent<LobbyManager>( );
    }

    public void CreateGameAndHost( ) {
        lobbyManager.StartHost( );
    }
}
