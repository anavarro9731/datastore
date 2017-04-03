mkdir c:\Nuget.Local

cd DataStore
del bin\DEBUG\*.nupkg
dotnet pack -c DEBUG 
copy bin\DEBUG\*.nupkg c:\Nuget.Local
																																																																																																																																																																		
cd ..
cd DataStore.Interfaces
del bin\DEBUG\*.nupkg
dotnet pack -c DEBUG 
copy bin\DEBUG\*.nupkg c:\Nuget.Local

cd ..
cd DataStore.Interfaces.LowLevel
del bin\DEBUG\*.nupkg
dotnet pack -c DEBUG 
copy bin\DEBUG\*.nupkg c:\Nuget.Local

cd ..
cd DataStore.Impl.DocumentDb
del bin\DEBUG\*.nupkg
dotnet pack -c DEBUG 
copy bin\DEBUG\*.nupkg c:\Nuget.Local

cd ..
cd DataStore.Models
del bin\DEBUG\*.nupkg
dotnet pack -c DEBUG 
copy bin\DEBUG\*.nupkg c:\Nuget.Local

cd ..
