$ErrorActionPreference = 'Stop';
$null = New-Item -Path temp -ItemType Directory -Force;
Write-Host "Downloading...";
Invoke-WebRequest -Uri "https://file.xunkong.cc/download/windsong/WindSong-CI.zip" -OutFile temp/WindSong-CI.zip;
Expand-Archive -Path temp/WindSong-CI.zip -DestinationPath temp -Force;
Move-Item -Path temp/WindSong-CI/* -Destination ./ -Force;
Write-Host "Update is completed." -ForegroundColor Green;
Remove-Item -Path temp -Force -Recurse;