using System;

using Niantic.ARDK.AR.WayspotAnchors;
using Niantic.ARDK.Extensions;

using UnityEngine;

namespace Niantic.ARDKExamples.RemoteAuthoring
{
  // Tracks the status of the associated WayspotAnchor and sets the visibility of the associated
  // game object to match the resolved status of the anchor.
  public class AnchorStatusTracker: WayspotAnchorTracker
  {
    public bool AnchorIsResolved
    {
      get
      {
        return
          WayspotAnchor != null &&
          (WayspotAnchor.Status == WayspotAnchorStatusCode.Success || WayspotAnchor.Status == WayspotAnchorStatusCode.Limited);
      }
    }

    private void Awake()
    {
      UpdateVisibility();
    }

    protected override void OnStatusCodeUpdated(WayspotAnchorStatusUpdate args)
    {
      base.OnStatusCodeUpdated(args);
      UpdateVisibility();

      Debug.Log($"Anchor {WayspotAnchor.ID.ToString().Substring(0, 10)} status update: {args.Code}");
    }

    private void UpdateVisibility()
    {
      gameObject.SetActive(AnchorIsResolved);
    }
  }
}
