#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Tilemaps;
[ExecuteAlways]

public class TilemapGridProperties : MonoBehaviour
{
    private Tilemap tilemap;
    //private Grid grid;
    [SerializeField] private SO_GridProperties gridProperties = null;
    [SerializeField] private GridBoolProperty gridBoolProperty = GridBoolProperty.diggable;

    private void OnEnable()
    {
        if (!Application.IsPlaying(gameObject))//在编辑模式下执行
        {
            tilemap = GetComponent<Tilemap>();
            if (gridProperties != null)
            {
                gridProperties.gridPropertyList.Clear();
            }
        }
    }

    private void OnDisable()
    {
        if (!Application.IsPlaying(gameObject))//在编辑模式下执行
        {
            UpdateGridProperties();

            if (gridProperties != null)
            {
#if UNITY_EDITOR
                EditorUtility.SetDirty(gridProperties);//标记为脏，触发自动保存
#endif
            }
        }
    }

    private void UpdateGridProperties()
    {
        tilemap.CompressBounds();

        if (!Application.IsPlaying(gameObject))
        {
            if (gridProperties != null)
            {
                //设置网格边界
                Vector3Int startCell = tilemap.cellBounds.min;
                Vector3Int endCell = tilemap.cellBounds.max;

                for (int x = startCell.x; x < endCell.x; x++)
                {
                    for (int y = startCell.y; y < endCell.y; y++)
                    {
                        TileBase tile = tilemap.GetTile(new Vector3Int(x, y, 0));//获取当前单元格的tile
                        if (tile != null)//有瓦片则记录属性
                        {
                            gridProperties.gridPropertyList.Add(new GridProperty(new GridCoordinate(x, y), gridBoolProperty, true));
                        }
                    }
                }
            }
        }
    }
    private void Update()
    {
        if (!Application.IsPlaying(gameObject))
        {
            Debug.Log("禁用Property Tilemaps!");
        }
    }
}
