using System.Threading.Tasks;

public interface IGameplayStrategy
{
    Task Setup();

    Task<GameProgress> StageAction(string input);
}
