using System;
using UnityEngine;

public class LevelNumberManager : MonoBehaviour
{
    public static int Level { get; private set; }

    public static event Action LevelChanged;

    public static void GoToFirstLevel()
    {
        GoToLevel(1);
    }

    public static void GoToNextLevel()
    {
        GoToLevel(Level + 1);
    }

    public static void GoToLevel(int level)
    {
        Level = level;

        LevelChanged?.Invoke();
    }
}
