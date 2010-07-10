package org.critterai.nav;

import static org.junit.Assert.assertTrue;
import static org.junit.Assert.fail;

import org.junit.Test;

public final class QuickMeshTest 
{

    /*
     * Design Notes:
     * 
     * This test suite does not test exception handling.
     * It also does not attempt to test both the class loader and file IO
     * functionalities.
     * 
     */

    private static final String TEST_FILE_NAME = "quickmesh.txt";

    @Test
    public void TestConstructorA()
    {
        QuickMesh m = null;
        try 
        {
            m = new QuickMesh(TEST_FILE_NAME, false);
        } 
        catch (Exception e) 
        {
            fail(e.toString());
        }
        
        assertTrue(m.indices.length == 6);
        assertTrue(m.verts.length == 4 * 3);

        assertTrue(m.verts[0] == 0.01f);
        assertTrue(m.verts[1] == 0.1f);
        assertTrue(m.verts[2] == 0.0f);
        assertTrue(m.verts[3] == 0.02f);
        assertTrue(m.verts[4] == 0.003f);
        assertTrue(m.verts[5] == 1.0f);
        assertTrue(m.verts[6] == 1.02f);
        assertTrue(m.verts[7] == 1.0f);
        assertTrue(m.verts[8] == 1.01f);
        assertTrue(m.verts[9] == 1.0f);
        assertTrue(m.verts[10] == -1.03f);
        assertTrue(m.verts[11] == 0.0f);

        assertTrue(m.indices[0] == 0);
        assertTrue(m.indices[1] == 2);
        assertTrue(m.indices[2] == 3);
        assertTrue(m.indices[3] == 0);
        assertTrue(m.indices[4] == 1);
        assertTrue(m.indices[5] == 2);
    }

    @Test
    public void TestConstructorB()
    {
        QuickMesh m = null;
        try 
        {
            m = new QuickMesh(TEST_FILE_NAME, true);
        } 
        catch (Exception e) 
        {
            fail(e.toString());
        }

        assertTrue(m.indices.length == 6);
        assertTrue(m.verts.length == 4 * 3);

        assertTrue(m.verts[0] == 0.01f);
        assertTrue(m.verts[1] == 0.1f);
        assertTrue(m.verts[2] == 0.0f);
        assertTrue(m.verts[3] == 0.02f);
        assertTrue(m.verts[4] == 0.003f);
        assertTrue(m.verts[5] == 1.0f);
        assertTrue(m.verts[6] == 1.02f);
        assertTrue(m.verts[7] == 1.0f);
        assertTrue(m.verts[8] == 1.01f);
        assertTrue(m.verts[9] == 1.0f);
        assertTrue(m.verts[10] == -1.03f);
        assertTrue(m.verts[11] == 0.0f);

        assertTrue(m.indices[0] == 0);
        assertTrue(m.indices[1] == 3);
        assertTrue(m.indices[2] == 2);
        assertTrue(m.indices[3] == 0);
        assertTrue(m.indices[4] == 2);
        assertTrue(m.indices[5] == 1);
    }
}

