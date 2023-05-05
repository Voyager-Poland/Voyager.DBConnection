$version='2.5.0'
dotnet build -c Release /property:Version=$version
dotnet pack -c Release /property:Version=$version

$ostatniPakiet = (gci bin\Release\*.nupkg | select -last 1).Name
$sciezka = "bin\Release\$ostatniPakiet"

dotnet nuget push "$sciezka" -s Voyager-Poland