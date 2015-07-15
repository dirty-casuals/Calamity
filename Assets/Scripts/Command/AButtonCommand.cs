using UnityEngine;
using System.Collections;

//Example of how to create a new command
public class AButtonCommand : Command
{
    public override void Execute( GameObject controllableObject )
    {
        //controllableObject.GetComponent<CommandExample>( ).Message( );
    }
}