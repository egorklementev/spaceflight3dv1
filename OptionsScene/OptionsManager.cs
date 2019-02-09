using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsManager : MonoBehaviour {

    [Header("Text references")]
    public TextMeshProUGUI gemNumberText;
    public TextMeshProUGUI seqSizeText;
    public TextMeshProUGUI gridXText;
    public TextMeshProUGUI gridYText;
    [Space(10)]

    [Header("Slider references")]
    public Slider gemNumberSlider;
    public Slider seqSizeSlider;
    public Slider gridXSlider;
    public Slider gridYSlider;
    [Space(10)]

    [Header("Dropdown references")]
    public TMP_Dropdown qualityDropdown;
    [Space(10)]

    [Header("Fading")]
    public FadeManager fadeManager;

    private void Start()
    {
        gemNumberSlider.value = PlayerPrefs.GetInt("gemNumber", 8);
        seqSizeSlider.value = PlayerPrefs.GetInt("seqSize", 3);
        gridXSlider.value = PlayerPrefs.GetInt("sizeX", 8);
        gridYSlider.value = PlayerPrefs.GetInt("sizeY", 5);
    }

    public void GemNumberSlider(float gemNumber)
    {
        gemNumberText.text = gemNumber.ToString("0");
    }

    public void SeqSizeSlider(float seqSize)
    {
        seqSizeText.text = seqSize.ToString("0");
    }

    public void GridXSlider(float sizeX)
    {
        gridXText.text = sizeX.ToString("0");
    }

    public void GridYSlider(float sizeY)
    {
        gridYText.text = sizeY.ToString("0");
    }

    public void OnDropdownUpdate(int dropdown)
    {
        QualitySettings.SetQualityLevel(dropdown);
    }

    public void StartGame()
    {
        PlayerPrefs.SetInt("gemNumber", (int)gemNumberSlider.value);
        PlayerPrefs.SetInt("seqSize", (int)seqSizeSlider.value);
        PlayerPrefs.SetInt("sizeX", (int)gridXSlider.value);
        PlayerPrefs.SetInt("sizeY", (int)gridYSlider.value);
        PlayerPrefs.Save();

        fadeManager.NeedToFade = true;
    }

}
