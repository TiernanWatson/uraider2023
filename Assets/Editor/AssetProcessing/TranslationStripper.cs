using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TranslationStripper : AssetPostprocessor
{
    private const float kHipHeightAdjust = -0.086f;

    private AnimationCurve _clipHipRotationX;

    void OnPostprocessAnimation(GameObject go, AnimationClip clip)
    {
        /*
        PopulateRotationCurve(clip);
        StripClip(clip);*/
    }

    private void PopulateRotationCurve(AnimationClip clip)
    {
        _clipHipRotationX = null;

        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);

        // Find the hip rotation for use later in determining how to offset the hip
        for (int i = 0; i < bindings.Length; i++)
        {
            if (bindings[i].path.Equals("ROOT/HIP"))
            {
                if (bindings[i].propertyName.Contains("LocalRotation.x"))
                {
                    _clipHipRotationX = AnimationUtility.GetEditorCurve(clip, bindings[i]);
                }
            }
        }
    }

    private void StripClip(AnimationClip clip)
    {
        bool adjustAsCircle = clip.name.Contains("PoleSwing");

        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);

        for (int i = 0; i < bindings.Length; i++)
        {
            // Don't strip the hip bone because it used to define the player's height
            // This strips anything that contains these for e.g. 'ROOT/HIP/THIGH_L/CALF_L'
            if (bindings[i].path.Contains("SPINE") || bindings[i].path.Contains("THIGH"))
            {
                if (bindings[i].propertyName.Contains("LocalPosition"))
                {
                    AnimationUtility.SetEditorCurve(clip, bindings[i], null);
                }
            }

            if (bindings[i].path.Equals("ROOT/HIP"))
            {
                if (bindings[i].propertyName.Contains("LocalPosition.z"))
                {
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, bindings[i]);

                    for (int j = 0; j < curve.keys.Length; j++)
                    {
                        Keyframe adjustment = curve.keys[j];

                        if (adjustAsCircle && _clipHipRotationX != null)
                        {
                            AdjustHeightAsCircle(ref adjustment, false);
                        }
                        else
                        {
                            adjustment.value += kHipHeightAdjust;
                        }
                        curve.MoveKey(j, adjustment);
                    }

                    AnimationUtility.SetEditorCurve(clip, bindings[i], curve);
                }
                else if (adjustAsCircle && bindings[i].propertyName.Contains("LocalPosition.y")) // Axes are z = up, y = back
                {
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, bindings[i]);

                    for (int j = 0; j < curve.keys.Length; j++)
                    {
                        Keyframe adjustment = curve.keys[j];
                        AdjustHeightAsCircle(ref adjustment, true);
                        curve.MoveKey(j, adjustment);
                    }

                    AnimationUtility.SetEditorCurve(clip, bindings[i], curve);
                }
            }
        }
    }

    /// <summary>
    /// Used for animations like pole swinging were we also offset on y (forward/back)
    /// </summary>
    /// <param name="key">Keyframe to adjust</param>
    /// <param name="isY">Is this forward/back or up/down?</param>
    private void AdjustHeightAsCircle(ref Keyframe key, bool isY)
    {
        float time = key.time;
        float rotationValue = _clipHipRotationX.Evaluate(time) * 180.0f;

        Vector3 toHip = Quaternion.Euler(rotationValue, 0.0f, 0.0f) * new Vector3(0.0f, 0.0f, 1.0f);
        toHip *= kHipHeightAdjust;

        key.value += isY ? toHip.y : toHip.z;
    }
}
