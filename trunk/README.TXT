Each project has its own root directory which is located within a directory
indicating its language or target platform.

For example: 

The java utility project is under /java/util.
The Unity3D utility project is under /u3d/util.

Java Projects

For Java the standard project directory structure is as follows:

project directory
    build files     : Ant
    src directory   : Project source.
    test directory  : Test source.

Visual Studio Based Projects

Visual Studio based projects are split into two directories,
one for the main project source and another the test code.

E.g.  /u3d/util and /u3d/util-test.

The test project references the base project.


    

