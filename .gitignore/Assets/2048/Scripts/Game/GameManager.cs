using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Game _game;
    [SerializeField] private int _scoreToWin = 2048;

    [Header("- UI -")]
    [SerializeField] private Text _scoreText;
    [SerializeField] private Text _bestScoreText;
    [SerializeField] private GameObject _winUI;
    [SerializeField] private GameObject _gameOverUI;

    private string _bestScoreKey = "Best";
    private int _bestScore;

    public int CurrentScore { get; private set; }

    public delegate void OnGameOver();
    public delegate void OnGameWin();

    public event OnGameOver OnGameOverEvent;
    public event OnGameWin OnGameWinEvent;

    private void Start()
    {
        _bestScoreKey += SceneManager.GetActiveScene().buildIndex;
        _bestScore = PlayerPrefs.GetInt(_bestScoreKey, 0);
        _bestScoreText.text = _bestScore.ToString();

        _game.IncreaseScoreEvent += IncreaseScore;
        _game.CannotMoveEvent += GameOver;
    }

    public void Replay()
    {
        LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadScene(int index)
    {
       SceneManager.LoadScene(index);
    }

    private void IncreaseScore(int number)
    {
        CurrentScore += number;
        _scoreText.text = CurrentScore.ToString();

        if (CurrentScore > _bestScore)
            PlayerPrefs.SetInt(_bestScoreKey, CurrentScore);

        if (CheckWinState(number))
            Win();
    }

    private bool CheckWinState(int number)
    {
        return number == _scoreToWin ? true : false;
    }

    private void Win()
    {
        _winUI.SetActive(true);

        if (OnGameWinEvent != null)
            OnGameWinEvent();
    }

    private void GameOver()
    {
        _gameOverUI.SetActive(true);

        if (OnGameOverEvent != null)
            OnGameOverEvent();
    }
}
