using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeInit : MonoBehaviour
{
    private GameObject[][] nodeAry = new GameObject[4][];
    
    void Start()
    {
        nodeAry[0] = new GameObject[7];
        nodeAry[1] = new GameObject[6];
        nodeAry[2] = new GameObject[7];
        nodeAry[3] = new GameObject[6];
        int rowCount = 0;
        int colCount = 0;

        foreach (Transform temp in GetComponentsInChildren<Transform>())
        {
            if (!temp.name.Contains("Node") || temp.name.Contains("s"))
                continue;
            if (temp.transform != this.transform)
            {
                nodeAry[rowCount][colCount++] = temp.gameObject;

                if ((rowCount == 0 || rowCount == 2) && colCount == 7)
                {
                    rowCount++;
                    colCount = 0;
                }

                else if (rowCount % 2 != 0 && colCount == 6)
                {
                    rowCount++;
                    colCount = 0;
                }

                if (rowCount == 4)
                    break;
            }
        }
        GetComponent<AStar>().GetAry(ref nodeAry);  // 노드오브젝트를 담은 배열 전달
        GetComponent<AStar>().searchPath = true;    // 경로를 찾도록 스위치 on
    }
}