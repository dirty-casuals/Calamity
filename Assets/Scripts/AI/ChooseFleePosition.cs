using UnityEngine;
using System;
using System.Collections;
using RAIN.Action;
using RAIN.Core;
using RAIN.Motion;
using RAIN.Representation;

[RAINAction( "Choose Flee Position" )]
public class ChooseFleePosition : RAINAction {

    public Expression fleeDistance = new Expression( );
    public Expression fleeFrom = new Expression( );
    public Expression stayOnGraph = new Expression( );
    public Expression fleeTargetVariable = new Expression( );
    private float defaultFleeDistance = 10.0f;
    private MoveLookTarget fleeTarget = new MoveLookTarget( );

    public override RAINAction.ActionResult Execute( AI ai ) {
        //tFleeDistance
        float validFleeDistance = GetValidFleeDistance( ai );

        if ( !fleeTargetVariable.IsVariable ) {
            throw new Exception( "The Choose Flee Position node requires a valide Flee Target Variable" );
        }

        //Start by trying to flee away from the flee from
        if ( fleeFrom.IsVariable ) {
            MoveLookTarget.GetTargetFromVariable( ai.WorkingMemory, fleeFrom.VariableName,
                ai.Motor.DefaultCloseEnoughDistance, fleeTarget );
        } else {
            fleeTarget.TargetType = MoveLookTarget.MoveLookTargetType.None;
        }

        if ( fleeTarget.IsValid ) {
            Vector3 calculateOppositeDirection = ai.Kinematic.Position - fleeTarget.Position;
            Vector3 fleeDirection = calculateOppositeDirection.normalized *
                                    UnityEngine.Random.Range( 1.0f, validFleeDistance );
            Vector3 fleePosition = ai.Kinematic.Position + fleeDirection;
            if ( ai.Navigator.OnGraph( fleePosition, ai.Motor.MaxHeightOffset ) ) {
                ai.WorkingMemory.SetItem<Vector3>( fleeTargetVariable.VariableName, fleePosition );
                return ActionResult.SUCCESS;
            }

            Vector3 fortyFive = Quaternion.Euler( new Vector3( 0, 45, 0 ) ) * fleeDirection;
            fleePosition = ai.Kinematic.Position + fortyFive;
            if ( ai.Navigator.OnGraph( fleePosition, ai.Motor.MaxHeightOffset ) ) {
                ai.WorkingMemory.SetItem<Vector3>( fleeTargetVariable.VariableName, fleePosition );
                return ActionResult.SUCCESS;
            }

            Vector3 negativeFortyFive = Quaternion.Euler( new Vector3( 0, -45, 0 ) ) * fleeDirection;
            fleePosition = ai.Kinematic.Position + negativeFortyFive;
            if ( ai.Navigator.OnGraph( fleePosition, ai.Motor.MaxHeightOffset ) ) {
                ai.WorkingMemory.SetItem<Vector3>( fleeTargetVariable.VariableName, fleePosition );
                return ActionResult.SUCCESS;
            }

        }

        Vector3 direction = new Vector3( UnityEngine.Random.Range( -1.0f, 1.0f ), 0.0f,
                                         UnityEngine.Random.Range( -1.0f, 1.0f ) );
        direction *= validFleeDistance;

        Vector3 destination = ai.Kinematic.Position + direction;
        if ( stayOnGraph.IsValid && ( stayOnGraph.Evaluate<bool>( ai.DeltaTime, ai.WorkingMemory ) ) ) {
            if ( !ai.Navigator.OnGraph( destination, ai.Motor.MaxHeightOffset ) ) {
                return ActionResult.FAILURE;
            }
        }
        ai.WorkingMemory.SetItem<Vector3>( fleeTargetVariable.VariableName, destination );
        return ActionResult.SUCCESS;
    }

    private float GetValidFleeDistance( AI ai ) {
        float calculatedFleedDistance = 0.0f;

        if ( fleeDistance.IsValid ) {
            calculatedFleedDistance = fleeDistance.Evaluate<float>( ai.DeltaTime, ai.WorkingMemory );
        }
        if ( calculatedFleedDistance <= 0.0f ) {
            calculatedFleedDistance = defaultFleeDistance;
        }
        return calculatedFleedDistance;
    }

}