using UnityEngine;

public class RaycastObstacleSensor : MonoBehaviour, IObstacleSensor
{
    [SerializeField] private float range = 5f;
    [SerializeField] private int rayCount = 7;
    [SerializeField] private float fieldOfView = 120f;

    public ObstacleReading Sense()
    {
        Vector3 origin = transform.position;
        bool found = false;
        float bestDistance = float.PositiveInfinity;
        float bestAngle = 0f;

        for (int i = 0; i < rayCount; i++)
        {
            float t = rayCount == 1 ? 0.5f : (float)i / (rayCount - 1);
            float angle = Mathf.Lerp(-fieldOfView / 2f, fieldOfView / 2f, t);
            Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;

            bool hit = Physics.Raycast(origin, direction, out RaycastHit info, range);
            Debug.DrawRay(origin, direction * (hit ? info.distance : range), hit ? Color.red : Color.green);

            if (hit && info.distance < bestDistance)
            {
                found = true;
                bestDistance = info.distance;
                bestAngle = angle;
            }
        }

        return found ? new ObstacleReading(true, bestDistance, bestAngle) : ObstacleReading.None;
    }
}