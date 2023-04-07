using System.Collections;
using System.Collections.Generic;
using Niantic.ARDK.AR.WayspotAnchors;
using UnityEngine;
using UnityEngine.UI;

namespace Niantic.ARDKExamples.RemoteAuthoring
{
  public class LocationSelectView: MonoBehaviour
  {
    private LocationManifestManager _locationManifestManager;

    [SerializeField]
    private Dropdown locationDropdown;

    [SerializeField]
    private Button loadLocationButton;

    [SerializeField]
    private Button openButton;

    [SerializeField]
    private Button closeButton;

    [SerializeField]
    private Text statusLog;

    [SerializeField]
    private Text localizationStatus;

    private int _selectedLocation = 0;

    public void Start()
    {
      _locationManifestManager = LocationManifestManager.Instance;
      //populate dropdown with list of locations from manifests
      var locNames = _locationManifestManager.GetLocationNames();
      List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
      foreach (var locName in locNames)
      {
        options.Add(new Dropdown.OptionData(locName));
      }

      locationDropdown.options = options;

      //keep track of selected location
      locationDropdown.onValueChanged.AddListener(OnChangeDropdown);

      //link load button to load behavior
      loadLocationButton.onClick.AddListener(LoadButtonClicked);

      //link close button
      closeButton.onClick.AddListener(CloseButtonClicked);

      //link open button
      openButton.onClick.AddListener(OpenButtonClicked);

      //subscribe to status tracking to present to user
      _locationManifestManager.StatusLogChangeEvent += StatusLogChanged;
      _locationManifestManager.AddLocalizationStatusListener(LocationStatusChanged);

      //default to first item selected
      locationDropdown.value = 0;
    }

    private void OnChangeDropdown(int selected)
    {
      _selectedLocation = selected;
    }

    private void LoadButtonClicked()
    {
      _locationManifestManager.LoadWayspotAnchors(_selectedLocation);
    }

    private void CloseButtonClicked()
    {
      gameObject.SetActive(false);
    }

    private void OpenButtonClicked()
    {
      gameObject.SetActive(true);
    }

    private void StatusLogChanged(string message)
    {
      statusLog.text = message;
    }

    private void LocationStatusChanged(LocalizationStateUpdatedArgs args)
    {
      string text = "Localization Status: ";
      if (args != null)
      {
        text += $"{args.State} " +
          (args.State == LocalizationState.Failed ? $"(Reason: {args.FailureReason})" : "");
      }
      localizationStatus.text = text;
    }
  }
}
