using System;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Extensions
{
    public static class Vector2Extensions
    {
        public static Vector2Int FloorToInt(this Vector2 vector3)
        {
            return new Vector2Int(Mathf.FloorToInt(vector3.x), Mathf.FloorToInt(vector3.y));
        }
        public static Vector2 CastToFloat(this Vector2Int vector3)
        {
            return new Vector2(vector3.x, vector3.y);
        }
    }

    public static class Vector3Extensions
    {
        public static int3 ToInt3(this Vector3Int vector) => new int3(vector.x, vector.y, vector.z);
        public static float3 ToFloat3(this Vector3 vector) => new float3(vector.x, vector.y, vector.z);
        public static Vector3 ToVector3(this float3 vector) => new Vector3(vector.x, vector.y, vector.z);
        public static Vector3Int ToVectorInt3(this int3 int3) => new Vector3Int(int3.x, int3.y, int3.z);
        public static Vector3Int FloorToInt(this Vector3 vector3) => new Vector3Int(Mathf.FloorToInt(vector3.x), Mathf.FloorToInt(vector3.y), Mathf.FloorToInt(vector3.z));
        public static Vector3Int RoundToInt(this Vector3 vector3) => new Vector3Int((int)Math.Round(vector3.x, MidpointRounding.AwayFromZero), (int)Math.Round(vector3.y, MidpointRounding.AwayFromZero), (int)Math.Round(vector3.z, MidpointRounding.AwayFromZero));
        public static Vector3 ProjectPointOnLine(this Vector3 start, Vector3 end, Vector3 point)
        {
            var line = (end - start).normalized;
            var startToDot = point - start;
            var dot = Vector3.Dot(startToDot, line);
            return dot * line + start;
        }

        public static Vector3 CircleOffset(this Vector3 center, float distance, int count, int index) => center + Quaternion.Euler(0, 0, 360f * index / count) * Vector3.up * distance;

        public static Vector3 Multiply(this Vector3 a, Vector3 b) => new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        public static Vector3 Divide(this Vector3 a, Vector3 b) => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);

        public static Vector3 HalfScreen => new Vector3(Screen.width, Screen.height) * 0.5f;
        public static Vector3 QHDHalfScreen => new Vector3(2560f, 1440f) * 0.5f;

        public static Vector3 CalculateLaunchVectorToDestination(this Vector3 position, Vector3 destination)
        {
            float gravity = Physics.gravity.magnitude;
            float angle = 60f;
            if (destination.y + 0.5f < position.y)
            {
                var direction = (destination - position);
                direction.y = 0;
                var heightDiff = position.y - destination.y;
                destination += direction.normalized * (heightDiff * 0.3f + 2);
            }

            angle *= Mathf.Deg2Rad;
            // Selected angle in radians

            // Positions of this object and the target on the same plane
            Vector3 planarTarget = new Vector3(destination.x, 0, destination.z);
            Vector3 planarPostion = new Vector3(position.x, 0, position.z);

            // Planar distance between objects
            float distance = Vector3.Distance(planarTarget, planarPostion);

            // Distance along the y axis between objects
            float yOffset = position.y - destination.y;

            float initialVelocity = (1 / Mathf.Cos(angle)) *
                                    Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) /
                                               (distance * Mathf.Tan(angle) + yOffset));

            Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));

            // Rotate our velocity to match the direction between the two objects
            float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPostion) *
                                        (destination.x > position.x ? 1 : -1);
            Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;
            return finalVelocity;
        }
    }

    public static class QuaternionExtensions
    {
        public static float4 ToFloat4(this Quaternion quaternion) => new float4(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
        public static Quaternion ToQuaternion(this float4 float4) => new Quaternion(float4.x, float4.y, float4.z, float4.w);
    }
}