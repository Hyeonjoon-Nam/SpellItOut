/*--------------------------------------------------------------------------------*
  File Name: CombatTriggered.cs
  Authors: Nathaniel Thoma

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public class CombatTrigger : MonoBehaviour
{
    private BoxCollider bc;
    private Serializer serializer;
    private GameObject enemy;

    public Door doorToUnlock;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bc = GetComponent<BoxCollider>();
        serializer = FindFirstObjectByType<Serializer>();

        //GenerateRooms roomGenerator = FindFirstObjectByType<GenerateRooms>();
        //enemy = Instantiate(roomGenerator.enemyPrefab, gameObject.transform.position, gameObject.transform.rotation);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("I hit: " + other.name);
        Debug.Log("My BoxCollider: " + bc);
        Debug.Log("Their Collider: " + other);

        if (other.GetComponent<PlayerController>())
        {
            GameManager.Instance.savedPlayerPosition = other.transform.position;
            GameManager.Instance.hasSavedPlayerPos = true;
            GameManager.Instance.savedDungeon = serializer.SaveDungeon();

            LevelManager.instance.LoadScene("FightScene", "CrossFade");
            AudioSystem.Instance.PlayMusic(1);
        }
    }
}
