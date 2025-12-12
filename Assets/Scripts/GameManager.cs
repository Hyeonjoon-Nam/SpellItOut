/*--------------------------------------------------------------------------------*
  File Name: GameManager.cs
  Authors: Nathaniel Thoma

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int currentLevel = 0;

    // saved player position when leaving the dungeon
    public Vector3 savedPlayerPosition;
    public bool hasSavedPlayerPos = false;

    // full dungeon save data
    public SavedDungeon savedDungeon = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LevelManager.OnSceneTransitionComplete += PlacePlayerAfterTransition;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlacePlayerAfterTransition()
    {
        if (!hasSavedPlayerPos) return;

        GameObject player = GameObject.FindWithTag("Player");
        if (player)
        {
            var cc = player.GetComponent<CharacterController>();
            cc.enabled = false;
            player.transform.position = savedPlayerPosition;
            cc.enabled = true;
        }
    }

    public void DestroyDungeon()
    {
        savedDungeon.ClearRooms();
        savedDungeon = null;
        hasSavedPlayerPos = false;
    }
}