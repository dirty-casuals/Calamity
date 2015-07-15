using UnityEngine;
using System.Collections;

public class InputController : MonoBehaviour
{
    private Command A_BUTTON;

    private void Awake( )
    {
        BindButtonsToCommand( );
    }

    //Bind/Swap New Commands Here
    private void BindButtonsToCommand( )
    {
        A_BUTTON = new AButtonCommand( );
    }

    private void FixedUpdate( )
    {
        UpdateControls( );
    }

    private void UpdateControls( )
    {
        if ( Input.GetKeyDown( KeyCode.Space ) )
        {
            A_BUTTON.Execute( gameObject );
        }
    }
}