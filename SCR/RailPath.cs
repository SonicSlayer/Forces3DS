using UnityEngine;

public class RailPath : MonoBehaviour
{
    public Transform[] points;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (points != null && points.Length > 1)
        {
            for (int i = 0; i < points.Length - 1; i++)
            {
                if (points[i] != null && points[i + 1] != null)
                {
                    Gizmos.DrawLine(points[i].position, points[i + 1].position);
                }
            }
        }
    }
}
