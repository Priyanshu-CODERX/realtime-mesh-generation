using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Niantic.ARDK.AR.Scanning;
using Niantic.ARDK.Extensions.Scanning;

public class MeshScanManager : MonoBehaviour
{
    [Tooltip("This Scene's Scan Manager")]
    [SerializeField] private ARScanManager _scanManager;
    [SerializeField] private Slider _progressSlider;
    [SerializeField] private PointCloudVisualizer _pointCloudVisualizer;

    private GameObject scannedObject;
    [SerializeField] private GameObject _scannedObjectPrefab;
    [SerializeField] private Transform _scannedObjectParent;
    private void Start()
    {
        _scanManager.SetVisualizer(_pointCloudVisualizer);
        _scanManager.ScanProcessed += ((args) =>
        {
            ShowTexturedMesh(args.TexturedMesh);
        });
    }

    public void StartMeshScanning() => _scanManager.StartScanning();
    public void StopMeshScanning() => _scanManager.StopScanning();

    public void ProcessMeshScan()
    {
        IScanner.State state = _scanManager.ScannerState;
        if (state == IScanner.State.ScanCompleted)
        {
            _scanManager.StartProcessing();
        }
    }

    public void CancelMeshProcess() => _scanManager.CancelProcessing();

    private void Update()
    {
        IScanner.State state = _scanManager.ScannerState;

        if (state == IScanner.State.Processing)
        {
            _progressSlider.gameObject.SetActive(true);
            _progressSlider.value = _scanManager.GetScanProgress();
        }
        else
        {
            _progressSlider.gameObject.SetActive(false);
        }
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
}
