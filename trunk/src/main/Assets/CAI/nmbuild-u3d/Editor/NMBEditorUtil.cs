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
using UnityEditor;
using org.critterai.nav.u3d;
using org.critterai.u3d.editor;
using org.critterai.nmgen;

namespace org.critterai.nmbuild.u3d.editor
{
    internal static class NMBEditorUtil
    {
        public const string AssetLabel = "NMGen";

        public const int BuildGroup = EditorUtil.AssetGroup;
        public const int SceneGroup = BuildGroup + 100;
        public const int FilterGroup = BuildGroup + 200;
        public const int AreaGroup = BuildGroup + 300;
        public const int CompilerGroup = BuildGroup + 400;

        public static NavmeshBuildInfo GetConfig(NavmeshBuild build)
        {
            NMGenParams config = build.Config.GetConfig();

            NavmeshBuildInfo result = new NavmeshBuildInfo();

            result.tileSize = config.TileSize;
            result.walkableHeight = config.WalkableHeight;
            result.walkableRadius = config.WalkableRadius;
            result.walkableStep = config.WalkableStep;
            result.xzCellSize = config.XZCellSize;
            result.yCellSize = config.YCellSize;
            result.borderSize = config.BorderSize;
            result.inputScene = EditorApplication.currentScene;

            return result;
        }
    }
}
