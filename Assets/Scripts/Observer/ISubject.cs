using UnityEngine;
using System.Collections.Generic;

public interface ISubject {

    void AddObserver( IObserver newObserver );

    void AddUnityObservers( GameObject newObserver );

    void RemoveObserver( IObserver oldObserver );

    void RemoveUnityObserver( GameObject oldUnityObserver );

    void RemoveAllObservers( );

    string NumberOfObservers( );

    string NumberOfUnityObservers( );

    void GarbageCollectObservers( );

    void Notify( string staticEventName );

    void NotifyExtendedMessage( string staticEventName, List<string> extendedMessage );

    void NotifySendObject( Object sender, string staticEventName );
}
