using UnityEngine;
using psai.net;
using System.Collections.Generic;


public class PsaiTcSkipIfThemeTypeIsPlaying : PsaiTriggerCondition
{
    public ThemeType themeType = ThemeType.action;

    public override bool EvaluateTriggerCondition()
    {
        PsaiInfo psaiInfo = PsaiCore.Instance.GetPsaiInfo();
        if (psaiInfo != null)
        {
            ThemeInfo effectiveTheme = PsaiCore.Instance.GetThemeInfo(psaiInfo.effectiveThemeId);
            if (effectiveTheme != null)
            {
                return !(effectiveTheme.type == this.themeType);
            }
        }

        return true;
    }
}
