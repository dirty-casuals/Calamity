using UnityEngine;
using UnityEngine.Networking;

public class UnityObserver : MonoBehaviour, IObserver {

    protected void SetupObserver( ) {
        gameObject.tag = "UnityObserver";
        InitializeUnityObserver( );
    }

    public virtual void InitializeUnityObserver( ) { }

    public virtual void OnNotify( Object sender, EventArguments e ) { }
}