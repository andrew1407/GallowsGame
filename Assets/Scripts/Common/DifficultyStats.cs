using System;
using System.Collections.Generic;

public sealed class DifficultyStats
{
    public enum Difficulty
    {
        DIFFICULT = 1,
        NORMAL = 2,
        EASY = 3,
        NO_CHALLANGE = 4,
    }

    public static bool Is(string value, Difficulty comparable) => value == StringifyDifficulty(comparable);

    public static string StringifyDifficulty(Difficulty difficulty) => (difficulty as Enum).ToString();

    public static int Tries(Difficulty difficulty) => _difficultyTries[StringifyDifficulty(difficulty)];

    public static int Tries(string difficulty) => _difficultyTries[difficulty];
    
    private static readonly Dictionary<string, int> _difficultyTries = new() {
        {StringifyDifficulty(Difficulty.DIFFICULT).ToString(), 1},
        {StringifyDifficulty(Difficulty.NORMAL).ToString(), 7},
        {StringifyDifficulty(Difficulty.EASY), 13},
        {StringifyDifficulty(Difficulty.NO_CHALLANGE), 11819615},
    };
}
