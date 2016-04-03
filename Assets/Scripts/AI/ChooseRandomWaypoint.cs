using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using RAIN.Action;
using RAIN.Core;
using RAIN.Representation;
using RAIN.Motion;
using RAIN.Navigation;
using RAIN.Navigation.Graph;
using RAIN.Navigation.Waypoints;

[RAINAction( "Choose Next Waypoint" )]
public class ChooseWaypoint : RAINAction {

    public Expression WaypointNetwork = new Expression( ); //either the name of the network or a variable containing a network
    public Expression MoveTargetVariable = new Expression( ); //the variable you want to use for the output move target

    private MoveLookTarget moveTarget = new MoveLookTarget( );
    private int waypointArrivedAt = -1;
    private int previousWaypoint = -1;
    private WaypointSet lastWaypointSet = null;

    public override ActionResult Execute( AI ai ) {
        if (!MoveTargetVariable.IsValid || !MoveTargetVariable.IsVariable) {
            return ActionResult.FAILURE;
        }

        WaypointSet waypointSet = GetWaypointSetFromExpression( ai );
        if (waypointSet == null) {
            return ActionResult.FAILURE;
        }

        if (waypointSet != lastWaypointSet) {
            waypointArrivedAt = -1;
            lastWaypointSet = waypointSet;
        }

        if (waypointArrivedAt == -1) {
            waypointArrivedAt = waypointSet.GetClosestWaypointIndex( ai.Kinematic.Position );
            if (waypointArrivedAt < 0) {
                return ActionResult.FAILURE;
            }

            moveTarget.VectorTarget = waypointSet.Waypoints[ waypointArrivedAt ].position;
            moveTarget.CloseEnoughDistance = Mathf.Max( waypointSet.Waypoints[ waypointArrivedAt ].range, ai.Motor.CloseEnoughDistance );
            if (!ai.Motor.IsAt( moveTarget )) {
                ai.WorkingMemory.SetItem<MoveLookTarget>( MoveTargetVariable.VariableName, moveTarget );
                return ActionResult.SUCCESS;
            }
        }

        NavigationGraphNode tNode = waypointSet.Graph.GetNode( waypointArrivedAt );
        if (tNode.OutEdgeCount > 0) {
            List<int> tConnectedNodes = new List<int>( );
            for (int k = 0; k < tNode.OutEdgeCount; k++) {
                int tIndex = ((VectorPathNode)tNode.EdgeOut( k ).ToNode).NodeIndex;
                if ((tIndex != previousWaypoint) && (!tConnectedNodes.Contains( tIndex )))
                    tConnectedNodes.Add( tIndex );
            }
            if (tConnectedNodes.Count == 0) {
                previousWaypoint = waypointArrivedAt;
                waypointArrivedAt = ((VectorPathNode)tNode.EdgeOut( 0 ).ToNode).NodeIndex;
            } else {
                previousWaypoint = waypointArrivedAt;
                waypointArrivedAt = tConnectedNodes[ UnityEngine.Random.Range( 0, tConnectedNodes.Count ) ];
            }
        }
        moveTarget.VectorTarget = waypointSet.Waypoints[ waypointArrivedAt ].position;
        moveTarget.CloseEnoughDistance = Mathf.Max( waypointSet.Waypoints[ waypointArrivedAt ].range, ai.Motor.CloseEnoughDistance );
        ai.WorkingMemory.SetItem<MoveLookTarget>( MoveTargetVariable.VariableName, moveTarget );

        return ActionResult.SUCCESS;
    }

    private WaypointSet GetWaypointSetFromExpression( AI ai ) {
        WaypointSet waypointSet = null;

        if (WaypointNetwork != null && WaypointNetwork.IsValid) {
            if (WaypointNetwork.IsVariable) {
                string varName = WaypointNetwork.VariableName;
                if (ai.WorkingMemory.ItemExists( varName )) {
                    Type t = ai.WorkingMemory.GetItemType( varName );
                    if (t == typeof( WaypointRig ) || t.IsSubclassOf( typeof( WaypointRig ) )) {
                        WaypointRig wgComp = ai.WorkingMemory.GetItem<WaypointRig>( varName );
                        if (wgComp != null)
                            waypointSet = wgComp.WaypointSet;
                    } else if (t == typeof( WaypointSet ) || t.IsSubclassOf( typeof( WaypointSet ) )) {
                        waypointSet = ai.WorkingMemory.GetItem<WaypointSet>( varName );
                    } else if (t == typeof( GameObject )) {
                        GameObject go = ai.WorkingMemory.GetItem<GameObject>( varName );
                        if (go != null) {
                            WaypointRig wgComp = go.GetComponentInChildren<WaypointRig>( );
                            if (wgComp != null)
                                waypointSet = wgComp.WaypointSet;
                        }
                    } else {
                        string setName = ai.WorkingMemory.GetItem<string>( varName );
                        if (!string.IsNullOrEmpty( setName ))
                            waypointSet = NavigationManager.Instance.GetWaypointSet( setName );
                    }
                } else {
                    if (!string.IsNullOrEmpty( varName ))
                        waypointSet = NavigationManager.Instance.GetWaypointSet( varName );
                }
            } else if (WaypointNetwork.IsConstant) {
                waypointSet = NavigationManager.Instance.GetWaypointSet( WaypointNetwork.Evaluate<string>( 0, ai.WorkingMemory ) );
            }
        }

        return waypointSet;
    }
}