@echo off
SET targetPath=.\UnityProjects\MRTKDevTemplate
SET linkPath=.\HorizonVision

mklink /D "%linkPath%" "%targetPath%"
PAUSE