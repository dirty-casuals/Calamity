using UnityEngine;
using System.Collections;
using RAIN.Action;
using RAIN.Representation;
using RAIN.Core;
using RAIN.Motion;

[RAINDecision]
public class NotReachedTarget : RAINDecision {
    public Expression target;
    public Expression targetLanded;

    private int _lastRunning = 0;

    public override void Start( RAIN.Core.AI ai ) {
        base.Start( ai );

        _lastRunning = 0;
    }

    public override RAINDecision.ActionResult Execute( AI ai ) {
        base.Start( ai );

        ActionResult result = ActionResult.FAILURE;
        ActionResult tResult = ActionResult.FAILURE;

        if (target.IsValid && target.IsVariable) {
            Vector3 tTarget = target.Evaluate<Vector3>( ai.DeltaTime, ai.WorkingMemory );
            float tDistance = (tTarget - ai.Kinematic.Position).magnitude;

            if (tDistance > 1.5f) {
                for (; _lastRunning < _children.Count; _lastRunning++) {
                    tResult = _children[ _lastRunning ].Run( ai );
                    if (tResult != ActionResult.SUCCESS) {
                        break;
                    }
                }

                result = tResult;
            } else {
                if (targetLanded.IsValid && targetLanded.IsVariable) {
                    ai.WorkingMemory.SetItem<bool>( targetLanded.VariableName, false );
                }
            }
        }

        return result;
    }
}
