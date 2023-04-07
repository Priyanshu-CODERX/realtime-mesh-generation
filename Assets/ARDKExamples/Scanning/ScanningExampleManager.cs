// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;

using Niantic.ARDK.AR.Scanning;
using Niantic.ARDK.Extensions;
using Niantic.ARDK.Extensions.Scanning;

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ARDKExamples.Scanning
{
  public class ScanningExampleManager : MonoBehaviour
  {
    enum ScanningExampleState
    {
      SAVED_SCAN_LIST,
      SAVED_SCAN_SINGLE,
      NEW_SCAN,
      UPLOADING,
      SCAN_QUALITY
    }


    [SerializeField] [Tooltip("The scene's ARScanManager")]
    private ARScanManager _scanManager;

    [SerializeField] private ARSessionManager _arSessionManager;

    [SerializeField] [Tooltip("Container holding scanner control buttons.")]
    private GameObject _buttons;

    [SerializeField] private Text _statusText;

    [SerializeField] private GameObject _scannedObjectPrefab;

    [SerializeField] private Transform _scannedObjectParent;

    [SerializeField] private PointCloudVisualizer _pointCloudVisualizer;
#if ARDK_HAS_URP
    [SerializeField] private UrpRaycastScanVisualizer  _raycastScanVisualizer;
#else
    [SerializeField] private RaycastScanVisualizer _raycastScanVisualizer;
#endif
    [SerializeField] private WorldSpaceScanVisualizer _worldSpaceScanVisualizer;

    [SerializeField] private Slider _progressBar;

    private int _currentlyActiveVisualizer;
    private GameObject scannedObject;


    [SerializeField] private GameObject _savedScanGlobalRoot;

    [SerializeField] private GameObject _savedScanListParent;

    [SerializeField] private GameObject _savedScanPanelPrefab;

    [SerializeField] public GameObject _mockBackground;

    [SerializeField] private ScanNames _scanNames;

    [SerializeField] private GameObject _scanQualityPanel;
    [SerializeField] private Text _scanQualityMainText;
    [SerializeField] private Text _scanQualityDetailText;

    private string _currentPoiName;

    // Last state of the scanner:
    private IScanner.State lastScannerState = IScanner.State.Done;
    private ScanningExampleState exampleState = ScanningExampleState.NEW_SCAN;
    private ScanningExampleState lastExampleState = ScanningExampleState.NEW_SCAN;

    private List<IScanVisualizer> _enabledVisualizers;

    public void ScanButtonPressed()
    {
      _scanManager.StartScanning();
    }

    public void PauseButtonPressed()
    {
      _scanManager.PauseScanning();

    }

    public void ResumeButtonPressed()
    {
      _scanManager.ResumeScanning();
    }

    public void StopButtonPressed()
    {
      _scanManager.StopScanning();
    }

    public void ProcessButtonPressed()
    {
      _scanManager.StartProcessing();
    }

    public void RestartButtonPressed()
    {
      if (scannedObject != null)
      {
        Destroy(scannedObject.GetComponent<Renderer>().material.mainTexture);
        Destroy(scannedObject.GetComponent<MeshFilter>().sharedMesh);
        Destroy(scannedObject);
        scannedObject = null;
      }

      _scanManager.Restart();
    }

    private string getVisualizationButtonText()
    {
      if (ReferenceEquals(_enabledVisualizers[_currentlyActiveVisualizer], _raycastScanVisualizer))
      {
        return "Viz: Colors";
      }
      else if (ReferenceEquals(_enabledVisualizers[_currentlyActiveVisualizer], _worldSpaceScanVisualizer))
      {
        return "Viz: Normals";
      }
      else
      {
        return "Viz: Voxels";
      }
    }

    private void updateVisualizerSettings()
    {
      FindButton("VisButton").GetComponentInChildren<Text>().text = getVisualizationButtonText();
    }

    public void VisButtonPressed()
    {
      _currentlyActiveVisualizer++;
      _currentlyActiveVisualizer %= _enabledVisualizers.Count;
      _scanManager.SetVisualizer(_enabledVisualizers[_currentlyActiveVisualizer]);
      updateVisualizerSettings();
    }

    public void CancelProcessButtonPressed()
    {
      _scanManager.CancelProcessing();
    }

    public void SaveButtonPressed()
    {
      if (_scanNames != null && _currentPoiName != null)
      {
        _scanNames.SetScanName(_scanManager.GetScanId(), _currentPoiName + " " + DateTime.Now.ToString("MM/dd hh:mm"));
      }

      _scanManager.SaveCurrentScan();
      RestartButtonPressed();
    }

    private void DeleteScan(SavedScanPanel toDelete)
    {
      _scanManager.DeleteScan(toDelete.getScanId());
      Destroy(toDelete.gameObject);
    }


    private void UploadScan(SavedScanPanel toUpload)
    {
      exampleState = ScanningExampleState.UPLOADING;
      _scanManager.UploadScan(toUpload.getScanId(), (float progress) => { _progressBar.value = progress; },
        (bool success, string error) =>
        {
          exampleState = ScanningExampleState.NEW_SCAN;
          if (!success)
          {
            Debug.LogError(error);
          }
        });
    }

    public void ShowSavedScans()
    {
      exampleState = ScanningExampleState.SAVED_SCAN_LIST;

      List<string> scanIds = _scanManager.GetSavedScans();
      List<GameObject> toDestroy = new List<GameObject>();
      foreach (Transform child in _savedScanListParent.transform)
      {
        toDestroy.Add(child.gameObject);
      }

      foreach (GameObject oldItem in toDestroy)
      {
        Destroy(oldItem);
      }

      foreach (var scanId in scanIds)
      {
        GameObject storedScanPanelPrefab = Instantiate(_savedScanPanelPrefab, _savedScanListParent.transform);
        SavedScanPanel savedScanPanel = storedScanPanelPrefab.GetComponent<SavedScanPanel>();
        string displayText = scanId;
        bool canUpload = false;
        if (_scanNames != null && _scanNames.GetName(scanId) != null)
        {
          canUpload = true;
          displayText = _scanNames.GetName(scanId);
        }

        bool canCalulateScanQuality = _scanQualityPanel != null;

        Action uploadCallback = () => this.UploadScan(savedScanPanel);
        Action scanQualityCallback = () => this.CalculateScanQuality(savedScanPanel);

        savedScanPanel.init(scanId, displayText,
          () => this.ShowSavedScan(savedScanPanel),
          canCalulateScanQuality ? scanQualityCallback : null,
          () => this.DeleteScan(savedScanPanel),
          canUpload ? uploadCallback : null);
      }
    }

    public void HideSavedScans()
    {
      exampleState = ScanningExampleState.NEW_SCAN;
    }

    private void ShowSavedScan(SavedScanPanel toShow)
    {
      _statusText.text = "Viewing: " + toShow.getDisplayText();
      _savedScanListParent.gameObject.SetActive(false);
      SavedScan savedScan = _scanManager.GetSavedScan(toShow.getScanId());

      ShowTexturedMesh(savedScan.GetTexturedMesh());
      exampleState = ScanningExampleState.SAVED_SCAN_SINGLE;
    }

    public void HideSavedScan()
    {
      if (scannedObject != null)
      {
        Destroy(scannedObject.GetComponent<Renderer>().material.mainTexture);
        Destroy(scannedObject.GetComponent<MeshFilter>().sharedMesh);
        Destroy(scannedObject);
        scannedObject = null;
      }

      _savedScanListParent.gameObject.SetActive(true);
      exampleState = ScanningExampleState.SAVED_SCAN_LIST;
    }

    private void CalculateScanQuality(SavedScanPanel toCalculate)
    {
      Debug.Log("Quality start");
      _scanManager.GetScanQuality(toCalculate.getScanId(), (result) =>
      {
        exampleState = ScanningExampleState.SCAN_QUALITY;
        _scanQualityPanel.gameObject.SetActive(true);
        float roundedScore = Mathf.Round(result.ScanQualityScore * 100) / 100.0f;
        _scanQualityMainText.text = roundedScore > 0.5 ? "Pass: " + roundedScore : "Fail: " + roundedScore;
        string detailText = "Possible issues: ";
        var rejectedReasons = result.RejectionReasons;
        if (rejectedReasons.Count > 0)
        {
          for(int i = 0; i < rejectedReasons.Count; i++)
          {
            detailText += rejectedReasons[i].ToString() + (i != rejectedReasons.Count - 1 ? ", " : ".");
          }
        }
        else
        {
          detailText += "none detected.";
        }
        
        _scanQualityDetailText.text = detailText;
      });
    }

    public void HideScanQuality()
    {
      _scanQualityPanel.gameObject.SetActive(false);
      exampleState = ScanningExampleState.SAVED_SCAN_LIST;
    }

    private void ShowTexturedMesh(TexturedMesh texturedMesh)
    {
      if (scannedObject == null)
      {
        scannedObject = Instantiate(_scannedObjectPrefab, _scannedObjectParent);
      }

      Bounds meshBoundary = texturedMesh.mesh.bounds;
      scannedObject.transform.localPosition = -1 * meshBoundary.center;
      scannedObject.transform.localScale = Vector3.one / meshBoundary.extents.magnitude * 2;
      scannedObject.GetComponent<MeshFilter>().sharedMesh = texturedMesh.mesh;
      if (texturedMesh.texture != null)
        scannedObject.GetComponent<Renderer>().material.mainTexture = texturedMesh.texture;
    }

    private void Start()
    {
      _scanManager.ScanProcessed += ((args) =>
      {
        ShowTexturedMesh(args.TexturedMesh);
      });
      _enabledVisualizers = new List<IScanVisualizer>();
      _enabledVisualizers.Add(_raycastScanVisualizer);
      _enabledVisualizers.Add(_worldSpaceScanVisualizer);
      _enabledVisualizers.Add(_pointCloudVisualizer);
      _enabledVisualizers = _enabledVisualizers.Where(s => s != null).ToList();
      if (_enabledVisualizers.Count > 0)
      {
        _scanManager.SetVisualizer(_enabledVisualizers[0]);
        updateVisualizerSettings();
      }

      FindButton("VisButton").GetComponentInChildren<Text>().text = getVisualizationButtonText();
      _mockBackground.gameObject.SetActive(Application.isEditor);
    }

    private void Update()
    {
      UpdateUI();
    }

    private void UpdateUI()
    {
      IScanner.State scannerState = _scanManager.ScannerState;
      if (scannerState == IScanner.State.Processing)
      {
        _progressBar.value = _scanManager.GetScanProgress();
      }

      if (scannerState != lastScannerState || exampleState != lastExampleState)
      {
        if (exampleState == ScanningExampleState.NEW_SCAN)
        {
          _statusText.text = "" + scannerState;
          FindButton("ScanButton").SetActive(scannerState == IScanner.State.Ready);
          FindButton("PauseButton").SetActive(scannerState == IScanner.State.Scanning);
          FindButton("ResumeButton").SetActive(scannerState == IScanner.State.Paused);
          FindButton("SavedScanListButton").SetActive(scannerState == IScanner.State.Ready);
          FindButton("SavedScanBackToListButton").SetActive(false);
          _savedScanGlobalRoot.gameObject.SetActive(false);
          FindButton("StopButton").SetActive(scannerState == IScanner.State.Scanning);
          FindButton("VisButton").SetActive(scannerState == IScanner.State.Ready && _enabledVisualizers.Count >= 2);
          FindButton("ProcessButton").SetActive(scannerState == IScanner.State.ScanCompleted);
          FindButton("CancelProcessButton").SetActive(scannerState == IScanner.State.Processing);
          FindButton("RestartButton").SetActive(scannerState == IScanner.State.Done ||
                                                scannerState == IScanner.State.Cancelled ||
                                                scannerState == IScanner.State.Error);
          FindButton("SaveButton").SetActive(scannerState == IScanner.State.Done);
          _progressBar.gameObject.SetActive(scannerState == IScanner.State.Processing);
        }
        else if (exampleState == ScanningExampleState.UPLOADING)
        {
          _progressBar.gameObject.SetActive(true);
          _statusText.text = "Uploading";
          _savedScanGlobalRoot.GetComponent<CanvasGroup>().interactable = false;
        }
        else // list or single or sqc
        {
          if (exampleState == ScanningExampleState.SAVED_SCAN_LIST)
          {
            _statusText.text = "Saved scans";
          }

          _savedScanGlobalRoot.GetComponent<CanvasGroup>().interactable = true;
          FindButton("ScanButton").SetActive(false);
          FindButton("SavedScanListButton").SetActive(false);
          _progressBar.gameObject.SetActive(false);
          FindButton("SavedScanBackToListButton").SetActive(exampleState == ScanningExampleState.SAVED_SCAN_SINGLE);
          _savedScanGlobalRoot.gameObject.SetActive(exampleState == ScanningExampleState.SAVED_SCAN_LIST);
          if (_scanQualityPanel != null)
          {
            _scanQualityPanel.gameObject.SetActive(exampleState == ScanningExampleState.SCAN_QUALITY);
          }
        }

        lastExampleState = exampleState;
        lastScannerState = scannerState;
      }
    }

    private GameObject FindButton(string name)
    {
      foreach (Transform child in _buttons.transform)
      {
        if (child.gameObject.name == name)
          return child.gameObject;
      }

      return null;
    }

    public void StartScanningPoi(string poi, string poiName)
    {
      _scanManager.SetScanTargetId(poi);
      _scanManager.StartScanning();
      _currentPoiName = poiName;
    }
  }
}