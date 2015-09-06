using UnityEngine;
using System.Collections;

public class MonsterTransformation : MonoBehaviour {

    public CameraFollow characterCamera;
    public GameObject player;
    public GameObject monster;

    public void TransformPlayerIntoMonster( ) {
        player.SetActive( false );
        monster.transform.position = player.transform.position;
        characterCamera.cameraTarget = monster.transform;
        monster.SetActive( true );
    }
}