@echo off
rem Simple script to create NPM packages suitable for Unity.
rem Does NOT publish the packages, they will be left in Assets folder.
rem 
rem Run this script from project's pipelines\scripts folder (where it exists).
rem 
rem A new window will be spawned for each package created.
rem After each packaging, inspect the results in the spawned window. 
rem If results look good, close the window to proceed to next package.
pushd ..\..\Assets

copy package_core.json package.json /Y
copy package_core.json.meta package.json.meta /Y
echo Exit npm window after checking results (enter "exit" on command line).
start /WAIT npm pack

copy package_samples.json package.json /Y
copy package_samples.json.meta package.json.meta /Y
echo Exit npm window after checking results (enter "exit" on command line).
start /WAIT npm pack

rem Cleanup.
del package.json
del package.json.meta

popd
