@echo off

echo *** print working dir
cd
echo.

rem D:\Development\Intern\Tools\ZipCheck\ZipCheck
rem D:\Development\Intern\Tools\ZipCheck\ZipCheck\bin\Release
rem D:\Development\...\Dependencies\BuildTools\ZipCheck

rem 7z64.dll
rem 7z86.dll
rem SevenZipLib.dll
rem ZipCheck.exe

set _SOURCE_DIR=.\bin\Release
set _DEST_DIR=..\..\Test

rem Check
if not exist %_SOURCE_DIR% (
	color 0C
	echo *** ERROR *** No Directory %_SOURCE_DIR% !!!
	goto error
)
if not exist %_DEST_DIR% (
	color 0C
	echo *** ERROR *** No Directory %_DEST_DIR% !!!
	goto error
)

echo.
echo *** copy %_SOURCE_DIR%\*               %_DEST_DIR%\*
copy          %_SOURCE_DIR%\ZipCheck.exe    %_DEST_DIR%\ZipCheck.exe
copy          %_SOURCE_DIR%\SevenZipLib.dll %_DEST_DIR%\SevenZipLib.dll
copy          %_SOURCE_DIR%\7z64.dll        %_DEST_DIR%\7z64.dll
copy          %_SOURCE_DIR%\7z86.dll        %_DEST_DIR%\7z86.dll

echo.
dir                                         %_DEST_DIR%\*.*
goto ende

:error
goto ende

:ende
echo.
echo *** fertig ***
pause
