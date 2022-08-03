using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class Example
{
    [UnityTest]
    public IEnumerator ExampleWithEnumeratorPasses()
    {
        yield return null;
        Assert.That(true);
    }
}
