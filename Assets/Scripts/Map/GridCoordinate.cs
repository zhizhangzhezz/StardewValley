using UnityEngine;
[System.Serializable]
public class GridCoordinate
{
    public int x;
    public int y;
    public GridCoordinate(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    //显式转换为Vector2、Vector2Int、Vector3、Vector3Int
    public static explicit operator Vector2(GridCoordinate gridCoordinate)
    {
        return new Vector2((float)gridCoordinate.x, (float)gridCoordinate.y);
    }
    public static explicit operator Vector2Int(GridCoordinate gridCoordinate)
    {
        return new Vector2Int((int)gridCoordinate.x, (int)gridCoordinate.y);
    }
    public static explicit operator Vector3(GridCoordinate gridCoordinate)
    {
        return new Vector3((float)gridCoordinate.x, (float)gridCoordinate.y, 0f);
    }
    public static explicit operator Vector3Int(GridCoordinate gridCoordinate)
    {
        return new Vector3Int((int)gridCoordinate.x, (int)gridCoordinate.y, 0);
    }
}
