using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour {

    public Text fpsText;
    public TextMeshProUGUI scoreText;
    public GemManager gemManager;
    public FadeManager fadeManager;

	void Update () {
        fpsText.text = "FPS: " + (1f / Time.deltaTime).ToString("0");
        scoreText.text = "Score: " + (gemManager.score).ToString("0");
    }

    public void OnBackHit()
    {
        fadeManager.NextSceneIndex = 0;
        fadeManager.NeedToFade = true;
    }
}
