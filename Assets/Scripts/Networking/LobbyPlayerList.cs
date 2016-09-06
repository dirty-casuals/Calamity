using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class LobbyPlayerList : MonoBehaviour {
    public static LobbyPlayerList _instance = null;

    public RectTransform playerListContentTransform;
    public GameObject warningDirectPlayServer;

    public void Awake( ) {
        _instance = this;
    }

    public void DisplayDirectServerWarning( bool enabled ) {
        if (warningDirectPlayServer != null)
            warningDirectPlayServer.SetActive( enabled );
    }

    public void AddPlayer( LobbyPlayer player ) {
        player.transform.position = new Vector3( 0, 0, 0 );
        player.transform.SetParent( playerListContentTransform, false );
    }
}