using UnityEditor.SceneManagement;
using UnityEngine;

namespace Niantic.LightshipHub
{
  public class SamplesFactory : MonoBehaviour
  {
    public static void OpenSampleProject_ARHockey()
    {
      EditorSceneManager.OpenScene("Assets/LightshipHUB/SampleProjects/ARHockey.unity");
    }
  }
}
