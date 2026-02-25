$LocalPublishPath = ".\bin\release\net8.0\publish\"

$nupkgFile  = $LocalPublishPath + (Get-ChildItem -Path $LocalPublishPath -Filter "*.nupkg"  -File | Sort-Object LastWriteTime -Descending | Select-Object -First 1).name
$snupkgFile = $LocalPublishPath + (Get-ChildItem -Path $LocalPublishPath -Filter "*.snupkg" -File | Sort-Object LastWriteTime -Descending | Select-Object -First 1).name

$githubPAT = Get-Content -Path ".\.github.pat"

$tmp = "Publishing: " + $nupkgFile
echo $tmp
dotnet nuget push $nupkgFile  --source "github" --api-key $githubPAT --skip-duplicate

echo " "
echo " "
$tmp = "Publishing: " + $snupkgFile
echo $tmp
dotnet nuget push $snupkgFile --source "github" --api-key $githubPAT --skip-duplicate