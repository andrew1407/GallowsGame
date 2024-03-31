using System;
using UnityEngine;
using UnityEngine.UI;

public class LinkAddressAction : MonoBehaviour
{
    [SerializeField] private string _urlAddress;

    public static bool IsValidURL(string url)
    {
        Uri uriResult;
        bool isValidUrl = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

        return isValidUrl;
    }

    // Start is called before the first frame update
    void Start()
    {
        var button = GetComponent<Button>();
        if (button != null) button.onClick.AddListener(handleOnClick);
    }

    private void handleOnClick()
    {
        if (IsValidURL(_urlAddress)) Application.OpenURL(_urlAddress);
    }
}
