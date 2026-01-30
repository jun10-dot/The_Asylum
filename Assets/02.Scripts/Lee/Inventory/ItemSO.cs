using System.Collections;
using System.Collections.Generic;

using UnityEngine;

// 메뉴에 Scriptable 오브젝트를 생성할 수 있도록 추가
[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Object/ItemSO", order = int.MaxValue)]
public class ItemSO : ScriptableObject
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