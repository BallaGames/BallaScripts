using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Intended to be used to load from the init scene to the menu scene.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public SceneReference menuScene;
    void Start()
    {
        SceneManager.LoadScene(menuScene.BuildIndex);
    }
}
