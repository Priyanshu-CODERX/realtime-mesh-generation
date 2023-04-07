#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Niantic.LightshipHub
{
  public class LightshipUIFactory : MonoBehaviour
  {
    public static void CreateUIComponent(string prefabPath)
    {
      Canvas currentCanvas = CheckCanvas();
      GameObject uIPrefab = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath), currentCanvas.gameObject.transform);
      PrefabUtility.UnpackPrefabInstance(uIPrefab, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
    }

    private static Canvas CheckCanvas()
    {
      Canvas canvas = FindObjectOfType<Canvas>();
      if (canvas != null) return canvas;
      else
      {
        GameObject canvasGO = new GameObject("Canvas");
        Canvas newCanvas = canvasGO.AddComponent<Canvas>();
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();
        newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        return newCanvas;
      }
    }
  }
}
#endif