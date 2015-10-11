using UnityEngine;
using System.Collections;

public class Observer {
    public Observer( ) {
        InitializeObserver( );
    }

    public virtual void InitializeObserver( ) { }

    public virtual void OnNotify( Object sender, EventArguments e ) { }
}