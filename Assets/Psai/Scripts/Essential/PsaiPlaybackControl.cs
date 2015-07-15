using UnityEngine;
using psai.net;

[System.Serializable]
public abstract class PsaiPlaybackControl : MonoBehaviour
{
    public bool immediately = false;
    public float fadeoutSeconds = 3.0f;
    public bool dontExecuteIfTriggersAreFiring;
    public ThemeType restrictBlockToThisThemeType;
    public abstract void OnSignal();
}
