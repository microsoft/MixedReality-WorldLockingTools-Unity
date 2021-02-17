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

xcopy package_core.json package.json /YQ
xcopy package_core.json.meta package.json.meta /YQ
echo Close npm window after checking results.
start /WAIT npm pack

xcopy package_samples.json package.json /YQ
xcopy package_samples.json.meta package.json.meta /YQ
echo Close npm window after checking results.
start /WAIT npm pack

rem Cleanup.
del package.json
del package.json.meta

popd
