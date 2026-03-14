﻿﻿﻿﻿﻿﻿﻿using UnityEngine;
using Cinemachine;

public class SwitchConfineBoundingShape : MonoBehaviour
{
    void Start()
    {
        SwitchBoundingShape();
    }

    private void SwitchBoundingShape()
    {
        GameObject confinerObj = GameObject.FindGameObjectWithTag(Tags.BoundsConfiner);
        if (confinerObj == null)
        {
            Debug.LogError("找不到标签为 " + Tags.BoundsConfiner + " 的物体！");
            return;
        }

        PolygonCollider2D polygonCollider2D = confinerObj.GetComponent<PolygonCollider2D>();
        if (polygonCollider2D == null)
        {
            Debug.LogError("BoundsConfiner 物体上没有 PolygonCollider2D 组件！");
            return;
        }

        CinemachineConfiner cinemachineConfiner = GetComponent<CinemachineConfiner>();
        if (cinemachineConfiner == null)
        {
            Debug.LogError("Virtual Camera 上没有 CinemachineConfiner 组件！请先添加扩展。");
            return;
        }

        cinemachineConfiner.m_BoundingShape2D = polygonCollider2D;
        cinemachineConfiner.InvalidatePathCache();
    }
}
