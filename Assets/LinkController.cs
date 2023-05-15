using EZDoor;
using UnityEngine;

public class LinkController : MonoBehaviour
{
    public BaseDoor door;   
    public GameObject link;
    
    void Start()
    {
        if (door.isClosed || !door.isOpen) DisableLink();
        else EnableLink();
        door.OnOpen.AddListener(EnableLink);
        door.OnClose.AddListener(DisableLink);
    }

    private void DisableLink()
    {
        link.SetActive(false);
    }

    private void EnableLink()
    {
        link.SetActive(true);
    }
}
