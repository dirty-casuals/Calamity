using UnityEngine;

public class PanelToggle : MonoBehaviour {
    public GameObject togglePanel;
    private bool toggle = false;

    public void TogglePanel( ) {
        toggle = !toggle;
        togglePanel.SetActive( toggle );
    }
}
