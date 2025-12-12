/*--------------------------------------------------------------------------------*
  File Name: MainMenu.cs
  Authors: Nathaniel Thoma

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        //SceneManager.LoadScene("GameScene");
        LevelManager.instance.LoadScene("GameScene", "CrossFade");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void GoMainMenu()
    {
        LevelManager.instance.LoadScene("MainMenu", "CrossFade");
        GameManager.Instance.DestroyDungeon();
    }
}
