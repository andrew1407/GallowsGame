using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class StageActivityTests
{
    private CoroutineRunner _runner;

    [SetUp]
    public void SetUp() => _runner = new GameObject(nameof(CoroutineRunner)).AddComponent<CoroutineRunner>();

    [TearDown]
    public void TearDown() => Object.Destroy(_runner.gameObject);

    [UnityTest]
    public IEnumerator Should_Return_True_When_Action_Finishes()
    {
        StageActivity stageActivity = new(_runner);
        int framesPlayed = 0;

        Task<bool> finished = stageActivity.AwaitActionFinish(countFrames(3, () => ++framesPlayed));
        yield return Wait.For(finished);

        Assert.True(finished.Result, message: "Finished action should report success");
        Assert.AreEqual(3, framesPlayed);
    }

    [UnityTest]
    public IEnumerator Should_Return_False_And_Stop_Action_On_Reset()
    {
        StageActivity stageActivity = new(_runner);
        int framesPlayed = 0;

        Task<bool> finished = stageActivity.AwaitActionFinish(countFrames(int.MaxValue, () => ++framesPlayed));
        yield return null;
        yield return null;
        stageActivity.ResetState();
        yield return Wait.For(finished);
        int framesOnReset = framesPlayed;
        yield return null;
        yield return null;

        Assert.False(finished.Result, message: "Interrupted action should report failure");
        Assert.AreEqual(framesOnReset, framesPlayed, message: "Action coroutine should be stopped on reset");
    }

    [Test]
    public void Should_Reset_Without_Running_Action()
    {
        StageActivity stageActivity = new(_runner);

        Assert.DoesNotThrow(stageActivity.ResetState);
    }

    [UnityTest]
    public IEnumerator Should_Run_Next_Action_After_Reset()
    {
        StageActivity stageActivity = new(_runner);

        Task<bool> interrupted = stageActivity.AwaitActionFinish(countFrames(int.MaxValue, () => { }));
        yield return null;
        stageActivity.ResetState();
        yield return Wait.For(interrupted);

        Task<bool> finished = stageActivity.AwaitActionFinish(countFrames(1, () => { }));
        yield return Wait.For(finished);

        Assert.True(finished.Result);
    }

    private IEnumerator countFrames(int frames, System.Action onFrame)
    {
        for (int i = 0; i < frames; ++i)
        {
            onFrame();
            yield return null;
        }
    }
}
