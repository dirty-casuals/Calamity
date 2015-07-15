using UnityEngine;
using System.Collections;


public class PsaiTcSkipIfComponentIsDisabled : PsaiTriggerCondition
{
    public MonoBehaviour _componentToCheck;

    public override bool EvaluateTriggerCondition()
    {
        if (_componentToCheck)
        {
            return _componentToCheck.enabled;
        }

        return true;
    }
}
