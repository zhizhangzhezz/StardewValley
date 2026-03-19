[System.Serializable]

public class SceneItem
{
    public int itemCode;
    public string itemName;
    public Vector3Serializable position;

    public SceneItem()
    {
        position = new Vector3Serializable();
    }

}
