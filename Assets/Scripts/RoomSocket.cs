using UnityEngine;

public class RoomSocket : MonoBehaviour
{
    [SerializeField] RoomType target;
    [SerializeField] public RoomType type;
    [SerializeField] bool isEntrance;
    [SerializeField] bool isExit;
    [SerializeField] public RoomSocket connectedSocket = null;
    [SerializeField] GameObject sealPrefab;

    public bool IsEntrance => isEntrance;

    public bool IsExit => isExit;

    public void Seal()
    {
        if (connectedSocket != null) return;

        Instantiate(sealPrefab, transform.position, transform.rotation);
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
