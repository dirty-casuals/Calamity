using UnityEngine;
using System.Collections;

public class UnityObserver : MonoBehaviour
{
    protected void Awake( )
    {
        this.gameObject.tag = "UnityObserver";
        //Subject.AddUnityObservers( );
    }
    public virtual void OnNotify( Object sender, EventArguments e ) { }
}