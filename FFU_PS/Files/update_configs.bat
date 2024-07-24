@echo off
setlocal

set "folder_cfg=Configs"
set "folder_loc=Localization"
set "deploy_local=..\..\..\_Release\FFU_Phase_Shift"
set "deploy_remote=..\..\..\..\..\..\workshop\content\2059170\3283318778"

rmdir /S /Q "%deploy_local%\%folder_cfg%"
rmdir /S /Q "%deploy_local%\%folder_loc%"

rmdir /S /Q "%deploy_remote%\%folder_cfg%"
rmdir /S /Q "%deploy_remote%\%folder_loc%"

for /R "%folder_cfg%" %%f in (*.csv) do (
    echo F | xcopy /F /Y "%%f" "%deploy_local%\%folder_cfg%\%%~xnf"
    echo F | xcopy /F /Y "%%f" "%deploy_remote%\%folder_cfg%\%%~xnf"
)

for /R "%folder_loc%" %%f in (*.json) do (
    echo F | xcopy /F /Y "%%f" "%deploy_local%\%folder_loc%\%%~xnf"
    echo F | xcopy /F /Y "%%f" "%deploy_remote%\%folder_loc%\%%~xnf"
)

endlocal