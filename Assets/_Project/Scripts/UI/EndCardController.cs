using DG.Tweening;
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
        [SerializeField] private CanvasGroup endCardCanvasGroup;
        [SerializeField] private RectTransform gameIconRect;
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private TextMeshProUGUI buttonLabel;
        [SerializeField] private Button actionButton;
        [SerializeField] private float fadeDuration = 0.3f;

        private void OnEnable()
        {
            puzzleBoard.OnLevelWon += HandleLevelWon;
            puzzleBoard.OnLevelLost += HandleLevelLost;
        }

        private void OnDisable()
        {
            puzzleBoard.OnLevelWon -= HandleLevelWon;
            puzzleBoard.OnLevelLost -= HandleLevelLost;
            actionButton.onClick.RemoveAllListeners();
        }

        private void HandleLevelWon()
        {
            if (puzzleBoard.HasNextLevel)
            {
                ShowEndCard(new LevelWonResult());
                actionButton.onClick.AddListener(LoadNextLevel);
            }
            else
            {
                ShowEndCard(new GameFinishedResult());
                actionButton.gameObject.SetActive(false);
            }
        }

        private void HandleLevelLost()
        {
            ShowEndCard(new LevelLostResult());
            actionButton.onClick.AddListener(RestartLevel);
        }

        private void ShowEndCard(LevelEndResult result)
        {
            headerText.text = result.HeaderText;
            buttonLabel.text = result.ButtonLabel;

            endCardRoot.SetActive(true);
            endCardCanvasGroup.alpha = 0f;
            gameIconRect.localScale = Vector3.zero;

            endCardCanvasGroup.DOFade(1f, fadeDuration).SetLink(endCardRoot);
            gameIconRect.DOScale(1f, fadeDuration * 1.5f)
                .SetEase(Ease.OutBack)
                .SetLink(endCardRoot);
        }

        private void LoadNextLevel()
        {
            LevelProgress.CurrentLevelIndex++;
            ReloadScene();
        }

        private void RestartLevel()
        {
            ReloadScene();
        }

        private void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}