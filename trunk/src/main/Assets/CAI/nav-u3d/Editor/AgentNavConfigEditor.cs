/*
 * Copyright (c) 2010 Stephen A. Pratt
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
using UnityEngine;
using UnityEditor;
using org.critterai.nav;

/// <summary>
/// Custom inspector for <see cref="AgentNavConfig"/>.
/// </summary>
[CustomEditor(typeof(AgentNavConfig))]
public class AgentNavConfigEditor
    : Editor
{

    private static bool mDisplayAvoidance = false;

    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        if (DrawInspectorGUI((AgentNavConfig)target))
        {
            EditorUtility.SetDirty(target);
        }
    }

    private static CrowdUpdateFlags HandleUpdateFlag(CrowdUpdateFlags flags
        , string name
        , CrowdUpdateFlags flag)
    {
        if (EditorGUILayout.Toggle(name, (flags & flag) != 0))
            flags |= flag;
        else
            flags &= ~flag;
        return flags;
    }

    public static bool DrawInspectorGUI(AgentNavConfig targ)
    {
        EditorGUIUtility.LookLikeControls(150);

        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PrefixLabel("Navigation Source");

#if UNITY_3_0_0	|| UNITY_3_1 || UNITY_3_2 || UNITY_3_3
        targ.manager = (NavSource)EditorGUILayout.ObjectField(targ.manager
            , typeof(NavSource));
#else
        targ.manager = (NavSource)EditorGUILayout.ObjectField(targ.manager
            , typeof(NavSource), true);
#endif

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        targ.maxPathSize = Mathf.Max(1, EditorGUILayout.IntField("Max Path Size"
            , targ.maxPathSize));

        targ.height = Mathf.Max(0.01f, EditorGUILayout.FloatField("Height"
            , targ.height));

        targ.radius = Mathf.Max(0, EditorGUILayout.FloatField("Radius"
            , targ.radius));

        targ.maxSpeed = Mathf.Max(0, EditorGUILayout.FloatField("Max Speed"
            , targ.maxSpeed));

        targ.maxAcceleration = Mathf.Max(0, EditorGUILayout.FloatField(
            "Max Acceleration"
            , targ.maxAcceleration));

        targ.collisionQueryRange = Mathf.Max(0.01f, EditorGUILayout.FloatField(
            "Collision Query Range"
            , targ.collisionQueryRange));

        targ.pathOptimizationRange = Mathf.Max(0, EditorGUILayout.FloatField(
            "Path Optimization Range"
            , targ.pathOptimizationRange));

        targ.separationWeight = Mathf.Max(0, EditorGUILayout.FloatField(
            "Separation Weight"
            , targ.separationWeight));

        EditorGUILayout.Separator();
        EditorGUILayout.PrefixLabel("Update Flags");

        CrowdUpdateFlags flags = targ.updateFlags;

        flags = HandleUpdateFlag(flags
            , "Anticipate Turns"
            , CrowdUpdateFlags.AnticipateTurns);

        flags = HandleUpdateFlag(flags
            , "Obstancle Avoidance"
            , CrowdUpdateFlags.ObstacleAvoidance);

        flags = HandleUpdateFlag(flags
            , "Crowd Separation"
            , CrowdUpdateFlags.CrowdSeparation);

        flags = HandleUpdateFlag(flags
            , "Optimize Vis"
            , CrowdUpdateFlags.OptimizeVis);

        flags = HandleUpdateFlag(flags
            , "Optimize Topo"
            , CrowdUpdateFlags.OptimizeTopo);

        targ.updateFlags = flags;

        EditorGUILayout.Separator();

        AvoidanceConfigSet asource =
            (targ.manager == null ? null : targ.manager.avoidanceSource);

        if (asource == null)
        {
            targ.avoidanceType = (byte)Mathf.Clamp(
                EditorGUILayout.IntField("Avoidance Type", targ.avoidanceType)
                , 0, byte.MaxValue);
        }
        else
        {
            targ.avoidanceType = (byte)Mathf.Clamp(targ.avoidanceType
                , 0, AvoidanceConfigSet.MaxCount);

            mDisplayAvoidance = EditorGUILayout.Foldout(mDisplayAvoidance
                , "Avoidance Type: " + asource.GetName(targ.avoidanceType));

            if (mDisplayAvoidance)
            {
                if (asource != null)
                {
                    for (int i = 0; i < AvoidanceConfigSet.MaxCount; i++)
                    {
                        if (EditorGUILayout.Toggle(asource.GetName(i)
                            , (targ.avoidanceType == i)))
                        {
                            targ.avoidanceType = (byte)i;
                        }
                    }
                }
            }
        }

        EditorGUILayout.Separator();

        return GUI.changed;
    }

}
