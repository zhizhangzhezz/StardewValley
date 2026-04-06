// 定义可保存的接口
public interface ISavable
{
    string ISavableUniqueID { get; set; }
    GameObjectSave GameObjectSave { get; set; }
    void ISavableRegister();
    void ISavableDeregister();

    GameObjectSave ISavableSave();
    void ISavableLoad(GameSave gameSave);

    void ISavableStoreScene(string sceneName);
    void ISavableRestoreScene(string sceneName);
}
