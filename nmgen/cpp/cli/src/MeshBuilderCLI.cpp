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
#include "NMGen.h"
#include "NMGenCLI.h"
#include "Recast.h"

namespace org 
{
namespace critterai
{
namespace nmgen
{
        /// <summary>
        /// Builds a simple triangle navigation mesh based on the provided
        /// source geometry.
        /// </summary>
        /// <param name="sourceVerts">
        /// The source geometry vertices in the form (x, y, z).
        /// </param>
        /// <param name="sourceTriangles">
        /// The source geometry triangles in the form 
        /// (vertAIndex, vertBIndex, vertCIndex).
        /// </param>
        /// <param name="config">
        /// The configuration to use when generating the navigation mesh.
        /// </param>
        /// <param name="messages">
        /// A list to load the build messages into.  Can be null.
        /// </param>
        /// <param name="messageDetail">
        /// A value between 0 and 3 which indicates the level of detail for
        /// messages.  (0 = Messages disabled, 3 = Trace level of detail)
        /// </param>
        /// <returns>
        /// A navigation mesh built from the source geometry and configuation.
        /// </returns>
    TriangleMesh^ MeshBuilder::BuildSimpleMesh(
        array<float>^ sourceVerts
        , array<int>^ sourceTriangles
        , BuildConfig^ config
        , List<String^>^ messages
        , int messageDetail)
    {

        messages->Clear();

        // Transfer configuration data to native configuration 
        // structure.
        Configuration rcc;
        rcc.xzResolution = config->XZResolution;
	    rcc.yResolution = config->YResolution;
        rcc.maxTraversableSlope = config->MaxTraversableSlope;
        rcc.minTraversableHeight = config->MinTraversableHeight;
        rcc.maxTraversableStep = config->MaxTraversableStep;
        rcc.traversableAreaBorderSize = config->TraversableAreaBorderSize;
        rcc.maxEdgeLength = config->MaxEdgeLength;
        rcc.edgeMaxDeviation = config->EdgeMaxDeviation;
        rcc.minUnconnectedRegionSize = config->MinUnconnectedRegionSize;
        rcc.mergeRegionSize = config->MergeRegionSize;
	    rcc.maxVertsPerPoly = config->MaxVertsPerPoly;
        rcc.contourSampleDistance = config->ContourSampleDistance;
        rcc.contourMaxDeviation = config->ContourMaxDeviation;
        rcc.heightfieldBorderSize = config->HeightfieldBorderSize;
        rcc.smoothingThreshold = config->SmoothingThreshold;
        rcc.clipLedges = config->ClipLedges;

        // Prepare arrays.
        const pin_ptr<float> pSourceVerts = &sourceVerts[0];
	    const pin_ptr<int> pSourceTriangles = &sourceTriangles[0];

        array<char>^ charMsgs = gcnew array<char>(10000);
        const pin_ptr<char> ptrCharMsgs = &charMsgs[0];

        // Perform the build.

        TriangleMesh^ finalMesh = nullptr;
        float** ppResultVerts = new float*;
        int** ppResultTriangles = new int*;
        int resultVertLength = 0;
        int resultTrianglesLength = 0;

        if (buildSimpleMesh(rcc
            , pSourceVerts
            , sourceVerts->Length
            , pSourceTriangles
            , sourceTriangles->Length
            , ppResultVerts
            , &resultVertLength
            , ppResultTriangles
            , &resultTrianglesLength
            , ptrCharMsgs
            , charMsgs->Length
            , messageDetail))
        {
            // Success: Transfer data to output.
            finalMesh = gcnew TriangleMesh(resultVertLength
                , resultTrianglesLength);
            
            float* pVerts = *ppResultVerts;
            for (int i = 0; i < resultVertLength; i++)
            {
                finalMesh->vertices[i] = pVerts[i];
            }
            int* pTriangles = *ppResultTriangles;
            for (int i = 0; i < resultTrianglesLength; i++)
            {
                finalMesh->triangles[i] = pTriangles[i];
            }
            freeMesh(ppResultVerts, ppResultTriangles);
        }
        
        delete ppResultVerts;
        ppResultVerts = 0;
        delete ppResultTriangles;
        ppResultTriangles = 0;

        /*
         * Split the character array of messages into an array of string
         * messages.  Then add the values to the output message list.
         *
         * Note: Must use the long format of the string constructor, otherwise
         * the string will only be constructed up to the first null terminator.
         * Also, when debugging, the string message will only display the
         * first message.  But it actually contains the content of the entire 
         * char array.
         */
        if (messageDetail > 0)
        {
            String^ strMessage = gcnew String(ptrCharMsgs, 0, charMsgs->Length);
            messages->Add(strMessage);
            array<wchar_t>^ delim = { '\0' };
            array<String^>^ strMsgs = 
                strMessage->Split(delim, StringSplitOptions::RemoveEmptyEntries);
            for (int i = 0; i < strMsgs->Length; i++)
            {
                messages->Add(strMsgs[i]);
            }
        }

        return finalMesh;
    }
}
}
}

