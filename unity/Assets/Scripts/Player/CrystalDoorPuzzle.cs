
using UnityEngine;

public class CrystalDoorPuzzle : MonoBehaviour
{
    [SerializeField] private CrystalTarget crystal1;
    [SerializeField] private CrystalTarget crystal2;
    [SerializeField] private CrystalTarget crystal3;

    [SerializeField] private Transform door;
    [SerializeField] private float openDistance = 3f;
    [SerializeField] private float openSpeed = 2f;

    private Vector3 openPosition;
    private bool opening;

    private void Start()
    {
        if (door == null)
        {
            Debug.LogWarning("DungeonGateDoor atanmamis!");
            return;
        }

        openPosition = door.position + Vector3.down * openDistance;
    }

    private void Update()
    {
        if (door == null || crystal1 == null ||
            crystal2 == null || crystal3 == null)
            return;

        if (!opening &&
            crystal1.IsActivated &&
            crystal2.IsActivated &&
            crystal3.IsActivated)
        {
            opening = true;
            Debug.Log("Uc kristal aktif! Muhurlu kapi aciliyor!");
        }

        if (opening)
        {
            door.position = Vector3.MoveTowards(
                door.position,
                openPosition,
                openSpeed * Time.deltaTime
            );
        }
    }
}
