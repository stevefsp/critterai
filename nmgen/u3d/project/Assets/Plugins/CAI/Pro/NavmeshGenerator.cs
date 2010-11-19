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
using System.Runtime.InteropServices;
using IntPtr = System.IntPtr;

/// <summary>
/// Provides methods for building static triangle navigation meshes.
///  (Unity Pro Only)
/// </summary>
/// <remarks>
/// This class handles all interop marshalling to/from the cai-nmgen-cli libary.
/// <p>Notes on use within Unity</p>
/// <ul>
/// <li>The functionality provided by this class can only be used within 
/// Unity Pro.  It will not function within the free version of Unity.</li>
/// <li>This class should not be referenced by any scripts attached to
/// scene objects in projects that need to be deployed to the Unity Web 
/// Player.</li>
/// <li>The normal useage model is to use the <see cref="GeneratedNavmesh"/>
/// component to build navigation meshes.  It has a custom inspector
/// which takes care of the details.</li>
/// </ul>
/// </remarks>
public static class NavmeshGenerator
{
    /*
     * Design notes:
     * 
     * The external functions are kept private to prevent abuse.
     * 
     * This class is kept free of Unity references in order to permit
     * dual use in .NET.
     * 
     */

    /// <summary>
    /// Builds a static triangle navigation mesh from the source geometry.
    /// </summary>
    /// <remarks>
    /// WARNING: If this method returns TRUE, then the freeMesh() method must
    /// be called before ptrResultVerts or ptrResultIndices go
    /// out of scope.  Otherwise a memory leak will occur.
    /// </remarks>
    /// <returns>TRUE if a mesh was successfully generated.</returns>
    [DllImport("cai-nmgen-cli")]
    private static extern bool buildSimpleMesh(Configuration config
        , float[] sourceVerts
        , int sourceVertsLength
        , int[] sourceIndices
        , int sourceIndicesLength
        , ref IntPtr ptrResultVerts
        , ref int resultVertLength
        , ref IntPtr ptrResultIndices
        , ref int resultIndicesLength
        , [In, Out] char[] messages
        , int messagesSize
        , int messageDetail);

    /// <summary>
    /// Frees the unmanaged memory allocated when a successful build has
    /// occurred.
    /// </summary>
    /// <remarks>
    /// WARNING: A memory leak will occur if this method is not called
    /// after a successful call to <see cref="buildSimpleMesh"/>.
    /// </remarks>
    /// <param name="ptrVertices">The pointer referencing the vertices
    /// created during a successful call to <see cref="buildSimpleMesh"/>.
    /// </param>
    /// <param name="ptrIndices">The pointer referencing the triangles
    /// created during a successful call to <see cref="buildSimpleMesh"/>.
    /// </param>
    [DllImport("cai-nmgen-cli")]
    private static extern void freeMesh(ref IntPtr ptrVertices
        , ref IntPtr ptrIndices);

    /// <summary>
    /// Updates a configuration so that it does not include any completely
    /// invalid settings.  Does not guard against a poor quality 
    /// configuration.
    /// </summary>
    /// <remarks>
    /// This method is useful as an initial validation pass.  But that is 
    /// all. Since extreme edge cases are supported by buildSimpleMesh(), 
    /// more validation is likely needed.  For example: A negative 
    /// xzResolution is never valid.  So this method will fix that.  But 
    /// in a tiny number of cases, an xzResolution of 0.01 is valid.  So 
    /// this operation won't fix that, even though in 99.99% of the cases
    /// 0.01 is not valid.
    /// <p>See <see cref="NavmeshConfig"/> for an example of strict validations.
    /// </p>
    /// </remarks>
    /// <param name="config">The configuration to check and update.</param>
    [DllImport("cai-nmgen-cli")]
    private static extern void applyStandardLimits(ref Configuration config);

    /// <summary>
    /// Specifies the configuration used to build navigation meshes.
    /// </summary>
    /// <remarks>
    /// This structure is required to marshal configuration data across
    /// the interop boundry.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    private struct Configuration
    {
        /*
         * Design notes:
         * 
         * WARNING: Interop will break if fields are added, removed, 
         * or re-ordered. (Renaming is OK.)
         * 
         * Keeping this structure private to reduce confusion.
         * External clients use NMGenConfig.
         * 
         * See the NMGenConfig class for API documentation on the fields.
         * 
         */
        public float xzResolution;
        public float yResolution;
        public float minTraversableHeight;
        public float maxTraversableStep;
        public float maxTraversableSlope;
        public float traversableAreaBorderSize;
        public float heightfieldBorderSize;
        public float maxEdgeLength;
        public float edgeMaxDeviation;
        public float contourSampleDistance;
        public float contourMaxDeviation;
        public int smoothingThreshold;
        public int minIslandRegionSize;
        public int mergeRegionSize;
        public int maxVertsPerPoly;
        public bool clipLedges;

    }

