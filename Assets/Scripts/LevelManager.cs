/*--------------------------------------------------------------------------------*
  File Name: LevelManager.cs
  Authors: Nathaniel Thoma

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    public GameObject transitionsContainer;

    private SceneTransition[] transitions;
    private GameManager gameManager;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        transitions = transitionsContainer.GetComponentsInChildren<SceneTransition>();
        gameManager = FindFirstObjectByType<GameManager>();

        AudioSystem.Instance.PlayMusic(0, true);
        AudioSystem.Instance.SetBGMVolume(0.1f);
    }

    public void LoadScene(string sceneName, string transitionName)
    {
        StartCoroutine(LoadSceneAsync(sceneName, transitionName));
    }

    private IEnumerator LoadSceneAsync(string sceneName, string transitionName)
    {
        // Begin to load the scene async
        SceneTransition transition = transitions.First(t => t.name == transitionName);
        AsyncOperation scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false;

        // Start the transition
        yield return transition.AnimateTransitionIn();

        // Waiting for scene to load
        do
        {
            // maybe prog bar? bobber? Something during the black screen
            // scene.progress may be useful here
        }
        while (scene.progress < 0.9f);

        // Activate scene
        scene.allowSceneActivation = true;

        // End the transition
        yield return transition.AnimateTransitionOut();

        OnSceneTransitionComplete?.Invoke();
    }

    public static Action OnSceneTransitionComplete;
}
