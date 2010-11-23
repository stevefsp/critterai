using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace org.critterai.nmgen
{
    static class Extern
    {
        [DllImport("cai-nmgen-cli")]
        public static extern bool buildSimpleMesh(Configuration config
            , float[] sourceVerts
            , int sourceVertsLength
            , int[] sourceTriangles
            , int sourceTrianglesLength
            , ref IntPtr ptrResultVerts
            , ref int resultVertLength
            , ref IntPtr ptrResultTriangles
            , ref int resultTrianglesLength
            , [In, Out] char[] messages
            , int messagesSize
            , int messageDetail);

        [DllImport("cai-nmgen-cli")]
        public static extern void freeMesh(ref IntPtr ptrVertices
            , ref IntPtr ptrTriangles);

    }
}
