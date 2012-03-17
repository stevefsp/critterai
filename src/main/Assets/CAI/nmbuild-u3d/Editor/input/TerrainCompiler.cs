/*
 * Copyright (c) 2012 Stephen A. Pratt
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
using System.Collections.Generic;
using org.critterai.geom;
using org.critterai.nmbuild;
using org.critterai.nmbuild.u3d;
using org.critterai.nmgen;
using UnityEngine;

[System.Serializable]
public sealed class TerrainCompiler
    : InputBuildProcessor
{
    public TerrainData terrainData;
    public bool includeTrees;

    [SerializeField]
    private float mResolution = 0.1f;

    public float Resolution
    {
        get { return mResolution; }
        set { mResolution = Mathf.Clamp01(value); }
    }

    public override bool DuplicatesAllowed { get { return true; } }

    private Terrain GetTerrain(InputBuildContext context)
    {
        if (terrainData == null)
            return null;

        Terrain[] items;

        if (context == null)
            items = (Terrain[])FindObjectsOfType(typeof(Terrain));
        else
            items = context.GetFromScene<Terrain>();

        if (items.Length == 0)
            return null;

        Terrain selected = null;
        bool multiple = false;

        foreach (Terrain item in items)
        {
            if (item.terrainData == terrainData)
            {
                if (selected == null)
                    selected = item;
                else
                    multiple = true;
            }
        }

        if (multiple)
        {
            string msg = string.Format(
                "{0}: Multiple terrains in the scene use the same data. {1} was selected"
                , name, selected.name);

            if (context == null)
                Debug.LogWarning(msg, this);
            else
                context.Log(msg, this);
        }

        return selected;
    }

    public Terrain GetTerrain() 
    {
        return GetTerrain(null);
    }

    public string Name { get { return name; } }
    public override int Priority { get { return NMBuild.MinPriority; } }

    public override bool ProcessInput(InputBuildState state, InputBuildContext context)
    {
        if (context != null && terrainData)
        {
            switch (state)
            {
                case InputBuildState.CompileInput:

                    Compile(context);
                    break;

                case InputBuildState.LoadComponents:

                    Load(context);
                    break;
            }
        }

        return true;
    }

    private void Load(InputBuildContext context)
    {
        context.info.loaderCount++;
        
        Terrain item = GetTerrain(context);

        if (!item)
        {
            context.LogWarning(string.Format("{0}: No terrain found using {1} terrain data."
                , Name, terrainData.name)
                , this);
        }
        else
        {
            context.Load(item);
            context.Log(string.Format("{0}: Loaded the {1} terrain object."
                , Name, terrainData.name)
                , this);
        }
    }

    private void Compile(InputBuildContext context)
    {
        context.info.compilerCount++;

        InputGeometryCompiler compiler = context.geomCompiler;
        List<Component> items = context.components;
        List<byte> areas = context.areas;

        for (int i = 0; i < items.Count; i++)
        {
            Component item = items[i];

            if (item is Terrain)
            {
                Terrain terrain = (Terrain)item;

                if (terrain.terrainData != terrainData)
                    continue;

                TriangleMesh mesh = TerrainUtil.TriangulateSurface(terrain, mResolution);
                byte[] lareas = NMGen.CreateAreaBuffer(mesh.triCount, areas[i]);

                if (compiler.AddTriangles(mesh, lareas))
                {
                    string msg = string.Format(
                        "{0}: Compiled the {1} terrain surface. Triangles: {2}"
                        , name, terrain.name, mesh.triCount);

                    context.Log(msg, this);
                }
                else
                {
                    string msg = string.Format("{0}: Compiler rejected mesh for the {1} terrain."
                        , name, terrain.name);

                    context.LogError(msg, this);

                    return;
                }

                if (includeTrees)
                {
                    int before = compiler.TriCount;

                    TerrainUtil.TriangluateTrees(terrain, areas[i], compiler);

                    string msg = string.Format("{0}: Compiled the {1} terrain trees. Triangles: {2}"
                        , name, terrain.name, compiler.TriCount - before);

                    context.Log(msg, this);
                }

                break;
            }
        }
    }
}
