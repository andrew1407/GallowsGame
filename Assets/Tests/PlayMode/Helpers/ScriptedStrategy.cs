using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Plays back queued stages and records what the game loop asks for
public class ScriptedStrategy : IGameplayStrategy, IResetable, IDisposable
{
    public readonly Queue<GameProgress> Stages = new();

    public readonly List<string> Inputs = new();

    public int SetupCalls { get; private set; }

    public int ResetCalls { get; private set; }

    public int DisposeCalls { get; private set; }

    public Task Setup()
    {
        ++SetupCalls;
        return Task.CompletedTask;
    }

    public Task<GameProgress> StageAction(string input)
    {
        Inputs.Add(input);
        return Task.FromResult(Stages.Count > 0 ? Stages.Dequeue() : default);
    }

    public void ResetState() => ++ResetCalls;

    public void Dispose() => ++DisposeCalls;
}
