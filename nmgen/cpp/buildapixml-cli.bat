ECHO OFF
REM Must be run from VS command prompt.

xdcmake ".\cli\obj\Release\*.xdc"  /out:".\bin\Release\cai-nmgen-cli.xml"