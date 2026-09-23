using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TicTacToeManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button[] boardButtons;         // 9つのマスのボタン
    [SerializeField] private Button restartButton;          // リトライボタン
    [SerializeField] private Button titleButton;            // タイトルに戻るボタン
    [SerializeField] private RectTransform boardPanelRect;  // BoardPanelのRectTransform
    [SerializeField] private TMP_Text resultText;           // 勝敗を表示するテキスト

    private int gameMode = 0;           // 0: 通常モード, 1: 新モード（移動あり）
    private bool isPlayer1Turn = true;  // true: ◯のターン, false: xのターン
    private bool isGameOver = false;

    // 新モード用の変数
    private int p1PieceCount = 0;       
    private int p2PieceCount = 0;       
    private enum GamePhase { Placing, Moving }
    private GamePhase currentPhase = GamePhase.Placing;
    private int selectedCellIndex = -1; 

    void Start()
    {
        // 0. タイトル画面からモードを取得
        gameMode = PlayerPrefs.GetInt("GameMode", 0);

        // 1. リトライボタンの設定
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(ResetGame);
        }

        // 2. タイトルに戻るボタンの設定（インスペクターの紐付けを使用）
        if (titleButton != null)
        {
            titleButton.gameObject.SetActive(false);
            titleButton.onClick.RemoveAllListeners();
            titleButton.onClick.AddListener(OnClickTitleButton);
        }
        else
        {
            Debug.LogError("❌ インスペクターの [Title Button] にボタンが割り当てられていません！");
        }

        if (resultText != null)
        {
            resultText.text = "";
        }

        // 3. ボードパネルの中央配置
        if (boardPanelRect != null)
        {
            boardPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
            boardPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
            boardPanelRect.pivot = new Vector2(0.5f, 0.5f);
            boardPanelRect.anchoredPosition = Vector2.zero;
        }

        // 4. 各マスのボタンにクリック時のイベントを登録
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
        if (isGameOver) return;

        TMP_Text buttonText = boardButtons[index].GetComponentInChildren<TMP_Text>();
        if (buttonText == null) return;

        if (gameMode == 0)
        {
            // 通常モード
            if (!string.IsNullOrEmpty(buttonText.text)) return;
            buttonText.text = isPlayer1Turn ? "○" : "x";

            string winner = CheckWin();
            if (!string.IsNullOrEmpty(winner))
            {
                isGameOver = true;
                ShowResult(winner + " Wins!");
                return;
            }

            if (CheckDraw())
            {
                isGameOver = true;
                ShowResult("Draw!");
                return;
            }

            isPlayer1Turn = !isPlayer1Turn;
        }
        else
        {
            // 新モード
            string mySign = isPlayer1Turn ? "○" : "x";

            if (currentPhase == GamePhase.Placing)
            {
                if (!string.IsNullOrEmpty(buttonText.text)) return;

                buttonText.text = mySign;
                if (isPlayer1Turn) p1PieceCount++;
                else p2PieceCount++;

                string winner = CheckWin();
                if (!string.IsNullOrEmpty(winner))
                {
                    isGameOver = true;
                    ShowResult(winner + " Wins!");
                    return;
                }

                isPlayer1Turn = !isPlayer1Turn;

                if (p1PieceCount >= 3 && p2PieceCount >= 3)
                {
                    currentPhase = GamePhase.Moving;
                }
                UpdateStatusText();
            }
            else
            {
                if (selectedCellIndex == -1)
                {
                    if (buttonText.text == mySign)
                    {
                        selectedCellIndex = index;
                        HighlightSelectedCell(index);
                        UpdateStatusText();
                    }
                }
                else
                {
                    if (index == selectedCellIndex)
                    {
                        selectedCellIndex = -1;
                        ResetHighlight();
                        UpdateStatusText();
                        return;
                    }

                    if (string.IsNullOrEmpty(buttonText.text))
                    {
                        TMP_Text prevText = boardButtons[selectedCellIndex].GetComponentInChildren<TMP_Text>();
                        if (prevText != null) prevText.text = "";

                        buttonText.text = mySign;
                        ResetHighlight();
                        selectedCellIndex = -1;

                        string winner = CheckWin();
                        if (!string.IsNullOrEmpty(winner))
                        {
                            isGameOver = true;
                            ShowResult(winner + " Wins!");
                            return;
                        }

                        isPlayer1Turn = !isPlayer1Turn;
                        UpdateStatusText();
                    }
                    else
                    {
                        if (buttonText.text == mySign)
                        {
                            ResetHighlight();
                            selectedCellIndex = index;
                            HighlightSelectedCell(index);
                            UpdateStatusText();
                        }
                    }
                }
            }
        }
    }

    string CheckWin()
    {
        int[,] winPatterns = new int[,]
        {
            {0, 1, 2}, {3, 4, 5}, {6, 7, 8},
            {0, 3, 6}, {1, 4, 7}, {2, 5, 8},
            {0, 4, 8}, {2, 4, 6}
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
        if (gameMode != 0) return false;
        for (int i = 0; i < boardButtons.Length; i++)
        {
            string text = boardButtons[i].GetComponentInChildren<TMP_Text>()?.text;
            if (string.IsNullOrEmpty(text)) return false;
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

        if (titleButton != null)
        {
            titleButton.gameObject.SetActive(true);
        }
    }

    void UpdateStatusText()
    {
        if (resultText == null || isGameOver) return;

        if (gameMode == 1)
        {
            string turnStr = isPlayer1Turn ? "○" : "x";
            if (currentPhase == GamePhase.Placing)
                resultText.text = turnStr + "'s Turn (Placing)";
            else
                resultText.text = (selectedCellIndex == -1) ? turnStr + "'s Turn: Select your piece" : turnStr + "'s Turn: Select empty cell";
        }
        else
        {
            resultText.text = "";
        }
    }

    void HighlightSelectedCell(int index)
    {
        ResetHighlight();

       Image img = boardButtons[index].GetComponent<Image>();
       if(img != null){
        img.color = Color.yellow;
       }
    }

    void ResetHighlight()
    {
        for (int i = 0; i < boardButtons.Length; i++)
        {
            Image img = boardButtons[i].GetComponent<Image>();
            if(img != null){
                img.color = Color.white;
            }
        }
    }

    public void ResetGame()
    {
        isGameOver = false;
        isPlayer1Turn = true;
        p1PieceCount = 0;
        p2PieceCount = 0;
        currentPhase = GamePhase.Placing;
        selectedCellIndex = -1;
        ResetHighlight();

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
        }

        if (titleButton != null)
        {
            titleButton.gameObject.SetActive(false);
        }

        if (resultText != null)
        {
            resultText.text = "";
        }

        for (int i = 0; i < boardButtons.Length; i++)
        {
            TMP_Text buttonText = boardButtons[i].GetComponentInChildren<TMP_Text>();
            if (buttonText != null) buttonText.text = "";
        }
        UpdateStatusText();
    }

    public void OnClickTitleButton()
    {
        SceneManager.LoadScene("TitleScene");
    }
}