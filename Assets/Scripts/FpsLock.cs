using UnityEngine;
using System.Collections;

public class FpsLock : MonoBehaviour {

    [SerializeField]
    private int targetFPS = 45;
    
	private void Start () {
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = targetFPS;
    }
}
