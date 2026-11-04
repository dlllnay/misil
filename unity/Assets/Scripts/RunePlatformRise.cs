
using UnityEngine;

public class RunePlatformRise : MonoBehaviour
{
    [SerializeField] private CrystalTarget crystal1;
    [SerializeField] private CrystalTarget crystal2;
    [SerializeField] private CrystalTarget crystal3;

    [SerializeField] private float riseDistance = 1f;
    [SerializeField] private float riseSpeed = 1.5f;

    private Vector3 hiddenPosition;
    private Vector3 raisedPosition;
    private bool rising;
    public bool IsRaised =>
    rising &&
    Vector3.Distance(transform.position, raisedPosition) < 0.05f;
    private void Start()
    {
        raisedPosition = transform.position;
        hiddenPosition = raisedPosition + Vector3.down * riseDistance;
        transform.position = hiddenPosition;
    }

    private void Update()
    {
        if (crystal1 == null || crystal2 == null || crystal3 == null)
            return;

        if (!rising &&
            crystal1.IsActivated &&
            crystal2.IsActivated &&
            crystal3.IsActivated)
        {
            rising = true;
            Debug.Log("Uc kristal aktif! Nyra muhru yukseliyor!");
        }

        if (rising)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                raisedPosition,
                riseSpeed * Time.deltaTime
            );
        }
    }
}
