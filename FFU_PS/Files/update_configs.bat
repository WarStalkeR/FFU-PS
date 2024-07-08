@echo off
setlocal

set "folder_cfg=Configs"
set "deploy_local=..\..\..\..\Mods\FFU_PS"
set "deploy_remote=..\..\..\..\..\..\workshop\content\2059170\3283318778"

rmdir /S /Q "%deploy_local%\%folder_cfg%"
rmdir /S /Q "%deploy_remote%\%folder_cfg%"

for /R "%folder_cfg%" %%f in (*.csv) do (
    echo F | xcopy /F /Y "%%f" "%deploy_local%\%folder_cfg%\%%~xnf"
    echo F | xcopy /F /Y "%%f" "%deploy_remote%\%folder_cfg%\%%~xnf"
)

endlocal