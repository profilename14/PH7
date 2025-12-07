#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using TMPro;

namespace GSPAWN
{
    public struct Circle3D
    {
        private static List<Vector3> _vector3Buffer = new List<Vector3>();

        public static Vector3 calcRandomPoint(Vector3 circleCenter, float circleRadius, Vector3 circleU, Vector3 circleV)
        {
            float r         = circleRadius * Mathf.Sqrt(Random.Range(0.0f, 1.0f));
            float theta     = Random.Range(0.0f, 1.0f) * 2.0f * Mathf.PI;
            return circleCenter + circleU * r * Mathf.Cos(theta) + circleV * r * Mathf.Sin(theta);
        }

        public static void calcExtents(Vector3 circleCenter, float circleRadius, Vector3 circleU, Vector3 circleV, List<Vector3> extents)
        {
            extents.Clear();
            extents.Add(circleCenter - circleU * circleRadius);
            extents.Add(circleCenter + circleV * circleRadius);
            extents.Add(circleCenter + circleU * circleRadius);
            extents.Add(circleCenter - circleV * circleRadius);
        }

        public static bool containsPointAsInfiniteCylinder(Vector3 circleCenter, float circleRadius, Vector3 circleU, Vector3 circleV, Vector3 point)
        {
            Vector3 toPt    = point - circleCenter;
            float sqr0      = Vector3Ex.absDot(toPt, circleU);
            sqr0 *= sqr0;

            float sqr1 = Vector3Ex.absDot(toPt, circleV);
            sqr1 *= sqr1;

            return (sqr0 + sqr1) <= (circleRadius * circleRadius);
        }

        public static bool containsPointsAsInfiniteCylinder(Vector3 circleCenter, float circleRadius, Vector3 circleU, Vector3 circleV, IEnumerable<Vector3> points)
        {
            foreach (var pt in points)
            {
                Vector3 toPt = pt - circleCenter;
                float sqr0 = Vector3Ex.absDot(toPt, circleU);
                sqr0 *= sqr0;

                float sqr1 = Vector3Ex.absDot(toPt, circleV);
                sqr1 *= sqr1;

                if ((sqr0 + sqr1) > (circleRadius * circleRadius)) return false;
            }

            return true;
        }

        public static bool intersectsOBBAsInfiniteCylinder(Vector3 circleCenter, float circleRadius, Vector3 circleU, Vector3 circleV, OBB obb)
        {
            calcExtents(circleCenter, circleRadius, circleU, circleV, _vector3Buffer);
            if (obb.containsPoint(_vector3Buffer[0])) return true;
            if (obb.containsPoint(_vector3Buffer[1])) return true;
            if (obb.containsPoint(_vector3Buffer[2])) return true;
            if (obb.containsPoint(_vector3Buffer[3])) return true;

            Box3DFace[] boxFaces = Box3D.facesArray;
            foreach(var boxFace in boxFaces)
            {
                Box3D.calcFaceCorners(obb.center, obb.size, obb.rotation, boxFace, _vector3Buffer);
                for (int ptIndex = 0; ptIndex < 4; ++ptIndex)
                {
                    if (intersectsSegment(circleCenter, circleRadius, circleU, circleV, _vector3Buffer[ptIndex], _vector3Buffer[(ptIndex + 1) % 4]))
                        return true;
                }
            }

            return false;
        }

        public static bool intersectsSegment(Vector3 circleCenter, float circleRadius, Vector3 circleU, Vector3 circleV, Vector3 p0, Vector3 p1)
        {
            // Calculate segment
            Vector3 segDir  = p1 - p0;
            float segLength = segDir.magnitude;
            segDir.Normalize();

            // Check if the segment intersects the plane of the circle
            float   t;
            Ray     ray         = new Ray(p0, segDir);
            Plane   circlePlane = new Plane(Vector3.Cross(circleU, circleV).normalized, circleCenter);
            if (circlePlane.Raycast(ray, out t))
            {
                // An intersection occurred, now check if the intersection point is within range
                return (ray.GetPoint(t) - circleCenter).magnitude <= circleRadius;
            }
            
            // The ray doesn't intersect the plane. It means the segment may be running
            // parallel to the plane surface. Project the circle center onto the segment
            // direction vector. If the distance between the circle center and its projection
            // is greater than the circle radius, the segment can not possibly be intersecting
            // the plane
            float   dot         = Vector3.Dot(circleCenter - p0, segDir);
            Vector3 prjCenter   = p0 + segDir * dot;
            float   toPrjDist   = (prjCenter - circleCenter).magnitude;
            if (toPrjDist > circleRadius) return false;

            // If the projected center lies within the segment end points, we have an intersection
            if (dot >= 0.0f && dot <= segLength) { return true; }

            // If we reach this point, it means the segment might be intersecting the circle.
            // Calculate the amount we would have to move from the projected center along
            // the segment direction to end up on the circle circumference.
            float length = Mathf.Sqrt(circleRadius * circleRadius - toPrjDist * toPrjDist);

            // Check if moving from the projected center along the segment direction
            // for an amount equal to 'length' ends up on a point on the segment.
            Vector3 pt = prjCenter - segDir * length;
            if (pt.isPointOnSegment(p0, p1)) { return true; }
            pt = prjCenter + segDir * length;
            if (pt.isPointOnSegment(p0, p1)) { return true; }

            // No intersection
            return false;
        }
    }
}
#endif