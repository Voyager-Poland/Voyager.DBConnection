$version='3.0.0'
dotnet build -c Release   /property:Version=$version
dotnet pack -c Release /property:Version=$version

$ostatniPakiet = (gci .\src\Voyager.DBConnection.MsSql\bin\Release\*.nupkg | select -last 1).Name
$sciezka = ".\src\Voyager.DBConnection.MsSql\bin\Release\$ostatniPakiet"

dotnet nuget push "$sciezka" -s Voyager-Poland
