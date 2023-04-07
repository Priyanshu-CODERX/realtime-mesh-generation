using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ARDKExamples.Scanning
{
  public class ScannedObjectControl : MonoBehaviour
  {
    // Start is called before the first frame update
    public Transform updatedTransform;
    private Transform _relativeCameraTransform;
    private Dictionary<int, Vector2> _trackedInputs;
    private Vector3 localRotation;

    void Start()
    {
      _trackedInputs = new Dictionary<int, Vector2>();
      _relativeCameraTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
      if (Input.mousePresent)
      {
        if (Input.GetMouseButton(0))
        {
          if (_trackedInputs.Count == 1)
          {
            Vector2 delta = (Vector2)Input.mousePosition - _trackedInputs[0];
            ApplyRotationDelta(delta);
          }

          _trackedInputs[0] = Input.mousePosition;
        }
        else
        {
          _trackedInputs.Clear();
        }
      }
      else if (Input.touchSupported)
      {
        if (_trackedInputs.Count == 1 && Input.touchCount == 1)
        {
          Touch touch = Input.GetTouch(0);
          if (_trackedInputs.ContainsKey(touch.fingerId))
          {
            Vector2 delta = touch.position - _trackedInputs[touch.fingerId];
            ApplyRotationDelta(delta);
          }
        }
        else if (_trackedInputs.Count == 2 && Input.touchCount == 2)
        {
          Vector2[] inputs = _trackedInputs.Values.ToArray();
          Vector2 delta0 = inputs[0] - Input.GetTouch(0).position;
          Vector2 delta1 = inputs[1] - Input.GetTouch(1).position;
          if (Vector2.Angle(delta0, delta1) < 45f)
          {
            // 2 fingers moving in the same direction is a translation
            Vector2 totalMove = (delta0 + delta1) / 300f;
            updatedTransform.localPosition -= new Vector3(totalMove.x, totalMove.y, 0);
          }
          else
          {
            // 2 fingers moving in the opposite direction is a zoom
            float previousDistance = Vector2.Distance(inputs[1], inputs[0]);
            float currentDistance = Vector2.Distance
              (Input.GetTouch(1).position, Input.GetTouch(0).position);

            float scaling = Mathf.Max(0.2f, currentDistance / previousDistance);
            updatedTransform.localScale *= scaling;
          }
        }

        _trackedInputs.Clear();
        for (int i = 0; i < Input.touchCount; i++)
        {
          Touch touch = Input.GetTouch(i);
          _trackedInputs.Add(i, touch.position);
        }
      }
    }

    private void ApplyRotationDelta(Vector2 screenInputDelta)
    {
      float xAmount = screenInputDelta.x * 0.2f;
      float yAmount = screenInputDelta.y * 0.2f;
      localRotation = new Vector3
      (Mathf.DeltaAngle(0, localRotation.x + yAmount),
        Mathf.DeltaAngle(0, localRotation.y - xAmount),
        localRotation.z);
      updatedTransform.localRotation =
        Quaternion.Euler(localRotation.x, 0, 0) * Quaternion.Euler(0, localRotation.y, 0);
    }
  }
}