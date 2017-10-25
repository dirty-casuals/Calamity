using UnityEngine;
using System.Collections;

public abstract class CalamityLight : UnityObserver {

    public void Start( ) {
        SetupObserver( );
        GameHandler.RegisterForStateEvents( this.gameObject );
    }

    public override void OnNotify( UnityEngine.Object sender, EventArguments e ) {
        switch (e.eventMessage) {
            case LightsHandler.SET_PRE_CALAMITY_LIGHTING:
                SetPreCalamityLighting( );
                break;

            case LightsHandler.SET_CALAMITY_LIGHTING:
                SetCalamityLighting( );
                break;
        }
    }

    protected abstract void SetPreCalamityLighting( );

    protected abstract void SetCalamityLighting( );
}
