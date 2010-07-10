package org.critterai.nav;

import java.io.BufferedReader;
import java.io.FileReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.util.ArrayList;
import java.util.regex.Pattern;

import org.critterai.math.Vector3;

/**
 * Creates a mesh from a file in a simplified wavefront format.
 * <p>Only the "v" and "f" entries are recognized.  All others are ignored.</p>
 * <p>The v entries are expected to be in one of the following forms:
 * "v x y z w" or "v x y z"</p>
 * <p>The f entries are expected to be in one of the following forms: 
 * "f  v1/vt1/vn1 v2/vt2/vn2 v3/vt3/vn3" or "f v1 v2 v3" Only the vertex
 * portions of the entries are recognized.  Also, only positive indices are
 * supported.  The vertex indices cannot be negative.</p>
 */
public final class QuickMesh
{
    /**
     * The mesh vertices in the form (x, y, z).
     */
    public final float[] verts;
    
    /**
     * The mesh indices in the form (v1, v2, v3).
     */
    public final int[] indices;
    
    /**
     * The minimum bounds of the loaded mesh.
     */
    public final Vector3 minBounds;

    /**
     * The maximum bounds of the loaded mesh.
     */
    public final Vector3 maxBounds;

    /**
     * Constructor
     * <p>The first search location is the class path via the class loader.  
     * If this fails, then standard file IO is attempted.</p>
     * @param path The path of the wavefront formatted file to load.
     * @param keepWrapDirection If TRUE, the wrap direction of the triangles is
     * maintained.  Otherwise they are reversed.  E.g. If the wrap direction in
     * the text asset is counter-clockwise, setting this value to TRUE will 
     * convert the triangles to clockwise.
     * @throws IOException If there is a problem with the path or reading the
     * file.
     * @throws Exception If there is a problem with the content of the file.
     */
    public QuickMesh(String path, Boolean keepWrapDirection) 
        throws IOException, Exception
    {
        InputStream is = QuickMesh.class.getClassLoader()
                .getResourceAsStream(path);
        BufferedReader reader = null;
        if (is != null)
        {
            reader = new BufferedReader(new InputStreamReader(is));
        }
        else
        {
            reader = new BufferedReader(new FileReader(path));
        }

        ArrayList<Float> lverts = new ArrayList<Float>();
        ArrayList<Integer> lindices = new ArrayList<Integer>();

        Pattern r = Pattern.compile("\\s+");
        Pattern rs = Pattern.compile("\\/");

        String line;
        int lineCount = 0;
        while ((line = reader.readLine()) != null)
        {
            lineCount++;
            String errPrefix = "Invalid vertex entry at line " 
                + lineCount + ".";
            String s = line.trim();
            String[] tokens = null;
            if (s.startsWith("v "))
            { 
                // Vertex entry.  Expecting one of: 
                // v x y z w
                // v x y z
                tokens = r.split(s);
                if (tokens.length < 4)
                    throw new Exception(errPrefix + "Too few fields.");
                for (int i = 1; i < 4; i++)
                {
                    String token = tokens[i];
                    try
                    {
                        lverts.add(Float.parseFloat(token));
                    }
                    catch (Exception e)
                    {
                        if (e == null) { }
                        throw new Exception(errPrefix 
                                + " Field is not a valid float.");
                    }
                }
            }
            else if (s.startsWith("f "))
            {
                // This is a face entry.  Expecting one of:
                // F  v1/vt1/vn1   v2/vt2/vn2   v3/vt3/vn3
                // F  v1 v2 v3
                errPrefix = "Invalid face entry at line " + lineCount + ".";
                tokens = r.split(s);
                if (tokens.length < 4)
                    throw new Exception(errPrefix + "Too few fields.");
                for (int i = 1; i < 4; i++)
                {
                    String token = tokens[i];
                    String[] subtokens = rs.split(token);
                    try
                    {
                        // Subtraction converts from 1-based index to 
                        // zero-based index.
                        lindices.add(Integer.parseInt(subtokens[0]) - 1);
                    }
                    catch (Exception e)
                    {
                        if (e == null) { }
                        throw new Exception(errPrefix 
                                + " Field is not a valid integer.");
                    }
                }
            }
        }

        verts = new float[lverts.size()];
        for (int i = 0; i < verts.length; i++)
            verts[i] = lverts.get(i);

        indices = new int[lindices.size()];
        for (int i = 0; i < indices.length; i++)
            indices[i] = lindices.get(i);
        
        // Default wrap direction for wavefront files is CCW.  We
        // want CW.
        if (!keepWrapDirection)
        {
            // Convert from counterclockwise to clockwise.
            for (int p = 1; p < indices.length; p += 3)
            {
                int t = indices[p];
                indices[p] = indices[p + 1];
                indices[p + 1] = t;
            }
        }
        
        minBounds = new Vector3(verts[0], verts[1], verts[2]);
        maxBounds = new Vector3(verts[0], verts[1], verts[2]);
        for (int pVert = 3; pVert < verts.length; pVert += 3)
        {
            minBounds.x = Math.min(minBounds.x, verts[pVert + 0]);
            minBounds.y = Math.min(minBounds.y, verts[pVert + 1]);
            minBounds.z = Math.min(minBounds.z, verts[pVert + 2]);
            maxBounds.x = Math.max(maxBounds.x, verts[pVert + 0]);
            maxBounds.y = Math.max(maxBounds.y, verts[pVert + 1]);
            maxBounds.z = Math.max(maxBounds.z, verts[pVert + 2]);
        }
    }
}
