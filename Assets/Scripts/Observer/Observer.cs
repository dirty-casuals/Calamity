using UnityEngine;
using System.Collections;

public class Observer {
    public Observer( ) {
        //Subject.AddObserver( this );
    }

    public virtual void OnNotify( Object sender, EventArguments e ) {
    }
}