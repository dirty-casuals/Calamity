using UnityEngine;
using System.Collections;

public class GameResult : MonoBehaviour {
    private bool allDied = false;
    private bool localPlayerWon = false;
    private bool otherPlayerWon = false;
    private int survivors = 0;

    public void SetEndResultAllDied( ) {
        allDied = true;
    }

    public void SetEndResultManySurvivors( int survivors ) {
        this.survivors = survivors;
    }

    public void SetLocalPlayerWon( ) {
        localPlayerWon = true;
    }

    public void SetOtherPlayerWon( ) {
        otherPlayerWon = true;
    }

    public bool DidAllDie( ) {
        return allDied;
    }

    public bool LocalPlayerWon( ) {
        return localPlayerWon;
    }

    public bool OtherPlayerWon( ) {
        return otherPlayerWon;
    }

    public int GetSurvivors( ) {
        return survivors;
    }
}
