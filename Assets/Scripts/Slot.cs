using UnityEngine;

public class Slot : MonoBehaviour
{
    public int slotId;           // 槽位编号 0-8
    public bool isOccupied = false;  // 是否已被占用

    public void SetOccupied(bool occupied)
    {
        isOccupied = occupied;
    }
}