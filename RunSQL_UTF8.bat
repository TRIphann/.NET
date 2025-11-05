@echo off
chcp 65001
echo ========================================
echo CHAY SQL SCRIPT VOI UTF-8 ENCODING
echo ========================================
echo.

sqlcmd -S PCT\SQLEXPRESS -U sa -P 123456789 -i "D:\sql\PhanTichThietKEYC\QLDuLichRBAC_Final\lib\QLDuLichRBAC_Final.sql" -f 65001

echo.
echo ========================================
echo HOAN THANH!
echo ========================================
pause
