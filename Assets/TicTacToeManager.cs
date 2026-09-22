using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TicTacToeManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button[] boardButtons;         // 9つのマスのボタン
    [SerializeField] private Button restartButton;          // リトライボタン
    [SerializeField] private RectTransform boardPanelRect;  // BoardPanelのRectTransform
    [SerializeField] private TMP_Text resultText;           // 勝敗を表示するテキスト

    private bool isPlayer1Turn = true; // true: Oのターン, false: Xのターン
    private bool isGameOver = false;

    void Start()
    {
        // 1. リトライボタンの設定
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(ResetGame);
        }

        if (resultText != null)
        {
            resultText.text = "";
        }

        // 2. ボードパネルの中央配置
        if (boardPanelRect != null)
        {
            boardPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
            boardPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
            boardPanelRect.pivot = new Vector2(0.5f, 0.5f);
            boardPanelRect.anchoredPosition = Vector2.zero;
        }

        // 3. 各マスのボタンにクリック時のイベントを安全に登録（重複を防ぐ）
        for (int i = 0; i < boardButtons.Length; i++)
        {
            int index = i;
            boardButtons[i].onClick.RemoveAllListeners();
            boardButtons[i].onClick.AddListener(() => OnButtonClicked(index));
        }

        ResetGame();
    }

    void OnButtonClicked(int index)
    {
        // ゲームオーバーまたはすでに文字が入っている場合は絶対に処理を通さない
        if (isGameOver) return;

        TMP_Text buttonText = boardButtons[index].GetComponentInChildren<TMP_Text>();
        if (buttonText != null && !string.IsNullOrEmpty(buttonText.text))
        {
            return;
        }

        if (buttonText != null)
        {
            buttonText.text = isPlayer1Turn ? "○" : "x";
        }

        // 勝利判定
        string winner = CheckWin();
        if (!string.IsNullOrEmpty(winner))
        {
            isGameOver = true;
            ShowResult(winner + " Wins!"); // ← 「！」を外して文字化けを防止
            return;
        }

        // 引き分け判定
        if (CheckDraw())
        {
            isGameOver = true;
            ShowResult("Draw!");
            return;
        }

        isPlayer1Turn = !isPlayer1Turn;
    }

    string CheckWin()
    {
        int[,] winPatterns = new int[,]
        {
            {0, 1, 2}, {3, 4, 5}, {6, 7, 8}, // 横の列
            {0, 3, 6}, {1, 4, 7}, {2, 5, 8}, // 縦の列
            {0, 4, 8}, {2, 4, 6}             // 斜めの列
        };

        for (int i = 0; i < winPatterns.GetLength(0); i++)
        {
            int a = winPatterns[i, 0];
            int b = winPatterns[i, 1];
            int c = winPatterns[i, 2];

            string textA = boardButtons[a].GetComponentInChildren<TMP_Text>()?.text;
            string textB = boardButtons[b].GetComponentInChildren<TMP_Text>()?.text;
            string textC = boardButtons[c].GetComponentInChildren<TMP_Text>()?.text;

            if (!string.IsNullOrEmpty(textA) && textA == textB && textB == textC)
            {
                return textA;
            }
        }
        return null;
    }

    bool CheckDraw()
    {
        for (int i = 0; i < boardButtons.Length; i++)
        {
            string text = boardButtons[i].GetComponentInChildren<TMP_Text>()?.text;
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }
        }
        return true;
    }

    void ShowResult(string message)
    {
        if (resultText != null)
        {
            resultText.text = message;
        }

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
        }
    }

    public void ResetGame()
    {
        isGameOver = false;
        isPlayer1Turn = true;

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
        }

        if (resultText != null)
        {
            resultText.text = "";
        }

        for (int i = 0; i < boardButtons.Length; i++)
        {
            TMP_Text buttonText = boardButtons[i].GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = "";
            }
        }
    }
}