using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Zenject;

public class GameModeSelectorTests : ZenjectIntegrationTestFixture
{
    private StartOptionsContainer _options;

    private TMP_InputField _addressInput;

    private TextTabsContainer.Tabs _tabs;

    void CommonInstall(bool offline)
    {
        PreInstall();

        _addressInput = UiFactory.MakeInputField("Address");
        _options = new StartOptionsContainer() {
            BlockSelf = new GameObject("StartMenu"),
            StartButton = new GameObject("Start").AddComponent<Button>(),
            OfflineMode = new GameObject("Offline").AddComponent<Toggle>(),
            AddressField = _addressInput.gameObject,
        };
        _options.OfflineMode.SetIsOnWithoutNotify(offline);

        _tabs = UiFactory.MakeTabs();
        GameLoopBindings.Install(Container, _tabs, UiFactory.MakeInputField("GameInput"));
        Container.BindInstance(_options);
        Container.Bind<GameplayStrategyFactory>().AsSingle();
        Container.Bind<GameModeSelector>().AsSingle();

        PostInstall();

        // The same order as GameManager.Start
        Container.Resolve<GameModeSelector>().Initialize();
        Container.Resolve<GameLoop>().Initialize();
    }

    [UnityTest]
    public IEnumerator Should_Start_Offline_Game_And_Hide_Menu()
    {
        CommonInstall(offline: true);

        _options.StartButton.onClick.Invoke();
        yield return Wait.Until(() => _tabs.Message.text == GameLoopBindings.DifficultyText, "Difficulties weren't shown");

        Assert.False(_options.BlockSelf.activeSelf, message: "Start menu should be hidden once the game starts");
        Assert.True(_tabs.Message.enabled);
    }

    [UnityTest]
    public IEnumerator Should_Hide_Address_Field_In_Offline_Mode()
    {
        CommonInstall(offline: true);
        yield return null;

        Assert.False(_options.AddressField.activeSelf);
    }

    [UnityTest]
    public IEnumerator Should_Show_Address_Field_In_Online_Mode()
    {
        CommonInstall(offline: false);
        yield return null;

        Assert.True(_options.AddressField.activeSelf);
    }

    [UnityTest]
    public IEnumerator Should_Toggle_Address_Field_With_Offline_Mode()
    {
        CommonInstall(offline: true);

        _options.OfflineMode.isOn = false;
        yield return null;
        Assert.True(_options.AddressField.activeSelf, message: "Address field should appear in online mode");

        _options.OfflineMode.isOn = true;
        yield return null;
        Assert.False(_options.AddressField.activeSelf, message: "Address field should disappear in offline mode");
    }

    [UnityTest]
    public IEnumerator Should_Clear_Address_When_Switching_To_Offline()
    {
        CommonInstall(offline: false);

        _addressInput.text = "http://localhost:8080";
        _options.OfflineMode.isOn = true;
        yield return null;

        Assert.IsEmpty(_addressInput.text);
    }

    [UnityTest]
    public IEnumerator Should_Stay_In_Menu_And_Clear_Invalid_Address_On_Start()
    {
        CommonInstall(offline: false);
        string[] addresses = { "sasik", "ftp://localhost:8080", "https://localhost" };

        foreach (var address in addresses)
        {
            _addressInput.text = address;
            _options.StartButton.onClick.Invoke();
            yield return null;

            Assert.True(_options.BlockSelf.activeSelf, message: $"Game shouldn't start with \"{address}\" address");
            Assert.IsEmpty(_addressInput.text, message: $"Invalid address \"{address}\" should be cleared");
        }
    }
}
