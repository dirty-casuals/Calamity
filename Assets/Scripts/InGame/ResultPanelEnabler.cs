using UnityEngine;
using UnityEngine.UI;

public class ResultPanelEnabler : MonoBehaviour {
    [SerializeField]
    private GameObject allDiedPanel;
    [SerializeField]
    private GameObject youWonPanel;
    [SerializeField]
    private GameObject otherPlayerWonPanel;
    [SerializeField]
    private GameObject manySurvivorsWonPanel;
    [SerializeField]
    private GameObject survivorsText;

    private void Start( ) {
        GameResult gameResult = FindObjectOfType<GameResult>( );

        if (gameResult.DidAllDie( )) {
            allDiedPanel.SetActive( true );
        } else if( gameResult.LocalPlayerWon()) {
            youWonPanel.SetActive( true );
        } else if( gameResult.OtherPlayerWon()) {
            otherPlayerWonPanel.SetActive( true );
        } else {
            Text textValue = survivorsText.GetComponent<Text>( );
            textValue.text = gameResult.GetSurvivors( ).ToString( );
            manySurvivorsWonPanel.SetActive( true );
        }

        Destroy( gameResult.gameObject );
    }
}
