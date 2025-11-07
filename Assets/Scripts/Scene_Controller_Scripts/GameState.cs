using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState
{
    // For let the game scene know which option player choosed when the game start
    public enum difficulties
    {
        Easy,
        Medium,
        Hard,
    }

    public static difficulties difficulty;

    // For implement save and load system
    public enum playOptions
    {
        NewGame,
        LoadGame,
    }

    public static playOptions playOption;
}
