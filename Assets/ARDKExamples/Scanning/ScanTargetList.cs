// Copyright 2022 Niantic, Inc. All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Niantic.ARDK;
using Niantic.ARDK.AR.Scanning;
using Niantic.ARDK.Configuration;
using Niantic.ARDK.Extensions.Scanning;
using Niantic.ARDK.LocationService;
using Niantic.ARDK.VirtualStudio.VpsCoverage;
using Niantic.ARDK.VPSCoverage;

using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

using LocationServiceStatus = Niantic.ARDK.LocationService.LocationServiceStatus;

namespace ARDKExamples.Scanning
{
  public class ScanTargetList: MonoBehaviour
  {
    
    [Header("Runtime Settings")]
    [SerializeField]
    [Tooltip("Request Scan targets from server (Live Device) or from mock responses")]
    private RuntimeEnvironment _runtimeEnvironment = RuntimeEnvironment.Default;
    
    [SerializeField]
    [Tooltip("These responses are used in mock mode")]
    private ScanTargetResponse _mockResponse;
    
    [SerializeField]
    [Tooltip("GPS used in Editor")]
    // Default is the Ferry Building in San Francisco
    private LatLng _spoofLocation = new LatLng(37.79531921750984, -122.39360429639748);

    [Header("ScrollList Setup")]
    [SerializeField]
    [Tooltip("The scroll list holding the items for each target information")]
    private ScrollRect _scrollList;

    [SerializeField]
    [Tooltip("Template item for target information")]
    private GameObject _itemPrefab;

    [SerializeField]
    [Tooltip("Targets items above this GameObject will have details downloaded")]
    private RectTransform _loadMoreItemsThreshold;

    [Tooltip("Target details can be downloaded in batches")]
    [SerializeField]
    [Min(1)]
    private int _loadingBatchSize = 4;

    [SerializeField] private GameObject closeButton;
    [SerializeField] private Text errorText;

    private IScanTargetClient _scanTargetClient;
    private ILocationService _locationService;

    private readonly List<GameObject> _items = new List<GameObject>();
    private readonly List<ScanTarget> _scanTargets = new List<ScanTarget>();
    private readonly Dictionary<string, ScanTarget> _scanTargetsById = new Dictionary<string, ScanTarget>();
    private GameObject _scrollListContent;

    private List<string> _targetIds = new List<string>();
    private int _nextItemToLoad = 0;

    private LatLng _requestLocation;
    private int _queryRadius;

    [SerializeField]
    private ScanningExampleManager _scanningExampleManager;
    
    void Start()
    {
      // This is necessary for setting the user id associated with the current user. 
      // We strongly recommend generating and using User IDs. Accurate user information allows
      //  Niantic to support you in maintaining data privacy best practices and allows you to
      //  understand usage patterns of features among your users.  
      // ARDK has no strict format or length requirements for User IDs, although the User ID string
      //  must be a UTF8 string. We recommend avoiding using an ID that maps back directly to the
      //  user. So, for example, don’t use email addresses, or login IDs. Instead, you should
      //  generate a unique ID for each user. We recommend generating a GUID.
      // When the user logs out, clear ARDK's user id with ArdkGlobalConfig.ClearUserIdOnLogout

      //  Sample code:
      //  // GetCurrentUserId() is your code that gets a user ID string from your login service
      //  var userId = GetCurrentUserId(); 
      //  ArdkGlobalConfig.SetUserIdOnLogin(userId);
      
      _locationService = LocationServiceFactory.Create();

      _scanTargetClient = ScanTargetClientFactory.Create(_runtimeEnvironment, _mockResponse);
      _scrollListContent = _scrollList.content.gameObject;

#if UNITY_EDITOR
      var spoofService = (SpoofLocationService)_locationService;
      spoofService.SetLocation(_spoofLocation.Latitude, _spoofLocation.Longitude);
#endif

      _scrollList.onValueChanged.AddListener(OnScroll);
      _queryRadius = 100;

    }

    private void Update()
    {
      if (_locationService.Status != LocationServiceStatus.Running)
        _locationService.Start();
    }

    public void RequestAreas(bool useSpoof)
    {
      ClearListContent();
      errorText.gameObject.SetActive(false);

      if (useSpoof)
      {
        _requestLocation = _spoofLocation;
        _scanTargetClient.RequestScanTargets(_spoofLocation, _queryRadius, OnScanTargetsReceived);
      }
      else
      {
        _requestLocation = new LatLng(_locationService.LastData);
        _scanTargetClient.RequestScanTargets(_locationService.LastData.Coordinates, _queryRadius, OnScanTargetsReceived);
      }

      closeButton.SetActive(true);
    }

