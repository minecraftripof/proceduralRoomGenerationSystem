using UnityEngine;

public class Room : MonoBehaviour
{
    RoomSocket[] sockets;
    [SerializeField] int socketCount;
    BoxCollider[] boxColliders;
    LayerMask roomPlacementOccupiedLayer;
    [SerializeField] GameObject roomPrefab;
    [SerializeField] RoomType type; 
    GameObject room;

    public RoomSocket[] Sockets => sockets;

    public int SocketCount => socketCount;

    public RoomType Type => type;

    private void OnValidate()
    {
        RoomSocket[] sockets = GetComponentsInChildren<RoomSocket>();
        socketCount = sockets.Length;
        foreach (RoomSocket socket in sockets)
        {
            socket.type = type;
        }
    }

    private void Awake()
    {
        sockets = GetComponentsInChildren<RoomSocket>();
        boxColliders = GetComponentsInChildren<BoxCollider>();
        roomPlacementOccupiedLayer = LayerMask.GetMask("RoomPlacementOccupied");
    }

    void Start()
    {

    }

    void Update()
    {
        
    }

    public void PlaceRoom()
    {
        room = Instantiate(roomPrefab, transform.position, transform.rotation);
        foreach(RoomSocket socket in sockets)
        {
            if(socket.connectedSocket == null)
            {
                socket.Seal();
            }
        }
    }

    public void SwitchColliderLayer()
    {
        boxColliders[0].gameObject.layer = LayerMask.NameToLayer("RoomPlacementOccupied");
    }

    private void OnDestroy()
    {
        Destroy(room);
    }

    public bool CheckPlacementOverlaps()
    {
        Vector3 halfExtents;
        foreach(BoxCollider box in boxColliders)
        {
            halfExtents = Vector3.Scale(box.size, box.transform.lossyScale) * 0.5f;
            halfExtents -= new Vector3(0.01f, 0.01f, 0.01f);

            if (Physics.OverlapBox(
                box.transform.TransformPoint(box.center),
                halfExtents,
                box.transform.rotation,
                roomPlacementOccupiedLayer,
                QueryTriggerInteraction.Collide
                ).Length != 0)
            {
                return false;
            }
        }
        return true;
    }
}