    /// <summary>
    /// Creates a triangle navigation mesh based on source geometry.
    /// </summary>
    /// <param name="config">The configuration to use for the build.</param>
    /// <param name="sourceVertices">The source vertices in the form:
    /// (x, y, z).</param>
    /// <param name="sourceIndices">The source triangles in the form:
    /// (vertIndexA, vertIndexB, vertIndexC).</param>
    /// <param name="resultVertices">The navigation mesh vertices in the form:
    /// (x, y, z).  Will be null if the build fails.</param>
    /// <param name="resultIndices">The navigation mesh triangles in the form:
    /// (vertIndexA, vertIndexB, vertIndexC). Will be null if the build
    /// fails.</param>
    /// <param name="messages">Messages from the build process.  Will
    /// be null if the message style disables messages.</param>
    /// <param name="messageStyle">The message style for the messages 
    /// argument.</param>
    /// <returns>TRUE if mesh creation was successful.
    /// </returns>
    public static bool BuildMesh(NavmeshConfig config
        , float[] sourceVertices
        , int[] sourceIndices
        , out float[] resultVertices
        , out int[] resultIndices
        , out string[] messages
        , CAIMessageStyle messageStyle)
    {
        Configuration localConfig = TranslateConfig(config);

        char[] charMsgArray = null;
        switch (messageStyle)
        {
            case CAIMessageStyle.None:
                charMsgArray = null;
                break;
            case CAIMessageStyle.Trace:
                charMsgArray = new char[8000];
                break;
            default:
                charMsgArray = new char[2000];
                break;
        }
		
        IntPtr ptrResultIndices = IntPtr.Zero;
        IntPtr ptrResultVerts = IntPtr.Zero;
        int resultVertLength = 0;
        int resultIndicesLength = 0;
		
        bool success = buildSimpleMesh(localConfig
            , sourceVertices
            , sourceVertices.Length
            , sourceIndices
            , sourceIndices.Length
            , ref ptrResultVerts
            , ref resultVertLength
            , ref ptrResultIndices
            , ref resultIndicesLength
            , charMsgArray
            , (charMsgArray == null ? 0 : charMsgArray.Length)
            , (int)messageStyle);

        if (success)
        {
            resultVertices = new float[resultVertLength];
            Marshal.Copy(ptrResultVerts
                , resultVertices
                , 0
                , resultVertLength);
            resultIndices = new int[resultIndicesLength];
            Marshal.Copy(ptrResultIndices
                , resultIndices
                , 0
                , resultIndicesLength);
            /*
             * WARNING!!!! IMPORTANT!!!
             * Danger Will Robinson!  If you don't call the next function you
             * will be a memory leak causing evil doer.
             */
            freeMesh(ref ptrResultVerts, ref ptrResultIndices);
        }
        else
        {
            resultVertices = null;
            resultIndices = null;
        }

        if (messageStyle != CAIMessageStyle.None)
        {
            messages = ExtractMessages(charMsgArray);
        }
        else
            messages = null;

        return success;
    }

    /// <summary>
    /// Applies the mandatory configuration limits required by the
    /// <see cref="BuildMesh"/>method.
    /// </summary>
    /// <remarks>
    /// This method is useful as an initial validation pass.  But that is all.
    /// Since extreme edge cases are supported by buildSimpleMesh(), more 
    /// validation is likely needed.  For example: A negative xzResolution
    /// is never valid.  So this method will fix that.  But in a tiny number
    /// of cases, an xzResolution of 0.01 is valid.  So this method won't fix
    /// that, even though in 99.99% of the cases 0.01 is not valid.
    /// See <see cref="NavmeshConfig"/> for an example of a more strict final 
    /// validation.
    /// </remarks>
    /// <param name="config">The configuration to check and update.</param>
    public static void ApplyMandatoryLimits(NavmeshConfig config)
    {
        Configuration localConfig = TranslateConfig(config);
        applyStandardLimits(ref localConfig);
        TransferConfig(localConfig, config);
    }

