mkdir c:\Nuget.Local

cd DataStore
del bin\RELEASE\*.nupkg
dotnet pack -c RELEASE --include-symbols
copy bin\RELEASE\*.nupkg c:\Nuget.Local
																																																																																																																																																																		
cd ..
cd DataStore.Interfaces
del bin\RELEASE\*.nupkg
dotnet pack -c RELEASE --include-symbols
copy bin\RELEASE\*.nupkg c:\Nuget.Local

cd ..
cd DataStore.Impl.DocumentDb
del bin\RELEASE\*.nupkg
dotnet pack -c RELEASE --include-symbols
copy bin\RELEASE\*.nupkg c:\Nuget.Local

cd ..
cd DataStore.Impl.SqlServer
del bin\RELEASE\*.nupkg
dotnet pack -c RELEASE --include-symbols
copy bin\RELEASE\*.nupkg c:\Nuget.Local

cd ..
cd DataStore.Models
del bin\RELEASE\*.nupkg
dotnet pack -c RELEASE --include-symbols
copy bin\RELEASE\*.nupkg c:\Nuget.Local

cd ..
cd DataStore.Interfaces.LowLevel
del bin\RELEASE\*.nupkg
dotnet pack -c RELEASE --include-symbols
copy bin\RELEASE\*.nupkg c:\Nuget.Local

cd ..
