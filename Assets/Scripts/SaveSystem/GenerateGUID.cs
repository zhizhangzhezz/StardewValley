using UnityEngine;
[ExecuteAlways]

public class GenerateGUID : MonoBehaviour
{
    [SerializeField] private string _gUID = "";
    public string GUID { get => _gUID; set => _gUID = value; }

    private void Awake()
    {
        //在编辑器中生成唯一GUID
        if (!Application.IsPlaying(gameObject))
        {
            if (_gUID == "")
            {
                _gUID = System.Guid.NewGuid().ToString();
            }
        }
    }
}
