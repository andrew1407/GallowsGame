using System;
using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

public static class Wait
{
    private const float _timeout = 5;

    public static IEnumerator For(Task task) => Until(() => task.IsCompleted, "Task wasn't completed");

    public static IEnumerator Until(Func<bool> condition, string message)
    {
        float startTime = Time.realtimeSinceStartup;
        yield return new WaitUntil(() => condition() || Time.realtimeSinceStartup - startTime > _timeout);
        Assert.True(condition(), message: $"{message} in {_timeout} seconds");
    }
}
