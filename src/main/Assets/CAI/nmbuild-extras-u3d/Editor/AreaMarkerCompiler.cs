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
using org.critterai.nmbuild;
using org.critterai.nmbuild.u3d.editor;
using UnityEngine;

public sealed class AreaMarkerCompiler
    : InputBuildProcessor
{
    public string Name { get { return name; } }
    public override int Priority { get { return NMBuild.MinPriority; } }

    public override bool DuplicatesAllowed { get { return false; } }

    public override bool ProcessInput(InputBuildState state, InputBuildContext context)
    {
        if (context != null)
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

        int count = context.LoadFromScene<NMGenAreaMarker>();

        context.Log(string.Format("{0}: Loaded {1} area markers.", Name, count), this);
    }

    private void Compile(InputBuildContext context)
    {
        context.info.compilerCount++;

        List<Component> items = context.components;

        int count = 0;

        // The components can't provide the processor directly because processors are editor-only.

        foreach (Component item in items)
        {
            if (item is CylinderAreaMarker)
            {
                CylinderAreaMarker.MarkerData data = ((CylinderAreaMarker)item).GetMarkerData();

                AreaCylinderMarker processor = AreaCylinderMarker.Create(item.name
                    , data.priority
                    , data.area
                    , data.centerBase
                    , data.radius
                    , data.height);

                context.processors.Add(processor);

                count++;
            }
            else if (item is BoxAreaMarker)
            {
                BoxAreaMarker.MarkerData data = ((BoxAreaMarker)item).GetMarkerData();

                AreaConvexMarker processor = AreaConvexMarker.Create(item.name
                    , data.priority
                    , data.area
                    , data.verts
                    , data.ymin
                    , data.ymax);

                context.processors.Add(processor);

                count++;
            }
        }

        context.Log(string.Format("{0}: Loaded {1} area markers.", Name, count), this);
    }
}
