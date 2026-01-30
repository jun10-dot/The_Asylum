using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    private List<Item> itemList = new List<Item>(10);    // 아이템들을 저장할 리스트
    public GameObject outLinePrefab;

    public GameObject prevObj;
    public GameObject curObj;

    private GameObject itemLayout;

    public PhotonView invenPv;

    public ToastManager toastManager;

    public List<Item> ItemList
    {
        
        get
        {
            return itemList;
        }
    }

    void Awake()
    {
        itemLayout = GameObject.Find("ItemLayout");
        invenPv = GetComponent<PhotonView>();
        toastManager = GameObject.Find("ToastManager").GetComponent<ToastManager>();
    }

    void Start()
    {
        prevObj = null;
        curObj = null;
        itemList.Clear();
    }

    void Update()
    {
        // 입력키에 따른 아이템 선택 처리
        switch (Input.inputString)
        {
            case "1": SelectItem(0); break;
            case "2": SelectItem(1); break;
            case "3": SelectItem(2); break;
            case "4": SelectItem(3); break;
            case "5": SelectItem(4); break;
            case "6": SelectItem(5); break;
            case "7": SelectItem(6); break;
            case "8": SelectItem(7); break;
            case "9": SelectItem(8); break;
            case "0": SelectItem(9); break;
            default: return;
        }
    
    }

    // 인벤토리 상황을 공유하도록 PhotonView 동기화
    [PunRPC]
    public void AddItem(string name)
    {
        ItemSO itemSo = Resources.Load<ItemSO>("Scriptable Objects" + "/Token" + name);
        if (itemList.Count >= 10)  // 인벤토리에 아이템 10개 초과 시 아이템 획득 불가
        {
            // Debug.Log("Item FULL!!!");
            toastManager.ShowToast("아이템을 더 주울 수 없습니다!", 1f);
            return;
        }

        GameObject newItem = new GameObject(itemSo.ItemName);
        newItem.transform.parent = itemLayout.transform;

        GameObject outLine = Instantiate(outLinePrefab);  // 아이템이 선택되었을 때 표시할 외각선 추가
        outLine.GetComponent<Transform>().SetParent(newItem.transform);
        outLine.GetComponent<Image>().enabled = false;

        Image sprite = newItem.AddComponent<Image>();   // 인벤토리에 아이템이 표시되도록 이미지 변경
        sprite.sprite = itemSo.ItemImg;

        Item item = newItem.AddComponent<Item>();  // Scriptable Object 데이터 값 전달     
        item.ItemName = itemSo.ItemName;
        item.ItemImg = itemSo.ItemImg;

        toastManager.ShowToast("토큰을 획득했습니다!", 1f);
        itemList.Add(item);    // Scriptable Object에 있던 정보들을 저장하여 리스트에 추가
        itemList.TrimExcess(); // 리스트 재정리
    }

    [PunRPC]
    public void UseItem(string name)
    {
        if (itemList.Find(items => items.ItemName == name) == null)   // 사용할 아이템이 없는 경우 처리
        {
            Debug.Log("No Item to Use");
            return;
        }

        GameObject temp = GameObject.Find(name);    // 아이템 오브젝트 삭제
        Destroy(temp);

        itemList.Remove(itemList[itemList.FindIndex(items => items.ItemName.Equals(name))]); // 리스트에서 삭제
        itemList.TrimExcess();     // 리스트 재정리
    }

    public void SelectItem(int index)
    {
        if (index < 0 || index >= 10)
        {
            Debug.Log("Out of Range");
            return;
        }

        try
        {
            curObj = itemList[index].gameObject;

            if (prevObj == null)    // 선택한 아이템이 없는 경우
            {
                prevObj = curObj;
                curObj.transform.GetChild(0).GetComponent<Image>().enabled = true;
                return;
            }
            if (prevObj == curObj)  // 선택한 아이템을 다시 선택한 경우
            {
                curObj.transform.GetChild(0).GetComponent<Image>().enabled = false;
                curObj = null;
                prevObj = null;
            }
            else                    // 아이템 선택 후 다른 아이템을 선택한 경우
            {
                SwitchItem(curObj.GetComponent<Transform>(), prevObj.GetComponent<Transform>());
                prevObj.transform.GetChild(0).GetComponent<Image>().enabled = false;
                curObj = null;
                prevObj = null;
            }
        }

        catch (ArgumentOutOfRangeException ex)
        {
            //Debug.Log("중복 처리 방지");
        }

        catch(NullReferenceException ex)
        {
            //Debug.Log("중복 처리 방지");
        }

    }

    public void SwitchItem(Transform item1, Transform item2)  // 아이템 위치 swap
    {
        // 리스트 swap
        Item tmp1 = itemList[item1.GetSiblingIndex()];
        Item tmp2 = itemList[item2.GetSiblingIndex()];
        
        itemList.RemoveAt(item1.GetSiblingIndex());
        itemList.Insert(item1.GetSiblingIndex(), tmp2);

        itemList.RemoveAt(item2.GetSiblingIndex());
        itemList.Insert(item2.GetSiblingIndex(), tmp1);

        // 오브젝트 swap
        int temp = item1.GetSiblingIndex();
        item1.SetSiblingIndex(item2.GetSiblingIndex());
        item2.SetSiblingIndex(temp);
    }

    public void CheckItem()
    {
        foreach (Item temp in itemList)
        {
            Debug.Log(temp.ItemName);
        }
        Debug.Log("Done");
    }
}