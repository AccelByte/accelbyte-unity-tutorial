@echo off

::Add CONDITION with "y" value for one time config
set CONDITION=n

echo Make sure you have build the project with the BuildProject.bat or Unity Editor
echo You can also change the batch file to config the path manually
echo:

if %CONDITION% == y goto OneTimeConfig
if %CONDITION% == n goto UserInputConfig

:OneTimeConfig
::Remember to set PROJECTPATH
set PROJECTPATH="<YourProjectPath>\Justice-Unity-Tutorial-Project"
goto Start

:UserInputConfig
echo Enter the Root project path. (Example= C:\justice-unity-tutorial-game\Justice-Unity-Tutorial-Project) 
set /p PROJECTPATH=

:Start
::Set client and server path based on PROJECTPATH
set CLIENTPATH=%PROJECTPATH%\Build\Client\Justice-Unity-Tutorial-Project.exe
set SERVERPATH=%PROJECTPATH%\Build\Server\Justice-Unity-Tutorial-Project.exe

::OPEN CLIENT 1
start "Local Client 1" %CLIENTPATH% local

::OPEN CLIENT 2
start "Local Client 2" %CLIENTPATH% local

::OPEN CLIENT 3
start "Local Client 3" %CLIENTPATH% local

::OPEN CLIENT 4
start "Local Client 4" %CLIENTPATH% local

::OPEN LOCAL SERVER
start "Local Server" %SERVERPATH% local
