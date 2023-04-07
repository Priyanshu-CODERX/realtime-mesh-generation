using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Niantic.LightshipHub.Tools
{
    public class Loading : MonoBehaviour
    {
        [HideInInspector]
        public GameObject Spinner;

        void Update()
        {
            if (Spinner != null && Spinner.activeSelf) 
                Spinner.transform.Rotate(0, 0, -200 * Time.deltaTime);
        }
    }
}
