using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(this);
            return;
        }

        _instance = this;

        DontDestroyOnLoad(gameObject);

        LevelNumberManager.GoToFirstLevel();
    }
}
