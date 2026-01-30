using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Respawn : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
              if (PlayerDeath.Instance.IsSpectatorEnding)
              {
                    SceneManager.LoadScene("Title");
                    Cursor.lockState = CursorLockMode.None;
              }
              else
              {
                   PlayerDeath.Instance.EnterSpectatorMode();
              }
        }
    }
    
          
        
}
