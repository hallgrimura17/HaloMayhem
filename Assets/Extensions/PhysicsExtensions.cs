using System.Linq;
using UnityEngine;

public static class PhysicsExtension
{
    public static Collider FindClosestCollider(this Collider[] hits, Vector3 position)
    {
        return hits.OrderBy(x => (x.ClosestPoint(position) - position).sqrMagnitude).FirstOrDefault();
    }

    public static float LinearApproach(float current, float target, float maxDelta = 0.1f) => Mathf.MoveTowards(current, target, maxDelta);

    public static bool IsInMask(this GameObject gameObject, LayerMask layerMask)
    {
        return ((1 << gameObject.layer) & layerMask) != 0;
    }
}