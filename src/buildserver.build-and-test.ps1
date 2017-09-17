<#

restore depdendent nuget packages

builds solution

runs domain tests against all test projects 

#>

function SetWorkingDirectory {

    $folder = GetScriptFolder    
    Push-Location $folder 
    [Environment]::CurrentDirectory = $folder #not set by push-location

    WriteHost "Working Directory set to $folder"
}


function GetScriptFolder {
    function GetScriptPath {
        $path = $MyInvocation.MyCommand.Path
        if ($path) {
            return $path
        } else {
            $path = $MyInvocation.ScriptName
            if ($path) {
                return $path
            } else {
                throw "Cannot determine script path"
            }
        }    
    }
    $scriptFolder = Split-Path $(GetScriptPath)
    
    return $scriptFolder
}

function WriteHost {

	Param ([string] $msg)

	Write-Host $msg -foregroundcolor green 
}

function WriteHostStep {

	Param ([string] $msg)

	Write-Host $msg -foregroundcolor cyan -backgroundcolor black
}

function RestoreNugetPackages {

    WriteHostStep "Restoring Nuget Packages..."
    
    dotnet restore
}

function BuildAllProjects {

    WriteHostStep "Building All Projects..." 
    #if you do not clear the duild directory each time, it's important to build before packing, otherwise you might package an older version of a dependent assembly before the dependent assembly has been rebuilt.

    dotnet build

}

function RunDomainTests {

    Param([array]$testProjects)
   
    WriteHostStep "Running Tests..."

    foreach($project in $testProjects) {

        WriteHost "Running Tests for $project"

        cd $project

        dotnet test --no-build #already built

        $result = $?

        if ($result -ne $true) { 
        
            throw "Test run for $project failed"             
        }

        SetWorkingDirectory

    }
}
        

#START

#entry method
function Main {

    $testPackages = @(
        "DataStore.Tests"         	         	    
    )
    

    SetWorkingDirectory

    RestoreNugetPackages

    BuildAllProjects

    RunDomainTests $testPackages
        
    Write-Host "Done."        
}

#go
Main
