using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

public class Subject : MonoBehaviour, ISubject {
    private readonly List<IObserver> listOfObservers = new List<IObserver>( );
    private List<GameObject> listOfUnityObservers = new List<GameObject>( );

    public virtual void AddObserver( IObserver newObserver ) {
        if ( listOfObservers.Contains( newObserver ) ) {
            return; 
        }
        listOfObservers.Add( newObserver );
    }

    public virtual void AddUnityObservers( GameObject newObserver ) {
        if ( listOfUnityObservers.Contains( newObserver ) ) {
            return;
        }
        listOfUnityObservers.Add( newObserver );
    }

    public virtual void RemoveObserver( IObserver oldObserver ) {
        if ( listOfObservers.Contains( oldObserver ) ) {
            listOfObservers.Remove( oldObserver );
        } else {
            Debug.LogWarning( "List Doesn't Contain Observer" );
        }
    }

    public virtual void RemoveUnityObserver( GameObject oldUnityObserver ) {
        if ( listOfUnityObservers.Contains( oldUnityObserver ) ) {
            listOfUnityObservers.Remove( oldUnityObserver );
        } else {
            Debug.LogWarning( "List Doesn't Contain Unity Observer" );
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

    private void NotifySendAll( Object sender, string eventName, List<string> extendedMessage ) {
        GarbageCollectObservers( );
        foreach ( var observer in listOfObservers ) {
            observer.OnNotify( sender, new EventArguments( eventName, extendedMessage ) );
        }
        NotifyUnityObservers( sender, eventName, extendedMessage );
    }

    private void NotifyUnityObservers( Object sender, string unityEventName, List<string> extendedMessage ) {
        foreach ( var unityObserver in listOfUnityObservers ) {
            unityObserver.GetComponent<IObserver>( ).OnNotify( sender,
            new EventArguments( unityEventName, extendedMessage ) );
        }
    }
}