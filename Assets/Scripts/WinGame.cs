/*--------------------------------------------------------------------------------*
  File Name: WinGame.cs
  Authors: Nathaniel Thoma

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class WinGame : MonoBehaviour
{
    private BoxCollider bc;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bc = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LevelManager.instance.LoadScene("WinScene", "CrossFade");
        }
    }
}
