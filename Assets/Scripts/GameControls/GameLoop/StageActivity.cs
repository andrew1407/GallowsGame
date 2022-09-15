using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class StageActivity : IResetable
{
    private readonly MonoBehaviour _monoBehaviour;

    private Coroutine _currentCoroutine;

    private TaskCompletionSource<bool> _coroutineFinished;

    public StageActivity(MonoBehaviour monoBehaviour) => _monoBehaviour = monoBehaviour;

    public async Task<bool> AwaitActionFinish(IEnumerator action)
    {
        _coroutineFinished = new();
        _currentCoroutine = _monoBehaviour.StartCoroutine(actionAwaiter(action));
        bool finished = await _coroutineFinished.Task;
        clear();
        return finished;
    }

    public void ResetState()
    {
        stop();
        clear();
    }

    private IEnumerator actionAwaiter(IEnumerator action)
    {
        yield return action;
        _coroutineFinished?.TrySetResult(true);
    }

    private void clear()
    {
        _currentCoroutine = null;
        _coroutineFinished = null;
    }

    private void stop()
    {
        if (_currentCoroutine != null) _monoBehaviour.StopCoroutine(_currentCoroutine);
        if (_coroutineFinished != null) _coroutineFinished.TrySetResult(false);
    }    
}