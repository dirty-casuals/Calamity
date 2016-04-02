﻿using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLevelLoader : MonoBehaviour {

    private GameObject loadingUIPanel;

    private void Awake( ) {
        loadingUIPanel = GetComponentInChildren<RectTransform>( true ).gameObject;
    }

    public void LoadSchoolLevel( ) {
        loadingUIPanel.SetActive( true );
        SceneManager.LoadSceneAsync( "1_school" );
    }
}
