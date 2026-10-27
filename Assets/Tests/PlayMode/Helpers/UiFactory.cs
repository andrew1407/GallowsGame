using TMPro;
using UnityEngine;

// Builds the bare UI pieces the game wires up in the scene, without any canvas or prefab
public static class UiFactory
{
    public static TextMeshProUGUI MakeText(string name) => new GameObject(name).AddComponent<TextMeshProUGUI>();

    public static TextTabsContainer.Tabs MakeTabs() => new() {
        Message = MakeText(nameof(TextTabsContainer.Tabs.Message)),
        Tries = MakeText(nameof(TextTabsContainer.Tabs.Tries)),
        Word = MakeText(nameof(TextTabsContainer.Tabs.Word)),
        CharactedDialogue = MakeText(nameof(TextTabsContainer.Tabs.CharactedDialogue)),
        RunnerDialogue = MakeText(nameof(TextTabsContainer.Tabs.RunnerDialogue)),
    };

    public static ViewTools MakeViewTools(TextTabsContainer.Tabs tabs) => new() { TextTabsContainer = new TextTabsContainer(tabs) };

    public static TMP_InputField MakeInputField(string name)
    {
        // Wired up while inactive, so the field enables with its text parts already in place
        var obj = new GameObject(name, typeof(RectTransform));
        obj.SetActive(false);
        var field = obj.AddComponent<TMP_InputField>();
        var text = MakeText(name + "Text");
        text.transform.SetParent(obj.transform);
        field.textViewport = obj.GetComponent<RectTransform>();
        field.textComponent = text;
        obj.SetActive(true);
        return field;
    }

    public static void Destroy(TextTabsContainer.Tabs tabs)
    {
        TextMeshProUGUI[] texts = { tabs.Message, tabs.Tries, tabs.Word, tabs.CharactedDialogue, tabs.RunnerDialogue };
        foreach (var text in texts)
            if (text != null) Object.Destroy(text.gameObject);
    }
}
