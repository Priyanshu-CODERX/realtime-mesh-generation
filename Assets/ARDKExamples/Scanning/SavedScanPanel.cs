using System;
using System.Collections;
using System.Collections.Generic;
using Niantic.ARDK.AR.Scanning;
using UnityEngine;
using UnityEngine.UI;

namespace ARDKExamples.Scanning
{
  public class SavedScanPanel : MonoBehaviour
  {
    [SerializeField] private Text scanIdText;

    [SerializeField] private GameObject uploadButton;
    [SerializeField] private GameObject qualityButton;

    private string scanId;
    private string displayString;
    private Action viewCallback;
    private Action deleteCallback;
    private Action uploadCallback;
    private Action qualityCallback;

    public void init(string scanId, string displayString, Action viewCallback, Action qualityCallback, Action deleteCallback,
      Action uploadCallback)
    {
      this.scanId = scanId;
      this.displayString = displayString;
      scanIdText.text = displayString;
      this.viewCallback = viewCallback;
      this.deleteCallback = deleteCallback;
      this.uploadCallback = uploadCallback;
      this.qualityCallback = qualityCallback;
      uploadButton.SetActive(uploadCallback != null);
      qualityButton.SetActive(qualityCallback != null);
    }

    public void OnViewButtonClicked()
    {
      viewCallback?.Invoke();
      ;
    }

    public void OnDeleteButtonClicked()
    {
      deleteCallback?.Invoke();
    }

    public void OnUploadButtonClicked()
    {
      uploadCallback?.Invoke();
    }

  public void OnQualityButtonClicked()
  {
    qualityCallback?.Invoke();
  }

    public string getScanId()
    {
      return scanId;
    }

    public string getDisplayText()
    {
      return displayString;
    }
  }
}