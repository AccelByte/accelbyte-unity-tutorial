@echo off

::Add CONDITION with "y" value for one time config
set CONDITION=n

echo Make sure you have add client secret in the Assets/Resources/AccelByteServerSDKConfig.json
echo You can also change the batch file to config the path manually
echo:

if %CONDITION% == y goto OneTimeConfig
if %CONDITION% == n goto UserInputConfig

:OneTimeConfig
::Remember to set ENGINEPATH AND PROJECTPATH if you choose one time config
set ENGINEPATH="<YourEnginePath>\Editor\Unity.exe"
set PROJECTPATH="<YourProjectPath>\Justice-Unity-Tutorial-Project"
goto Build

:UserInputConfig
echo Enter the Unity Engine path (Example= C:\Unity\2019.4.30f1\Editor\Unity.exe)
set /p ENGINEPATH=

echo Enter the Root project path. (Example= C:\justice-unity-tutorial-game\Justice-Unity-Tutorial-Project) 
set /p PROJECTPATH=

:Build
echo Build the Client. Please wait . . .
%ENGINEPATH% -quit -batchmode -nographics -wait -projectPath %PROJECTPATH% -executeMethod BuildClient.PerformBuild -logFile "Justice-Unity-Tutorial-Project\Build\Log\BuildClient.log"

echo Build the Server. Please wait . . .
%ENGINEPATH% -quit -batchmode -nographics -wait -projectPath %PROJECTPATH% -executeMethod BuildServer.PerformBuild -logFile "Justice-Unity-Tutorial-Project\Build\Log\BuildServer.log"

endlocal
echo Build process is done