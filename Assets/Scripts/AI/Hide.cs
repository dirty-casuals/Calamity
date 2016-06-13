using UnityEngine;
using System.Collections;
using RAIN.Action;
using RAIN.Representation;
using RAIN.Core;
using RAIN.Motion;
using RAIN.Navigation.Waypoints;

[RAINAction]
public class Hide : RAINAction {
    public Expression waypointArrivedAtVariable = new Expression( );
    public Expression moveSpeedVariable = new Expression( );

    private string lastName = null;

    public override RAINDecision.ActionResult Execute( AI ai ) {
        base.Start( ai );

        ActionResult result = ActionResult.SUCCESS;
        if (!waypointArrivedAtVariable.IsValid || !waypointArrivedAtVariable.IsVariable) {
            return ActionResult.FAILURE;
        }

        if (!moveSpeedVariable.IsValid || !moveSpeedVariable.IsVariable) {
            return ActionResult.FAILURE;
        }

        Waypoint target = ai.WorkingMemory.GetItem<Waypoint>( waypointArrivedAtVariable.VariableName );
        GameObject monster = ai.WorkingMemory.GetItem<GameObject>( "frmMonster" );
        GameObject player = ai.WorkingMemory.GetItem<GameObject>( "frmPlayer" );
        string targetName = target.WaypointName;

        ai.WorkingMemory.SetItem<string>( "lastName", lastName );
        if (targetName != lastName && targetName == "Hide") {
            ai.WorkingMemory.SetItem<int>( moveSpeedVariable.VariableName, 0 );
        } else {
            ai.WorkingMemory.SetItem<int>( moveSpeedVariable.VariableName, 1 );
        }

        if(player) {
            ai.WorkingMemory.SetItem<int>( moveSpeedVariable.VariableName, 2 );
        }

        if (monster) {
            ai.WorkingMemory.SetItem<int>( moveSpeedVariable.VariableName, 3 );
        }

        lastName = targetName;

        return result;
    }
}
