using TMPro;
using UnityEngine;

public class UITime : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI TimeText = null;

    private void Awake()
    {
        TimeText.text = "00.00";
    }

    private void Start()
    {
        TimeText.text = "00.00";
    }

    private void FixedUpdate()
    {
        SetTimeText();
    }

    private void SetTimeText()
    {
        int minutes = Mathf.FloorToInt(Time.timeSinceLevelLoad / 60f);
        int seconds = Mathf.FloorToInt(Time.timeSinceLevelLoad - (minutes * 60));

        TimeText.text = $"Time: {minutes}:{seconds}";
    }
}
