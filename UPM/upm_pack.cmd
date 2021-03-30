@echo off
rem Simple script to create NPM packages suitable for Unity.
rem Does NOT publish the packages, they will be left in Assets folder.
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

xcopy ..\UPM\core_files . /QY
echo Exit npm window after checking results (enter "exit" on npm window's command line).
start /WAIT npm pack

if exist Samples~ (rd/s/q Samples~)
mkdir Samples~
xcopy WorldLocking.Examples Samples~\WorldLocking.Examples /QIS

xcopy ..\UPM\samples_files . /QY
echo Exit npm window after checking results (enter "exit" on npm window's command line).
start /WAIT npm pack

rem Cleanup.
rd/s/q Samples~
del package.json
del package.json.meta
del CHANGELOG.*
del LICENSE.*
del NOTICE.*

popd
