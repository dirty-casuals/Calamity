using UnityEngine;
using System.Collections;

public interface IObserver {
    void OnNotify( Object sender, EventArguments e );
}
