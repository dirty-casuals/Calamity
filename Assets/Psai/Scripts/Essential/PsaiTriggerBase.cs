using UnityEngine;
using System.Collections;
using psai.net;


public abstract class PsaiTriggerBase : MonoBehaviour
{
    public int themeId = 1;

    protected PsaiTriggerCondition[] _triggerConditionsInGameObject;

    void Awake()
    {
        _triggerConditionsInGameObject = this.gameObject.GetComponents<PsaiTriggerCondition>();
    }

    /// <summary>
    /// Evaluates all PsaiTriggerConditions attached to this GameObject and returns true if they all succeeded, false otherwise.
    /// </summary>
    /// <remarks>
    /// PsaiTriggerConditions in child or parent nodes are ignored.
    /// </remarks>
    /// <returns></returns>
    public bool EvaluateAllTriggerConditions()
    {
        foreach (PsaiTriggerCondition condition in _triggerConditionsInGameObject)
        {
            if (condition.EvaluateTriggerCondition() == false)
                return false;
        }

        return EvaluateTriggerCondition();
    }


    /// <summary>
    /// Override this to define a general condition if this Trigger should fire in the current tick or not.
    /// </summary>
    /// <returns></returns>
    public virtual bool EvaluateTriggerCondition()
    {
        return true;
    }


}
