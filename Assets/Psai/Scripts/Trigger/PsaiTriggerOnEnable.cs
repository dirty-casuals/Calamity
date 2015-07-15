using UnityEngine;
using psai.net;

public class PsaiTriggerOnEnable : PsaiOneShotTrigger
{
    new void OnEnable()
    {
        base.OnEnable();
        TryToFireOneShotTrigger(this.intensity);
    }
}
