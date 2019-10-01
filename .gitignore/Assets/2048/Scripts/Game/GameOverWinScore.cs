using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Text))]
public class GameOverWinScore : MonoBehaviour {

    [SerializeField] private GameManager _gameManager;

    private void Start()
    {
        _gameManager.OnGameWinEvent += UpdateScoreText;
        _gameManager.OnGameOverEvent += UpdateScoreText;

        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        GetComponent<Text>().text = _gameManager.CurrentScore.ToString();
    }

}
