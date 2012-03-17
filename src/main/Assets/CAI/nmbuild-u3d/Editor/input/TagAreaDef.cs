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
using org.critterai.nmbuild.u3d;
using UnityEngine;

[System.Serializable]
public class TagAreaDef 
    : InputBuildProcessor
{
    public bool recursive;
    public List<string> tags;
    public List<byte> areas;

    [SerializeField]
    private int mPriority = NMBuild.DefaultPriority;

    public override int Priority { get { return mPriority; } }

    public void SetPriority(int value)
    {
        mPriority = NMBuild.ClampPriority(value);
    }

    public override bool DuplicatesAllowed { get { return true; } }

    public override bool ProcessInput(InputBuildState state, InputBuildContext context)
    {
        if (state != InputBuildState.ApplyAreaModifiers || tags.Count == 0)
            return true;

        context.info.areaModifierCount++;

        if (tags == null || areas == null || tags.Count != areas.Count)
        {
            context.Log("Mesh/Area size error. (Invalid processor state.)", this);
            return false;
        }

        List<Component> targetItems = context.components;
        List<byte> targetAreas = context.areas;

        int applied = 0;
        for (int iTarget = 0; iTarget < targetItems.Count; iTarget++)
        {
            Component targetItem = targetItems[iTarget];

            if (targetItem == null)
                continue;

            int iSource = tags.IndexOf(targetItem.tag);

            if (iSource != -1)
            {
                targetAreas[iTarget] = areas[iSource];
                applied++;
                continue;
            }

            if (recursive)
            {
                // Need to see if the tag is on any parent.
                Transform parent = targetItem.transform.parent;

                while (parent != null)
                {
                    iSource = tags.IndexOf(parent.tag);

                    if (iSource != -1)
                    {
                        // One of the tags is on this item.
                        targetAreas[iTarget] = areas[iSource];
                        applied++;
                        break;
                    }
                    parent = parent.parent;
                }
            }
        }

        context.Log(string.Format("{1}: Applied area(s) to {0} components", applied, name), this);

        return true;
    }
}
