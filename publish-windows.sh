#!/bin/bash
# Xuất bản game thành 1 file .exe cho standee Windows (chạy được ngay trên Mac M1)
set -e
cd "$(dirname "$0")"
dotnet publish -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
  -o publish/win-x64
echo ""
echo "Xong! Copy cả thư mục publish/win-x64 sang standee (DrCeuticsQuiz.exe + questions.json)."
open publish/win-x64 2>/dev/null || true
