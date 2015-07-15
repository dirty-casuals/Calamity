using UnityEditor;
using UnityEngine;
using psai.net;

[CustomEditor(typeof(PsaiTriggerOnPlayerCollision), true)]
class PsaiTriggerOnPlayerCollisionEditor : PsaiColliderBasedTriggerEditor
{
    public override void OnInspectorGUI()
    {
        PsaiTriggerOnPlayerCollision trigger = (PsaiTriggerOnPlayerCollision)target;

        InspectorGuiThemeSelection(trigger);
        trigger.intensity = EditorGUILayout.Slider("Intensity", trigger.intensity, 0.1f, 1.0f);

        InspectorGuiOneShot(trigger);
        InspectorGuiPlayerCollider(trigger);
    }
}

