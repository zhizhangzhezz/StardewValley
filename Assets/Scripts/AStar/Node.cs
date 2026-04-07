using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Node : IComparable<Node>
{
    public Vector2Int gridPosition;
    public int gCost = 0;//从起点的移动成本
    public int hCost = 0;//到终点的预估成本
    public bool isObstacle = false;
    public int movementPenalty;
    public Node parentNode;//父节点

    public Node(Vector2Int gridPosition)
    {
        this.gridPosition = gridPosition;
        parentNode = null;
    }

    //FCost属性
    public int FCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    //比较函数，用于比较开销
    public int CompareTo(Node other)
    {
        //优先FCost，相同则比较HCost
        int result = FCost.CompareTo(other.FCost);
        if (result == 0)
        {
            result = hCost.CompareTo(other.hCost);
        }
        return result;
    }
}
