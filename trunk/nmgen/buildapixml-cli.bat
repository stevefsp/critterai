ECHO OFF
REM Must be run from VS command prompt.
REM Only applies to the core library's .NET documentation.

xdcmake ".\cpp\cli\obj\Release\*.xdc"  /out:".\cpp\bin\Release\cai-nmgen-cli.xml"