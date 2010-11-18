This directory contains the native and C++/CLI code for NMGen.

Directorys:

cli - C++/CLI code.
native - Native code.
Recast - Recast base code.

Code from all directories is required to build the .NET mixed mode version which
which includes both the .NET and interop API's.

If the code in the cli directory is exlcuded, the remaining code can be used to
build a native library.  (If the compiler supports pragma once.)

The main native functions are exported as C functions to support use in
Unity3D pro.

