using UnityEngine;
using System.Collections;

public class UnityObserver : MonoBehaviour {
    //Don't Override Awake
    protected void Awake( ) {
        this.gameObject.tag = "UnityObserver";
        InitializeUnityObserver( );
    }

    public virtual void InitializeUnityObserver( ) { }

    public virtual void OnNotify( Object sender, EventArguments e ) { }
}