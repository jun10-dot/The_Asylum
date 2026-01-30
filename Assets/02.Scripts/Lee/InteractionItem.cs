using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PhotonView))]
public class InteractionItem : MonoBehaviour, IClickable
{

    private PhotonView pv;

    private PhotonView invenPv;
    private Inventory inventory; 
    private int tokenCount = 0;
    private SoundManager sMgr;

    void Awake()
    {
        inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();
        invenPv = inventory.gameObject.GetComponent<PhotonView>();
        sMgr = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        pv = GetComponent<PhotonView>();
    }
  
    public void OnClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (gameObject.tag == "MainDoor")
            {
                if (inventory.curObj == null)
                {
                    return;
                }
                invenPv.RPC("UseItem", PhotonTargets.All, inventory.curObj.name);
                // 마스터만 카운트를 증가시키도록 구현
                pv.RPC("TokenCount", PhotonTargets.All);
                if(tokenCount == 10)
                {
                    tokenCount = 0;
                    pv.RPC("EndingScene", PhotonTargets.All);
                }
            }
            else if(gameObject.tag == "Key")
            {
                if(inventory.ItemList.Count >= 10)
                    return;
                invenPv.RPC("AddItem", PhotonTargets.All, gameObject.name);
                sMgr.PlayEffct(gameObject.transform.position, sMgr.sfxClips[17]);
                pv.RPC("DeactivateItem", PhotonTargets.All);
            }         
        }
    }


    [PunRPC]
    void DeactivateItem()
    {
        if (gameObject.tag == "MainDoor")
            return;
        if(inventory.ItemList.Count > 10 && gameObject.tag == "Key")
            return;
        if(gameObject.tag == "Hint")
            return;
        gameObject.SetActive(false);
    }

    [PunRPC]
    void EndingScene()
    {
        SceneManager.LoadScene("Ending");
    }

    [PunRPC]
    void TokenCount()
    {
        tokenCount++;
    }
}