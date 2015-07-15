using UnityEngine;
using psai.net;
using System.Collections.Generic;


public class PsaiTcSkipIfNotVisible : PsaiTriggerCondition
{
    public Renderer rendererToCheck;
    private Renderer Renderer
    {
        get
        {
            if (rendererToCheck == null)
            {
                rendererToCheck = this.gameObject.GetComponent<Renderer>();
            }
            return rendererToCheck;
        }
    }
    

    public override bool EvaluateTriggerCondition()
    {
        if (this.Renderer != null)
        {
            return (this.Renderer.isVisible);
        }

        return false;               
    }
}