    /// <summary>
    /// Builds a string array of messages from a null terminator delimited
    /// array of characters. (The standard output of a call to 
    /// <see cref="buildSimpleMesh"/>.)
    /// </summary>
    /// <param name="charMessages">The message output array from a
    /// call to <see cref="buildSimpleMesh"/>.</param>
    /// <returns>An array of messages.</returns>
    private static string[] ExtractMessages(char[] charMessages)
    {
        /*
         * Design note:
         * 
         * This algorithm is a work around for an issue that I haven't
         * figured out the proper solution to yet.
         * 
         * When using the Microsoft .NET libraries, the content of the
         * messages array is as expected. Standard latin character set, 
         * one character per char array element.
         * 
         * When using the Mono .NET libraries, the array comes back from
         * the interop call with the characters packed into 8-bit segments.  
         * So each 16-bit char array element contains two char values packed
         * into it. They must be unpacked in order to get the correct string 
         * result.
         * 
         */
        char[] unpacked = charMessages;
        if (charMessages[0] > 0xff)
        {
            // The char's are packed.  Need to unpack them before creating
            // the string.
            unpacked = new char[charMessages.Length * 2];
            for (int i = 0; i < charMessages.Length; i++)
            {
                unpacked[i * 2 + 0] = (char)(charMessages[i] & 0xff);
                unpacked[i * 2 + 1] = (char)((charMessages[i] >> 8) & 0xff);
            }
        }

        string aggregateMsg = new string(unpacked);
        char[] delim = { '\0' };
        return aggregateMsg.Split(delim
            , System.StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Transfers the data from a <see cref="Configuration"/> structure to 
    /// the provided <see cref="NavmeshConfig"/> class.
    /// </summary>
    /// <param name="fromConfig">The source configuration.</param>
    /// <param name="toConfig">The configuration to copy the source 
    /// configuration into.</param>
    private static void TransferConfig(Configuration fromConfig
        , NavmeshConfig toConfig)
    {
        toConfig.clipLedges = fromConfig.clipLedges;
        toConfig.contourMaxDeviation = fromConfig.contourMaxDeviation;
        toConfig.contourSampleDistance = fromConfig.contourSampleDistance;
        toConfig.edgeMaxDeviation = fromConfig.edgeMaxDeviation;
        toConfig.heightfieldBorderSize = fromConfig.heightfieldBorderSize;
        toConfig.maxEdgeLength = fromConfig.maxEdgeLength;
        toConfig.maxTraversableSlope = fromConfig.maxTraversableSlope;
        toConfig.maxTraversableStep = fromConfig.maxTraversableStep;
        toConfig.maxVertsPerPoly = fromConfig.maxVertsPerPoly;
        toConfig.mergeRegionSize = fromConfig.mergeRegionSize;
        toConfig.minIslandRegionSize = fromConfig.minIslandRegionSize;
        toConfig.minTraversableHeight = fromConfig.minTraversableHeight;
        toConfig.smoothingThreshold = fromConfig.smoothingThreshold;
        toConfig.traversableAreaBorderSize = 
            fromConfig.traversableAreaBorderSize;
        toConfig.xzResolution = fromConfig.xzResolution;
        toConfig.yResolution = fromConfig.yResolution;
    }

    /// <summary>
    /// Creates a <see cref="Configuration"/> structure from the content 
    /// of an <see cref="NavmeshConfig"/> object.
    /// </summary>
    /// <param name="config">The configuration used to generation the
    /// structure.</param>
    /// <returns>A configuration structure loaded with the values from the
    /// <see cref="NavmeshConfig"/> object.</returns>
    private static Configuration TranslateConfig(NavmeshConfig config)
    {
        Configuration result = new Configuration();
        result.clipLedges = config.clipLedges;
        result.contourMaxDeviation = config.contourMaxDeviation;
        result.contourSampleDistance = config.contourSampleDistance;
        result.edgeMaxDeviation = config.edgeMaxDeviation;
        result.heightfieldBorderSize = config.heightfieldBorderSize;
        result.maxEdgeLength = config.maxEdgeLength;
        result.maxTraversableSlope = config.maxTraversableSlope;
        result.maxTraversableStep = config.maxTraversableStep;
        result.maxVertsPerPoly = config.maxVertsPerPoly;
        result.mergeRegionSize = config.mergeRegionSize;
        result.minIslandRegionSize = config.minIslandRegionSize;
        result.minTraversableHeight = config.minTraversableHeight;
        result.smoothingThreshold = config.smoothingThreshold;
        result.traversableAreaBorderSize = config.traversableAreaBorderSize;
        result.xzResolution = config.xzResolution;
        result.yResolution = config.yResolution;
        return result;
    }

}
