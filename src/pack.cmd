mkdir c:\Nuget.Local

cd DataStore
del bin\RELEASE\*.nupkg
dotnet pack -c RELEASE 
copy bin\RELEASE\*.nupkg c:\Nuget.Local

cd ..
cd DataStore.Infrastructure
del bin\RELEASE\*.nupkg
dotnet pack -c RELEASE 
copy bin\RELEASE\*.nupkg c:\Nuget.Local

cd ..
cd DataStore.Interfaces
del bin\RELEASE\*.nupkg
dotnet pack -c RELEASE 
copy bin\RELEASE\*.nupkg c:\Nuget.Local

cd ..
cd DataStore.Impl.DocumentDb
del bin\RELEASE\*.nupkg
dotnet pack -c RELEASE 
copy bin\RELEASE\*.nupkg c:\Nuget.Local

cd ..
cd DataStore.Addons
del bin\RELEASE\*.nupkg
dotnet pack -c RELEASE 
copy bin\RELEASE\*.nupkg c:\Nuget.Local

cd ..
cd DataStore.Models
del bin\RELEASE\*.nupkg
dotnet pack -c RELEASE 
copy bin\RELEASE\*.nupkg c:\Nuget.Local

cd ..
