using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

using Difficulty = DifficultyStats.Difficulty;

// How lost tries map onto hanger parts and body parts (GameplayViewController) and the win reveal (WinViewController)
public class GameplayStepsTests
{
    private const string _hangerPrefabPath = "Assets/Art/Prefabs/Hanger.prefab";

    private static readonly int _normalTries = DifficultyStats.Tries(Difficulty.NORMAL);

    private static readonly int _easyTries = DifficultyStats.Tries(Difficulty.EASY);

    // Every wrong guess but the last one raises one hanger part; the last one makes the character jump
    private static readonly int _hangerParts = _normalTries - 2;

    [Test]
    public void Should_Raise_Every_Hanger_Part_Once_In_Normal_Mode()
    {
        List<int> indices = new();

        // The first wrong guess leaves NORMAL tries - 1; at 1 try left the character jumps instead
        for (int triesLeft = _normalTries - 1; triesLeft > 1; --triesLeft)
        {
            Assert.False(GameplayViewController.IsLastTry(triesLeft));
            indices.Add(GameplayViewController.HangerPartIndex(_hangerParts, triesLeft));
        }

        Assert.AreEqual(Enumerable.Range(0, _hangerParts).ToArray(), indices.ToArray(), message: "Hanger parts should rise in order, each one once");
        Assert.True(GameplayViewController.IsLastTry(1));
    }

    [Test]
    public void Should_Build_Body_Before_Hanger_In_Easy_Mode()
    {
        for (int triesLeft = _easyTries - 1; triesLeft > 0; --triesLeft)
        {
            bool expected = triesLeft < _normalTries;
            Assert.AreEqual(expected, GameplayViewController.ShowsHangerPartInEasyMode(triesLeft), message: $"Invalid step kind for {triesLeft} tries left");
        }
    }

    [Test]
    public void Should_Show_Body_Parts_In_Order_And_Then_Whole_Body_In_Easy_Mode()
    {
        BodyPart[] expectedOrder = { BodyPart.LEFT_LEG, BodyPart.RIGHT_LEG, BodyPart.TORSO, BodyPart.LEFT_HAND, BodyPart.RIGHT_HAND };
        List<BodyPart> shown = new();

        int triesLeft = _easyTries - 1;
        for (; triesLeft > _normalTries; --triesLeft)
        {
            BodyPart? part = GameplayViewController.BodyPartToShow(_hangerParts, triesLeft);
            Assert.True(part.HasValue, message: $"A body part should be shown at {triesLeft} tries left");
            shown.Add(part.Value);
        }

        Assert.AreEqual(expectedOrder, shown.ToArray());
        Assert.IsNull(GameplayViewController.BodyPartToShow(_hangerParts, _normalTries), message: "The last body step should show the whole body");
    }

    [Test]
    public void Should_Count_No_Challange_Tries_Down_From_10()
    {
        int noChallangeTries = DifficultyStats.Tries(Difficulty.NO_CHALLANGE);

        Assert.AreEqual(10, GameplayViewController.DisplayedTries(noChallangeTries, noChallangeMode: true));
        Assert.AreEqual(1, GameplayViewController.DisplayedTries(noChallangeTries - 9, noChallangeMode: true));
        Assert.AreEqual(5, GameplayViewController.DisplayedTries(5, noChallangeMode: false), message: "Other modes show tries as they are");
    }

    [Test]
    public void Should_Have_As_Many_Hanger_Parts_As_Normal_Mode_Steps()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(_hangerPrefabPath);
        Assert.IsNotNull(prefab, message: $"{_hangerPrefabPath} is missing");
        var installer = prefab.GetComponentInChildren<HangerInstaller>(includeInactive: true);
        Assert.IsNotNull(installer, message: "Hanger prefab should have a HangerInstaller");

        SerializedProperty parts = new SerializedObject(installer).FindProperty("_parts");

        Assert.AreEqual(_hangerParts, parts.arraySize, message: $"Hanger needs {_hangerParts} parts for {_normalTries} NORMAL tries");
    }

    [Test]
    public void Should_Reveal_Body_On_Win_Only_If_Easy_Mode_Body_Is_Incomplete()
    {
        string easy = DifficultyStats.StringifyDifficulty(Difficulty.EASY);
        string normal = DifficultyStats.StringifyDifficulty(Difficulty.NORMAL);

        Assert.True(WinViewController.ShouldRevealBody(new() { Difficulty = easy, Tries = _easyTries }));
        Assert.True(WinViewController.ShouldRevealBody(new() { Difficulty = easy, Tries = _normalTries + 1 }));
        Assert.False(WinViewController.ShouldRevealBody(new() { Difficulty = easy, Tries = _normalTries }), message: "The body is already complete");
        Assert.False(WinViewController.ShouldRevealBody(new() { Difficulty = easy, Tries = 1 }));
        Assert.False(WinViewController.ShouldRevealBody(new() { Difficulty = normal, Tries = _normalTries }), message: "Only easy mode hides the body");
    }
}
