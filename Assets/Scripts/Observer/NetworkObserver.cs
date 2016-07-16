using UnityEngine;
using UnityEngine.Networking;

public class NetworkObserver : NetworkBehaviour, IObserver {
    public virtual void OnNotify( Object sender, EventArguments e ) { }
}