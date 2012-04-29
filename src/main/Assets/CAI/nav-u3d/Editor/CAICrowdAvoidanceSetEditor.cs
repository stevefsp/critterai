//*
// * Copyright (c) 2012 Stephen A. Pratt
// * 
// * Permission is hereby granted, free of charge, to any person obtaining a copy
// * of this software and associated documentation files (the "Software"), to deal
// * in the Software without restriction, including without limitation the rights
// * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// * copies of the Software, and to permit persons to whom the Software is
// * furnished to do so, subject to the following conditions:
// * 
// * The above copyright notice and this permission notice shall be included in
// * all copies or substantial portions of the Software.
// * 
// * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// * THE SOFTWARE.
// */
//using UnityEngine;
//using UnityEditor;
//using org.critterai.nav;
//using org.critterai.u3d.editor;
//using org.critterai.nav.u3d.editor;

//// TODO: EVAL: Staged for v0.5

///// <summary>
///// <see cref="CrowdAvoidanceSet"/> editor.
///// </summary>
//[CustomEditor(typeof(CAICrowdAvoidanceSet))]
//public class CAICrowdAvoidanceSetEditor
//    : Editor
//{
//    private bool mForceDirty = false;

//    private static bool[] mDisplay = new bool[CAICrowdAvoidanceSet.MaxCount];

//    /// <summary>
//    /// Controls behavior of the inspector.
//    /// </summary>
//    public override void OnInspectorGUI()
//    {
//        CAICrowdAvoidanceSet targ = (CAICrowdAvoidanceSet)target;

//        EditorGUIUtility.LookLikeControls(150);

//        string so;
//        string sn;
//        for (int i = 0; i < CAICrowdAvoidanceSet.MaxCount; i++)
//        {
//            // EditorGUILayout.Separator();
//            so = targ.GetName(i);

//            bool changed = GUI.changed;
//            mDisplay[i] = EditorGUILayout.Foldout(mDisplay[i], so);
//            GUI.changed = changed;
//            if (mDisplay[i])
//            {
//                sn = EditorGUILayout.TextField("Name", so);
//                if (so != sn)
//                    targ.SetName(i, sn);

//                CrowdAvoidanceParams config = targ[i];

//                config.velocityBias = EditorGUILayout.FloatField(
//                    "Velocity Bias"
//                    , config.velocityBias);

//                config.weightDesiredVelocity = EditorGUILayout.FloatField(
//                    "Desired Velocity Weight"
//                    , config.weightDesiredVelocity);

//                config.weightCurrentVelocity = EditorGUILayout.FloatField(
//                    "Current Velocity Weight"
//                    , config.weightCurrentVelocity);

//                config.weightSide = EditorGUILayout.FloatField(
//                    "Side Weight"
//                    , config.weightSide);

//                config.weightToi = EditorGUILayout.FloatField(
//                    "TOI Weight"
//                    , config.weightToi);

//                config.horizontalTime = EditorGUILayout.FloatField(
//                    "Horizontal Time"
//                    , config.horizontalTime);

//                int bv;

//                bv = EditorGUILayout.IntField("Grid Size"
//                    , config.gridSize);
//                config.gridSize = 
//                    (byte)Mathf.Clamp(bv, 0, byte.MaxValue);

//                bv = EditorGUILayout.IntField("Adaptive Divisions"
//                    , config.adaptiveDivisions);
//                config.adaptiveDivisions = 
//                    (byte)Mathf.Clamp(bv, 0, byte.MaxValue);

//                bv = (byte)EditorGUILayout.IntField("Adaptive Rings"
//                    , config.adaptiveRings);
//                config.adaptiveRings = 
//                    (byte)Mathf.Clamp(bv, 0, byte.MaxValue);

//                bv = (byte)EditorGUILayout.IntField("Adaptive Depth"
//                    , config.adaptiveDepth);
//                config.adaptiveDepth = 
//                    (byte)Mathf.Clamp(bv, 0, byte.MaxValue);

//            }
//        }
//        EditorGUILayout.Separator();

//        if (GUI.changed || mForceDirty)
//        {
//            EditorUtility.SetDirty(target);
//            mForceDirty = false;
//        }
//    }

//    [MenuItem(EditorUtil.NavAssetMenu + "Crowd Avoidance Set", false, NavEditorUtil.NavAssetGroup)]
//    static void CreateAsset()
//    {
//        CAICrowdAvoidanceSet item = 
//            EditorUtil.CreateAsset<CAICrowdAvoidanceSet>(NavEditorUtil.AssetLabel);

//        EditorUtility.FocusProjectWindow();
//        Selection.activeObject = item;
//    }
//}
