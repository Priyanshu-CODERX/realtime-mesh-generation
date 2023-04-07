// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Niantic.ARDK.Utilities.Input.Legacy;

namespace Niantic.LightshipHub.Templates
{
  public class PhysicsSceneController : MonoBehaviour
  {
    private List<GameObject> objectHolders = new List<GameObject>();
    private int objCount, maxObjects = 15;
    private bool isCooling = false;

    private void Start()
    {
      SetObjectHolders();
    }

    void Update()
    {
      if (PlatformAgnosticInput.touchCount <= 0) return;
      var touch = PlatformAgnosticInput.GetTouch(0);
      if (objCount <= maxObjects && !isCooling)
      {
        if (touch.phase == TouchPhase.Began) TouchBegan(touch);
      }
    }
    private void TouchBegan(Touch touch)
    {
      GameObject objectHolder = ActivateOneOH();
      LaunchObjectHolder(objectHolder);
      objCount++;
      isCooling = true;
      StartCoroutine(CoolDownSpawn());
      StartCoroutine(DeactivateObject(objectHolder));
    }

    private void SetObjectHolders()
    {
      for (int i = 0; i < maxObjects/3; i++)
      {
        foreach (Transform child in gameObject.transform)
        {
          if (child.gameObject.name == "PreloadManager") continue;
          if (child.gameObject.activeSelf) child.gameObject.SetActive(false);

          GameObject newObj = Instantiate(child.gameObject);
          var cursor = newObj.transform.Find("cursor");
          if (cursor != null) Destroy(cursor.gameObject);
          newObj.SetActive(false);
          objectHolders.Add(newObj.gameObject);
        }
      }
    }

    private GameObject ActivateOneOH()
    {
      for (int i = 0; i < objectHolders.Count; i++)
      {
        int rd = Random.Range(0, maxObjects);
        if (!objectHolders[rd].activeSelf) return objectHolders[rd];
      } return objectHolders[0];
    }

    private void LaunchObjectHolder(GameObject obj)
    {
      obj.TryGetComponent<PhysicsController>(out PhysicsController physicsController);
      physicsController.Launch();
    }
    private IEnumerator DeactivateObject(GameObject obj)
    {
      yield return new WaitForSeconds(Random.Range(3, 8));
      obj.SetActive(false);
      objCount--;
    }

    private IEnumerator CoolDownSpawn()
    {
      yield return new WaitForSeconds(.4f);
      isCooling = false;
    }
  }
}