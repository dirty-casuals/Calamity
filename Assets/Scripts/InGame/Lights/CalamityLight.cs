using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public abstract class CalamityLight : NetworkObserver {

    public override void OnStartServer( ) {
        GameHandler.RegisterForStateEvents( this.gameObject );
    }

    public override void OnNotify( UnityEngine.Object sender, EventArguments e ) {
        switch (e.eventMessage) {
            case LightsHandler.SET_PRE_CALAMITY_LIGHTING:
                RpcSetPreCalamityLighting( );
                break;

            case LightsHandler.SET_CALAMITY_LIGHTING:
                RpcSetCalamityLighting( );
                break;
        }
    }

    [ClientRpc]
    protected abstract void RpcSetPreCalamityLighting( );

    [ClientRpc]
    protected abstract void RpcSetCalamityLighting( );
}
