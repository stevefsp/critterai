All projects in this directory depend on Unity3D (http://unity3d.com/).
They all work with the free version.

Build Notes:

All builds target .NET 2.0.

The projects were created on a 64-bit version of Windows, so the
Unity3D DLL reference may be broken on 32-bit Windows.
The expected location of the DLL for both operating systems is as follows:

Windows 32-bit: C:\Program Files\Unity\Editor\Data\lib\UnityEngine.dll
Windows 64-bit: C:\Program Files (x86)\Unity\Editor\Data\lib\UnityEngine.dll

The following directory must be included in your enviornment in order to
perform certain builds:

\lib\cs

