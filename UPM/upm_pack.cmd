@echo off
rem Simple script to create NPM packages suitable for Unity.
rem Does NOT publish the packages, they will be left in Assets folder.
rem NOTE: Unity must be run on the project at least once, to ensure all
rem necessary .meta files are present.
rem 
rem Run this script from project's .\UPM folder (where it exists).
rem 
rem A new window will be spawned for each package created.
rem After each packaging, inspect the results in the spawned window. 
rem If results look good, close the window (enter "exit") to proceed to next package.
rem
rem After creating and verifying both packages, from command prompt go to Assets folder,
rem and run 
rem    'npm publish com.microsoft.mixedreality.worldlockingtools-1.2.3.tgz'
rem and 
rem    'npm publish com.microsoft.mixedreality.worldlockingsamples-1.2.3.tgz'
rem replacing '-1.2.3' with whatever the current version is.

pushd ..\Assets

rem Stash the .nupkg, it is redundant with the installation of the libs.
mkdir stash
move /Y Packages\Microsoft.MixedReality.Unity.FrozenWorld.Engine.1.1.1\*.nupkg stash >nul
move /Y Packages\Microsoft.MixedReality.Unity.FrozenWorld.Engine.1.1.1\*.nupkg.meta stash >nul

xcopy ..\UPM\core_files . /QY >nul
echo Exit npm window after checking results (enter "exit" on npm window's command line).
start /WAIT npm pack

xcopy ..\UPM\asa_files . /QY >nul
echo Exit npm window after checking results (enter "exit" on npm window's command line).
start /WAIT npm pack

if exist Samples~ (rd/s/q Samples~)
mkdir Samples~
xcopy WorldLocking.Examples Samples~\WorldLocking.Examples /QIS >nul

xcopy ..\UPM\samples_files . /QY >nul
echo Exit npm window after checking results (enter "exit" on npm window's command line).
start /WAIT npm pack

if exist Samples~ (rd/s/q Samples~)
mkdir Samples~
xcopy WorldLocking.ASA.Examples Samples~\WorldLocking.ASA.Examples /QIS >nul

xcopy ..\UPM\asa_samples_files . /QY >nul
echo Exit npm window after checking results (enter "exit" on npm window's command line).
start /WAIT npm pack

rem Cleanup.
rd/s/q Samples~
del package.json
del package.json.meta
del CHANGELOG.*
del LICENSE.*
del NOTICE.*

move /Y stash\* Packages\Microsoft.MixedReality.Unity.FrozenWorld.Engine.1.1.1 >nul
rd/s/q stash

popd

