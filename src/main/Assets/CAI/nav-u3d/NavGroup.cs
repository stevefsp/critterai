/*
 * Copyright (c) 2011 Stephen A. Pratt
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
using org.critterai;
using org.critterai.nav;

namespace org.critterai.nav.u3d
{
    /// <summary>
    /// Provides a convenient structure for grouping together related
    /// navigation resources.
    /// </summary>
    public struct NavGroup
    {
        /// <summary>
        /// The navigation mesh used by <see cref="query"/>.
        /// </summary>
        public Navmesh mesh;

        /// <summary>
        /// A navigation mesh query.
        /// </summary>
        public U3DNavmeshQuery query;

        /// <summary>
        /// The filter to use with <see cref="query"/>.
        /// </summary>
        public NavmeshQueryFilter filter;

        /// <summary>
        /// A crowd.
        /// </summary>
        public CrowdManager crowd;

        /// <summary>
        /// The extents to use with <see cref="query"/>.
        /// </summary>
        public float[] extents;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mesh">The navigation mesh used by the query.
        /// </param>
        /// <param name="query">A navigation mesh query.</param>
        /// <param name="crowd">A crowd.</param>
        /// <param name="filter">The filter to use with the query.</param>
        /// <param name="extents">The extents to use with query.</param>
        /// <param name="cloneFE">If true, the filter and extents will be
        /// cloned.  Otherwise they will be referenced.</param>
        public NavGroup(Navmesh mesh
            , U3DNavmeshQuery query
            , CrowdManager crowd
            , NavmeshQueryFilter filter
            , float[] extents
            , bool cloneFE)
        {
            this.mesh = mesh;
            this.query = query;
            this.crowd = crowd;
            if (cloneFE)
            {
                this.extents = (float[])extents.Clone();
                this.filter = filter.Clone();
            }
            else
            {
                this.extents = extents;
                this.filter = filter;
            }
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="copy">The group to copy.</param>
        /// <param name="cloneFE">If true, the filter and extents will be
        /// cloned.  Otherwise they will be referenced.</param>
        public NavGroup(NavGroup copy, bool cloneFE)
        {
            this.mesh = copy.mesh;
            this.query = copy.query;
            this.crowd = copy.crowd;
            if (cloneFE)
            {
                this.extents = (float[])copy.extents.Clone();
                this.filter = copy.filter.Clone();
            }
            else
            {
                this.extents = copy.extents;
                this.filter = copy.filter;
            }
        }
    }
}