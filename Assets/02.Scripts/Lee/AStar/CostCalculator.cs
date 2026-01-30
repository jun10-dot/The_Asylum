using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CostCalculator : IComparer<HexaNode>
{
    // FCost 비교
    public int Compare(HexaNode node1, HexaNode node2)
    {
        if (node1.FCost < node2.FCost)
            return -1;
        else if (node1.FCost > node2.FCost)
            return 1;
        else if (node1.FCost == node2.FCost)
        {
            if (node1.HCost < node2.HCost)
                return -1;
            else if (node1.HCost > node2.HCost)
                return 1;
        }

        return 0;
    }
}
