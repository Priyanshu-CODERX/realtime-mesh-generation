using System;
using Niantic.ARDK.AR.WayspotAnchors;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Niantic.ARDKExamples.RemoteAuthoring
{
    [Serializable]
    public struct AnchoredContent
    {
        [FormerlySerializedAs("AnchorDataIdentifier")] [DisplayWithoutEdit]
        public string ReadableName;
        
        [HideInInspector]
        public int ManifestID;
        
        public Vector3 ContentScale;
        public GameObject Content;
        public AnchoredContent(int id, string anchorDataID, Vector3 scale, GameObject instancePrefab)
        {
            ManifestID = id;
            ReadableName = anchorDataID;
            ContentScale = scale;
            Content = instancePrefab;
        }
    }
}
