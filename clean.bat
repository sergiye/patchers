@echo off

rmdir /s /q .vs
rmdir /s /q .idea

rmdir /s /q .\resharperPatcher\.vs
rmdir /s /q .\resharperPatcher\obj
rmdir /s /q .\resharperPatcher\bin

del /S ".\resharperPatcher\FodyWeavers.xsd"
rem del /S ".\resharperPatcher\bin\*.dll"
rem del /S ".\resharperPatcher\bin\*.pdb"
rem del /S ".\resharperPatcher\bin\*.xml"
