using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    private string itemName;
    [SerializeField]
    private Sprite itemImg;

    public string ItemName
    {
        get
        {
            return itemName;
        }
        set
        {
            itemName = value;
        }
    }

    public Sprite ItemImg
    {
        get
        {
            return itemImg;
        }
        set
        {
            itemImg = value;
        }
    }
}
