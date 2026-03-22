using System.Collections.Generic;
using UnityEngine;

public static class HelperMethods
{
    public static bool GetComponentsAtBoxLocation<T>(out List<T> listComponentsAtBoxLocation, Vector2 point, Vector2 size, float angle)
    {
        bool found = false;
        List<T> componentList = new List<T>();

        //获取所有范围内类型为T的物体
        Collider2D[] collider2DArray = Physics2D.OverlapBoxAll(point, size, angle);
        foreach (Collider2D collider in collider2DArray)
        {
            T component = collider.gameObject.GetComponentInParent<T>();
            if (component != null)
            {
                found = true;
                componentList.Add(component);
            }
            else
            {
                component = collider.gameObject.GetComponentInChildren<T>();
                if (component != null)
                {
                    found = true;
                    componentList.Add(component);
                }
            }
        }

        listComponentsAtBoxLocation = componentList;
        return found;
    }
}
