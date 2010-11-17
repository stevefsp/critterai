The main directory structure is broken down by project then platform/language.

For example: 

The java utility project is under util/java.
The Unity3D utility project is under util/u3d.

Individual distributions are structured as follows:

project directory
    planform directory
        src directory   : Project source.
        test directory  : Test source.

Java Specific

Ant for builds.
JUnit for testing.

.NET Specific

No automatic builds or packaging.
Visual Studio Test for testing.
Sandcastle for documentation build.
