using TMPro;
using UnityEngine;

public class UILevelNumber : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI LevelText = null;

    private void Awake()
    {
        LevelText.text = $"Level: 000";
    }

    private void Start()
    {
        LevelText.text = $"Level: {LevelNumberManager.Level:000}";

        LevelNumberManager.LevelChanged += LevelNumberManager_LevelChanged;
    }

    private void LevelNumberManager_LevelChanged()
    {
        SetLevelText();
    }

    private void OnDestroy()
    {
        LevelNumberManager.LevelChanged -= LevelNumberManager_LevelChanged;
    }

    private void SetLevelText()
    {
        LevelText.text = $"Level: {LevelNumberManager.Level:000}";
    }
}
