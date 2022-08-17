using Zenject;
using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;
using TMPro;

public class DifficultyViewControllerTests : ZenjectIntegrationTestFixture
{
    private const string _testData = "sasik";

    void CommonInstall()
    {
        PreInstall();

        Container.Bind<TextMeshProUGUI>().FromNewComponentOnNewGameObject().AsSingle();

        var messageField = Container.Resolve<TextMeshProUGUI>();
        var tabsContainer = new TextTabsContainer(new() { Message = messageField });
        var viewTools = new ViewTools() { TextTabsContainer = tabsContainer };

        Container.Bind<DifficultyViewController>().AsSingle();
        Container.BindInstances(_testData, viewTools);


        PostInstall();
    }

    [UnityTest]
    public IEnumerator Suould_Set_Text_To_Message_Field()
    {
        CommonInstall();

        var viewController = Container.Resolve<DifficultyViewController>();
        var messageField = Container.Resolve<TextMeshProUGUI>();

        Assert.True(string.IsNullOrEmpty(messageField.text), message: $"Field text should be empty as defalut");

        yield return viewController.PlayActions(default);

        Assert.AreEqual(_testData, messageField.text, message: $"Field text should be set as test data");
    }
}