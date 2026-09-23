using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    // 通常モードが選ばれたとき（モード番号: 0）
    public void OnClickNormalMode()
    {
        PlayerPrefs.SetInt("GameMode", 0);
        SceneManager.LoadScene("SampleScene"); // ※既存のゲームシーン名に合わせて変更してください
    }

    // 新モードが選ばれたとき（モード番号: 1）
    public void OnClickNewMode()
    {
        PlayerPrefs.SetInt("GameMode", 1);
        SceneManager.LoadScene("SampleScene"); // ※既存のゲームシーン名に合わせて変更してください
    }
}