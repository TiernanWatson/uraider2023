using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ReachType
{
    Clear,
    Hit,
    Miss
}

public static class UMath
{
    public static void DrawCapsule(Vector3 start, Vector3 end, float radius)
    {
        Debug.DrawLine(start + Vector3.down * radius, end + Vector3.up * radius, Color.red);

        Debug.DrawLine(start - Vector3.forward * radius, start + Vector3.forward * radius, Color.red);
        Debug.DrawLine(start - Vector3.right * radius, start + Vector3.right * radius, Color.red);

        Debug.DrawLine(end - Vector3.forward * radius, end + Vector3.forward * radius, Color.red);
        Debug.DrawLine(end - Vector3.right * radius, end + Vector3.right * radius, Color.red);
    }

    /// <summary>
    /// Returns the point at which two 2D lines intersect
    /// </summary>
    /// <param name="gradient1">Line 1 gradient</param>
    /// <param name="point1">Any point on line 1</param>
    /// <param name="gradient2">Line 2 gradient</param>
    /// <param name="point2">Any point on line 2</param>
    /// <returns>Point at which they intersect</returns>
    public static Vector2 LineIntersection(float gradient1, Vector2 point1, float gradient2, Vector2 point2)
    {
        float m1x1 = gradient1 * point1.x;
        float m2x2 = gradient2 * point2.x;

        float x = (m1x1 - m2x2 + point2.y - point1.y) / (gradient1 - gradient2);
        float y = gradient1 * x - gradient1 * point1.x + point1.y;

        return new Vector2(x, y);
    }

    public static float HorizontalMag(Vector3 vector)
    {
        return Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z);
    }

    public static ReachType CanReach(float forwardSpeed, float upSpeed, float zDist, float yDist, float gravity)
    {
        float timeAtPoint = zDist / forwardSpeed;  
        float peakTime = upSpeed / gravity; 

        // Player would hit wall before reaching jump peak and keep going up
        if (peakTime > timeAtPoint)
        {
            timeAtPoint = peakTime;
        }

        float height = upSpeed * timeAtPoint + 0.5f * -gravity * timeAtPoint * timeAtPoint;

        if (height > yDist)
        {
            return ReachType.Clear;
        }
        else if (height > yDist - 1.8f)
        {
            return ReachType.Hit;
        }
        else
        {
            return ReachType.Miss;
        }
    }

    public static bool CanClear(float forwardSpeed, float upSpeed, float zDist, float yDist, float gravity, out float heightOffset)
    {
        float timeAtPoint = zDist / forwardSpeed;
        float peakTime = -upSpeed / -gravity;

        // Player would hit wall before reaching jump peak and keep going up
        if (peakTime > timeAtPoint)
        {
            timeAtPoint = peakTime;
        }

        float height = upSpeed * timeAtPoint + 0.5f * -gravity * timeAtPoint * timeAtPoint;
        heightOffset = height - yDist;

        return height > yDist + Mathf.Epsilon;
    }

    public static float GetPeakHeight(float upSpeed, float gravity)
    {
        return upSpeed * upSpeed / 2.0f * gravity;
    }

    public static float HeightAt(float distance, float forwardSpeed, float upSpeed, float gravity, bool peakClamp = true)
    {
        float timeAtPoint = distance / forwardSpeed;
        float peakTime = -upSpeed / -gravity;

        // Player would hit wall before reaching jump peak and keep going up
        if (peakClamp && peakTime > timeAtPoint)
        {
            timeAtPoint = peakTime;
        }

        return upSpeed * timeAtPoint + 0.5f * -gravity * timeAtPoint * timeAtPoint;
    }

    public static float TimeToPoint(float displacement, float speed)
    {
        return displacement / speed;
    }

    /// <summary>
    /// Returns a velocity that cause an object to reach end at time 
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="gravity"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public static Vector3 JumpInTime(Vector3 start, Vector3 end, float gravity, float time)
    {
        Vector3 relative = end - start;

        Vector3 dir = relative;
        dir.y = 0f;
        dir.Normalize();

        float xz = HorizontalMag(relative) / time;  // u = s/t
        float y = (relative.y - 0.5f * -gravity * time * time) / time;  // u = (s - 0.5at^2) / t

        return new Vector3(xz * dir.x, y, xz * dir.z);
    }


    public static float JumpToReach(float distance, float time, float gravity)
    {
        return (distance - 0.5f * -gravity * time * time) / time;
    }

    public static float JumpZToReach(float yDist, float zDist, float ySpeed, float gravity, out float time)
    {
        time = -ySpeed - Mathf.Sqrt(ySpeed * ySpeed + 2.0f * -gravity * yDist);
        time /= -gravity;

        return zDist / time;
    }

    public static float PeakAt(float displace, float gravity, out float time)
    {
        float speed = Mathf.Sqrt(-2f * gravity * displace);
        time = -speed / gravity;
        return speed;
    }

    // Taken from https://answers.unity.com/questions/659932/how-do-i-clamp-my-rotation.html
    public static float ClampAngle(float angle, float min, float max)
    {
        angle = Mathf.Repeat(angle, 360);
        min = Mathf.Repeat(min, 360);
        max = Mathf.Repeat(max, 360);
        bool inverse = false;
        float tmin = min;
        float tangle = angle;
        if (min > 180)
        {
            inverse = !inverse;
            tmin -= 180;
        }
        if (angle > 180)
        {
            inverse = !inverse;
            tangle -= 180;
        }
        var result = !inverse ? tangle > tmin : tangle < tmin;
        if (!result)
            angle = min;

        inverse = false;
        tangle = angle;
        var tmax = max;
        if (angle > 180)
        {
            inverse = !inverse;
            tangle -= 180;
        }
        if (max > 180)
        {
            inverse = !inverse;
            tmax -= 180;
        }

        result = !inverse ? tangle < tmax : tangle > tmax;
        if (!result)
            angle = max;
        return angle;
    }
}
