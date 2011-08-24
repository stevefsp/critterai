>> Directory Layout <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

build

    Contains build scripts and Visual Studio projects for active projects.

doc

    Documentation source and project files.

legacy

    Older projects that are no longer being actively worked on.

src

    Source for active projects.

    The source directory layout is a bit odd.  Most C# source is located
    under  /src/main/Assets/CAI.  This layout mimics the layout within
    a Unity project.  It is structured in this way to support easy source
    deployment to non-Windows Unity projects which can't use the pre-compiled
    DLL's.

>> .NET Notes <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

No automatic builds or packaging.
Visual Studio Test for testing.
Sandcastle for documentation build.

DOCUMENTATION BUILD NOTES

The following tools are required to build the API documentation:

Sandcastle: http://sandcastle.codeplex.com/
Sandcastle Help File Builder: http://shfb.codeplex.com/

Sandcastle Styles Patch

http://sandcastlestyles.codeplex.com/ - Home Page
http://sandcastlestyles.codeplex.com/releases/view/47767 - Patch Used

>> Unity Notes <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


Projects targeted to the 'u3d' platform depend on Unity (http://unity3d.com/).
Some require Unity Pro.  See project home for details. (http://critterai.org/)

The Visual Studio projects were created on a 64-bit version of Windows, so the
Unity DLL reference may be broken on 32-bit Windows.

For Unity 3.0, the expected location of the DLL for both operating systems is 
as follows:

Windows 32-bit: C:\Program Files\Unity\Editor\Data\Managed
Windows 64-bit: C:\Program Files (x86)\Unity\Editor\Data\Managed\


