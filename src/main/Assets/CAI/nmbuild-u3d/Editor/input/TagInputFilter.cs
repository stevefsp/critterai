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

/// <summary>
/// Filters out all mesh filters that reference a list of meshes.
/// </summary>
[System.Serializable]
public class TagInputFilter 
    : InputBuildProcessor
{
    public bool recursive;
    public List<string> tags = new List<string>();

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
        if (state != InputBuildState.FilterComponents || context == null)
            return true;

        if (tags == null)
        {
            context.Log(name + " Mesh exclusion list is null. (Invalid processor state.)", this);
            return false;
        }

        if (tags.Count == 0)
            // Nothing to do.
            return true;

        List<Component> targetItems = context.components;

        int removed = 0;
        for (int iTarget = targetItems.Count - 1; iTarget >= 0; iTarget--)
        {
            Component targetItem = targetItems[iTarget];

            if (!targetItem)
                continue;

            int iSource = tags.IndexOf(targetItem.tag);

            if (iSource != -1)
            {
                // One of the tags is on this item.
                targetItems.RemoveAt(iTarget);
                removed++;
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
                        targetItems.RemoveAt(iTarget);
                        removed++;
                        break;
                    }
                    parent = parent.parent;
                }
            }
        }

        if (removed > 0)
            context.Log(string.Format("{0}: Filtered out {1} components.", name, removed), this);
        else
            context.Log(name + ": No components filtered.", this);

        return true;
    }
}