    private void OnScanTargetsReceived(ScanTargetResponse scanTargets)
    {
      if (scanTargets.status == ResponseStatus.Success)
      {
        FillScrollList(scanTargets.scanTargets);
        ResizeListContent();
        if (scanTargets.scanTargets.Count == 0)
        {
          errorText.gameObject.SetActive(true);
          errorText.text = "No scan targets nearby";
        }
        else
        {
          errorText.gameObject.SetActive(false);
        }
      }
      else
      {
        errorText.gameObject.SetActive(true);
        errorText.text = "Error with request: " + scanTargets.status;
      }
    }

    private void LoadNextTargetDetails(int batchSize)
    {
      int count = Mathf.Clamp(_targetIds.Count - _nextItemToLoad, 0, batchSize);
      string[] targetIds = _targetIds.GetRange(_nextItemToLoad, count).ToArray();
      int itemIndex = _nextItemToLoad;
      _nextItemToLoad += batchSize;
      foreach (string targetId in targetIds)
      {
        ScanTarget target = _scanTargetsById[targetId];
        FillTargetItem(itemIndex, target);
        itemIndex++;
      }
    }
    
    private bool IsUnloadedItemAboveThreshold()
    {
      return _nextItemToLoad < _targetIds.Count &&
        _items[_nextItemToLoad].transform.position.y > _loadMoreItemsThreshold.position.y;
    }

    private void OnScroll(Vector2 scrollDirection)
    {
      while (IsUnloadedItemAboveThreshold())
        LoadNextTargetDetails(_loadingBatchSize);
    }

    public void ClearListContent()
    {
      foreach (GameObject item in _items)
        Destroy(item);

      _scanTargets.Clear();
      _scanTargetsById.Clear();
      _targetIds.Clear();
      _items.Clear();
      _nextItemToLoad = 0;
    }


    private void FillScrollList(List<ScanTarget> result)
    {
      ClearListContent();
      _scanTargets.AddRange(result);

      _scanTargets.Sort
      (
        (a, b) => a.Centroid.Distance(_requestLocation)
          .CompareTo(b.shape[0].Distance(_requestLocation))
      );

      foreach (var target in _scanTargets)
      {
        _targetIds.Add(target.scanTargetIdentifier);
        GameObject newTargetItem = Instantiate(_itemPrefab, _scrollListContent.transform, false);
        if (target.localizabilityStatus == ScanTarget.ScanTargetLocalizabilityStatus.EXPERIMENTAL)
        {
          newTargetItem.GetComponent<Image>().color = new Color(1, 0.9409157f, 0.6933962f);
        }
        else if (target.localizabilityStatus == ScanTarget.ScanTargetLocalizabilityStatus.NOT_ACTIVATED)
        {
          newTargetItem.GetComponent<Image>().color = new Color(1, 0.35f, 0.4f);
        }
        _scanTargetsById.Add(target.scanTargetIdentifier, target);
        _items.Add(newTargetItem);
      }
    }

    private void ResizeListContent()
    {
      VerticalLayoutGroup layout = _scrollListContent.GetComponent<VerticalLayoutGroup>();
      RectTransform contentTransform = _scrollListContent.GetComponent<RectTransform>();
      float itemHeight = _itemPrefab.GetComponent<RectTransform>().sizeDelta.y;
      contentTransform.sizeDelta = new Vector2
      (
        contentTransform.sizeDelta.x,
        layout.padding.top + _scrollListContent.transform.childCount * (layout.spacing + itemHeight)
      );

      // Scroll all the way up
      contentTransform.anchoredPosition = new Vector2(0, Int32.MinValue);
    }
    
    private void FillTargetItem(int itemIndex, ScanTarget target)
    {
      Transform itemTransform = _items[itemIndex].transform;
      itemTransform.name = target.name;

      Transform image = itemTransform.Find("Image");
      target.DownloadImage(downLoadedImage =>
      {
        if (image != null && image.GetComponent<RawImage>() != null)
        {
          image.GetComponent<RawImage>().texture = downLoadedImage;
        }
      });

      Transform title = itemTransform.Find("Info/Title");
      title.GetComponent<Text>().text = target.name;

      Transform button = itemTransform.Find("Info/Button");
      button.GetComponent<Button>()
        .onClick.AddListener
          (delegate { StartScanningPoi(target); });
    }

    private void StartScanningPoi(ScanTarget target)
    {
      ClearListContent();
      _scanningExampleManager.StartScanningPoi(target.scanTargetIdentifier, target.name);
    }

   
  }
}
