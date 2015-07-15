using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Subject : MonoBehaviour {
    private readonly List<Observer> listOfObservers = new List<Observer>( );
    private List<GameObject> listOfUnityObservers = new List<GameObject>( );

    public virtual void AddObserver( Observer newObserver ) {
        if ( listOfObservers.Contains( newObserver ) ) {
            Debug.LogWarning( "List Already Contains Observer" );
            return;
        }
        listOfObservers.Add( newObserver );
    }

    public virtual void AddUnityObservers( GameObject newObserver ) {
        if ( listOfUnityObservers.Contains( newObserver ) ) {
            Debug.LogWarning( "Unity Observer already exists" );
            return;
        }
        listOfUnityObservers.Add( newObserver );
    }

    public virtual void RemoveObserver( Observer oldObserver ) {
        if ( listOfObservers.Contains( oldObserver ) ) {
            listOfObservers.Remove( oldObserver );
        } else {
            Debug.LogWarning( "List Doesn't Contain Observer" );
        }
    }

    public virtual void RemoveAllObservers( ) {
        listOfObservers.Clear( );
    }

    public virtual string NumberOfObservers( ) {
        return "UnityObs: " + listOfObservers.Count;
    }

    public virtual string NumberOfUnityObservers( ) {
        return "Obs: " + listOfUnityObservers.Count( );
    }

    public virtual void GarbageCollectObservers( ) {
        listOfObservers.RemoveAll( item => item == null );
        listOfUnityObservers.RemoveAll( item => item == null );
    }

    public virtual void Notify( string staticEventName ) {
        NotifySendAll( null, staticEventName, null );
    }

    public virtual void NotifyExtendedMessage( string staticEventName, List<string> extendedMessage ) {
        NotifySendAll( null, staticEventName, extendedMessage );
    }

    public virtual void NotifySendObject( Object sender, string staticEventName ) {
        NotifySendAll( sender, staticEventName, null );
    }

    public virtual void NotifySendAll( Object sender, string eventName, List<string> extendedMessage ) {
        GarbageCollectObservers( );
        foreach ( var observer in listOfObservers ) {
            observer.OnNotify( sender, new EventArguments( eventName, extendedMessage ) );
        }
        NotifyUnityObservers( sender, eventName, extendedMessage );
    }

    public virtual void NotifyUnityObservers( Object sender, string unityEventName, List<string> extendedMessage ) {
        foreach ( var unityObserver in listOfUnityObservers ) {
            unityObserver.GetComponent<UnityObserver>( ).OnNotify( sender,
            new EventArguments( unityEventName, extendedMessage ) );
        }
    }
}