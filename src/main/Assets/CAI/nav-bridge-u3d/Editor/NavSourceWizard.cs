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
using org.critterai.nmgen.u3d.editor;

public sealed class NavSourceWizard
    : EditorWindow
{

    private PolyMeshEditorFlags mNMGenFlags = 
        PolyMeshEditorFlags.MeshFilterArray
        | PolyMeshEditorFlags.BuildConfig
        | PolyMeshEditorFlags.BakedPolyMesh;

    private bool mIncludeNavmesh = true;
    private bool mIncludeAvoidance = false;
    private bool mIncludeAgent = false;

    public void OnGUI()
    {
        mNMGenFlags = PolyMeshWizard.HandleSelection(mNMGenFlags, true);

        EditorGUILayout.Separator();
        EditorGUILayout.PrefixLabel("Navmesh Options");

        mIncludeNavmesh = EditorGUILayout.Toggle("Baked Navmesh"
            , mIncludeNavmesh);

        mIncludeAvoidance = EditorGUILayout.Toggle("Avoidance Config"
            , mIncludeAvoidance);

        mIncludeAgent = EditorGUILayout.Toggle("Agent Config"
            , mIncludeAgent);

        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();

        bool closeIt = false;

        if (GUILayout.Button("Create"))
        {
            NavSource manager;
            Selection.activeGameObject = Build(mNMGenFlags
                , mIncludeNavmesh
                , mIncludeAvoidance
                , mIncludeAgent
                , out manager);
            closeIt = true;
        }

        if (GUILayout.Button("Cancel"))
            closeIt = true;

        EditorGUILayout.EndHorizontal();

        if (closeIt)
            this.Close();
    }

    public static GameObject Build(PolyMeshEditorFlags flags
        , bool includeNavmesh
        , bool includeAvoidanceConfig
        , bool includeAgentConfig
        , out NavSource data)
    {
        BakedNavmesh navmesh = null;
        GameObject goNavmesh = null; 
        
        if (includeNavmesh)
            goNavmesh = NavmeshWizard.Build(flags, out navmesh);

        GameObject goData = new GameObject("NavSource");

        data = goData.AddComponent<NavSource>();

        data.navmeshSource = navmesh;

        if (goNavmesh != null)
            goNavmesh.transform.parent = goData.transform;

        if (includeAvoidanceConfig)
        {
            AvoidanceConfigSet aconfig =
                goData.AddComponent<AvoidanceConfigSet>();

            data.avoidanceSource = aconfig;
            data.enableCrowdManager = true;
        }
        else
        {
            data.enableCrowdManager = false;
        }

        if (includeAgentConfig)
        {
            GameObject goAgentConfig = new GameObject("AgentConfig");
            AgentNavConfig agentConfig 
                = goAgentConfig.AddComponent<AgentNavConfig>();
            goAgentConfig.transform.parent = goData.transform;
            agentConfig.manager = data;
        }

        return goData;
    }

    [MenuItem("GameObject/Create CAI/Navigation Source", false, 0)]
    public static void CreateNavSource()
    {
        NavSourceWizard window = EditorWindow.GetWindow<NavSourceWizard>(
            true
            , "NavSource Options"
            , true);

        window.position = new Rect(100, 100, 250, 250);
        window.Focus();
    }
}
