using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ARDKExamples.Scanning
{
  public class ScanNames : MonoBehaviour
  {
    [Serializable]
    private class ScanName
    {
      public string ScanId;
      public string Name;

      public ScanName(string scanId, string scanName)
      {
        this.ScanId = scanId;
        this.Name = scanName;
      }
    }

    [Serializable]
    private class ScanNameRecord
    {
      public List<ScanName> ScanNames;

      public ScanNameRecord()
      {
        ScanNames = new List<ScanName>();
      }
    }

    private bool dirty;

    private ScanNameRecord _scanNameRecord;

    // Start is called before the first frame update
    void Start()
    {
      if (File.Exists(Application.persistentDataPath + "/scanNames.json"))
      {
        _scanNameRecord =
          JsonUtility.FromJson<ScanNameRecord>(
            File.ReadAllText(Application.persistentDataPath + "/scanNames.json"));
      }
      else
      {
        _scanNameRecord = new ScanNameRecord();
      }
    }

    // Update is called once per frame
    void Update()
    {
      if (dirty)
      {
        File.WriteAllText(Application.persistentDataPath + "/scanNames.json", JsonUtility.ToJson(_scanNameRecord));
        dirty = false;
      }
    }

    public string GetName(string scanId)
    {
      foreach (var scan in _scanNameRecord.ScanNames)
      {
        if (scan.ScanId == scanId)
        {
          return scan.Name;
        }
      }

      return null;
    }

    public void SetScanName(string scanId, string scanName)
    {
      this._scanNameRecord.ScanNames.RemoveAll(scan => scan.ScanId == scanId);
      this._scanNameRecord.ScanNames.Add(new ScanName(scanId, scanName));
      dirty = true;
    }
  }
}