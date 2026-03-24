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

    //获取鼠标位置的物品
    public static bool GetComponentAtCursorLocation<T>(out List<T> componentsAtPositionList, Vector3 positionToCheck)
    {
        bool found = false;
        componentsAtPositionList = new List<T>();

        Collider2D[] colliders = Physics2D.OverlapPointAll(positionToCheck);
        foreach (Collider2D collider in colliders)
        {
            T component = collider.gameObject.GetComponentInParent<T>();
            if (component != null)
            {
                found = true;
                componentsAtPositionList.Add(component);
            }
            else
            {
                component = collider.gameObject.GetComponentInChildren<T>();
                if (component != null)
                {
                    found = true;
                    componentsAtPositionList.Add(component);
                }
            }
        }

        return found;
    }

    //获取所有范围内类型为T的物体(不分配新内存)
    public static T[] GetComponentsAtBoxLocationNonAlloc<T>(int numberOfCollidersToTest, Vector2 point, Vector2 size, float angle)
    {
        Collider2D[] collider2DArray = new Collider2D[numberOfCollidersToTest];
        Physics2D.OverlapBoxNonAlloc(point, size, angle, collider2DArray);
        T tComponent = default(T);

        T[] componentArray = new T[collider2DArray.Length];
        for (int i = 0; i < collider2DArray.Length; i++)
        {
            if (collider2DArray[i] != null)
            {
                tComponent = collider2DArray[i].gameObject.GetComponent<T>();
                if (tComponent != null)
                {
                    componentArray[i] = tComponent;
                }

            }
        }
        return componentArray;
    }
}
