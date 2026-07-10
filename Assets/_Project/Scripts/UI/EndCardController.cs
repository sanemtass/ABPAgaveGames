using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AgavePuzzle.Gameplay;

namespace AgavePuzzle.UI
{
    public class EndCardController : MonoBehaviour
    {
        [SerializeField] private PuzzleBoard puzzleBoard;
        [SerializeField] private GameObject endCardRoot;
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private TextMeshProUGUI buttonLabel;
        [SerializeField] private Button actionButton;

        private void OnEnable()
        {
            puzzleBoard.OnLevelWon += HandleLevelWon;
            puzzleBoard.OnLevelLost += HandleLevelLost;
            actionButton.onClick.AddListener(RestartLevel);
        }

        private void OnDisable()
        {
            puzzleBoard.OnLevelWon -= HandleLevelWon;
            puzzleBoard.OnLevelLost -= HandleLevelLost;
            actionButton.onClick.RemoveListener(RestartLevel);
        }

        private void HandleLevelWon()
        {
            ShowEndCard(new LevelWonResult());
        }

        private void HandleLevelLost()
        {
            ShowEndCard(new LevelLostResult());
        }

        private void ShowEndCard(LevelEndResult result)
        {
            headerText.text = result.HeaderText;
            buttonLabel.text = result.ButtonLabel;
            endCardRoot.SetActive(true);
        }

        private void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}