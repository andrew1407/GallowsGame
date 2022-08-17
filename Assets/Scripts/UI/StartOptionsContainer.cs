using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct StartOptionsContainer
{
    public GameObject BlockSelf;

    public Button StartButton;

    public Toggle OfflineMode;

    public GameObject AddressField;
}
