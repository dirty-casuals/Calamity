using UnityEditor;
using UnityEngine;
using psai.net;

[CustomEditor(typeof(PsaiColliderBasedTrigger), true)]
public class PsaiColliderBasedTriggerEditor : PsaiSynchronizedTriggerEditor
{
    public void InspectorGuiPlayerCollider(PsaiColliderBasedTrigger trigger)
    {
        EditorGUILayout.Separator();
        trigger.PlayerCollider = EditorGUILayout.ObjectField("Player Collider", trigger.PlayerCollider, typeof(Collider), true) as Collider;
        trigger.LocalCollider = EditorGUILayout.ObjectField("Local Collider", trigger.LocalCollider, typeof(Collider), true) as Collider;
    }
}
