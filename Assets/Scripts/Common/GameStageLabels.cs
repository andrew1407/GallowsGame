public sealed class GameStageLabels
{
    public const string Difficulty = "difficulty";

    public const string Prologue = "prologue";
    
    public const string Gameplay = "gameplay";
    
    public const string Win = "win";
    
    public const string Loss = "loss";

    private static readonly string[] _interractiveStages = { Difficulty, Gameplay };

    public static bool IsInterractive(string stage) => System.Array.IndexOf(_interractiveStages, stage) > -1;
}
