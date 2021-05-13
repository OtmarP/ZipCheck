@echo off

rem Check
if not exist .\ZipCheck.exe (
	color 0C
	echo *** ERROR *** No File .\ZipCheck.exe !!!
	goto error
)
if not exist AndroidApp.AndroidApp.apk (
	color 0C
	echo *** ERROR *** No File AndroidApp.AndroidApp.apk !!!
	goto error
)

echo *** Check...
.\ZipCheck.exe AndroidApp.AndroidApp2.apk AndroidManifest.xml   -s:manifest  -sdl:105
echo ERRORLEVEL=%ERRORLEVEL%
echo.

.\ZipCheck.exe AndroidApp.AndroidApp.apk AndroidManifest.xml   -s:manifest  -sdl:105
echo ERRORLEVEL=%ERRORLEVEL%
echo.

.\ZipCheck.exe AndroidApp.AndroidApp.apk assets\app.config.xml -s:UpdateUrl -sdl:45
echo ERRORLEVEL=%ERRORLEVEL%
echo.

goto ende

:error
goto ende

:ende
echo.
echo *** fertig ***
pause
