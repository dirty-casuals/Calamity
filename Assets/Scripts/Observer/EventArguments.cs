using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EventArguments {
    public string eventMessage { get; set; }
    public List<string> extendedMessage { get; set; }
    public List<int> extendedMessageNumber { get; private set; }

    public EventArguments( string newEventMessage, List<string> newExtendedMessage ) {
        int newInteger;
        eventMessage = newEventMessage;
        extendedMessage = newExtendedMessage;

        if ( extendedMessage == null ) {
            return;
        }
        foreach( string message in extendedMessage ){
            if ( int.TryParse( message, out newInteger ) ) {
                extendedMessageNumber.Add( newInteger );
            }
        }
    }
}