using System;
using UnityEngine;
using UnityEngine.UI;

public class LinkAddressAction : MonoBehaviour
{
    [SerializeField] private string _urlAddress;

    [SerializeField] private Button _actionButton;

    public static bool IsValidURL(string url)
    {
        bool isUrlCreated = Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult);
        return isUrlCreated && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    void Start()
    {
        if (_actionButton == null) _actionButton = GetComponent<Button>();
        _actionButton?.onClick.AddListener(handleOnClick);
    }

    private void handleOnClick()
    {
        if (IsValidURL(_urlAddress)) Application.OpenURL(_urlAddress);
    }
}
