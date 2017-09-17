<#

runs validation checks

packages and publishes all public assemblies

#>
    
Param([string]$feedUri, [string] $symbolFeedUri, [string]$feedApiKey)
		
class EditableVersion {
	[int] $Major
	[int] $Minor
    [int] $Patch

    [string] ToString() { 
        return "$($this.Major).$($this.Minor).$($this.Patch)"
    }

    EditableVersion ([EditableVersion] $version) {
        $this.Major = $version.Major
        $this.Minor = $version.Minor
        $this.Patch = $version.Patch
    }

    EditableVersion([version] $version) {
        $this.Major = $version.Major
        $this.Minor = $version.Minor
        $this.Patch = $version.Build
    }
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


function VerifyVersionsInSync {
    
    Param ([array]$projects)

    WriteHostStep "Verifying Versions..."

    $scriptFolder = GetScriptFolder

    $projectZeroVersion = GetProjectVersion $scriptFolder $projects[0]

    foreach ($project in $projects) {
        $projectIVersion = GetProjectVersion $scriptFolder $project
        if ([string]$projectIVersion -ne [string]$projectZeroVersion) {
            throw "Not all project versions are in sync. $($project) [$projectIVersion] does not match $($projects[0]) [$projectZeroVersion]."
        }
    }

    WriteHost "All projects in sync at version $projectZeroVersion"

    return $projectZeroVersion
}


function GetProjectVersion {

    Param([string] $scriptFolder, [string] $project)

    [string] $projectFile = "$scriptFolder\$($project)\$($project).csproj"
        
	$xml=New-Object XML
	$xml.Load($projectFile)

	$currentVersion = [version]$xml.Project.PropertyGroup.Version

    return [EditableVersion]::new($currentVersion)

}
		
function SetWorkingDirectory {

    $folder = GetScriptFolder    
    Push-Location $folder 
    [Environment]::CurrentDirectory = $folder #not set by push-location

    WriteHost "Working Directory set to $folder"
}

function PackProjects {

	Param ([array]$projects)

    WriteHostStep "Packing Projects..."

    $scriptFolder = GetScriptFolder

	foreach($project in $projects) {

		cd $project
		
        WriteHost "Packing $project"

		dotnet pack --configuration Release --include-symbols

		$result = $?

        if ($result -ne $true) { 
        
            throw "Pack for $project failed"             
        }

		SetWorkingDirectory
	}
}

function PublishProjects {

	Param ([array]$projects, [array]$unlistedProjects, [string]$nugetFeedUri, [string]$nugetSymbolFeedUri, [string]$nugetApiKey, [EditableVersion]$latestVersion, [string] $packagePrefix)

    $packagePrefix = $(if ($packagePrefix) { "$packagePrefix." } else { })
    
    WriteHostStep "Publishing Projects... (package prefix: $packagePrefix)"

	$scriptFolder = GetScriptFolder

	foreach($project in $projects) {

		$command = "..\tools\nuget push $scriptFolder\$project\bin\Release\$packagePrefix$project.$latestVersion.nupkg $nugetApiKey -Source $nugetFeedUri"
        WriteHost $command
        iex $command


		if ($unlistedProjects.Contains($project)) {
            $command = "..\tools\nuget delete $project $latestVersion $nugetApiKey -Source $nugetFeedUri"
            WriteHost $command
            iex $command
        }

		$command = "..\tools\nuget push $scriptFolder\$project\bin\Release\$packagePrefix$project.$latestVersion.symbols.nupkg $nugetApiKey -Source $nugetSymbolFeedUri"
        WriteHost $command
        iex $command
	}    
}

function CheckForPublishTag {

    $message = git log -1 --pretty=%B    
    #$message = $(Build.SOURCEVERSIONMESSAGE)

	$result = "$message" -match "Updated Project Versions to";

    return $result
}

function TagCommitAsPublished {

    Param([EditableVersion] $latestVersion)

    WriteHostStep "Tagging Commit with version $latestVersion"
    git tag published-as-version-$latestVersion
    git push --porcelain 
    #see https://stackoverflow.com/questions/12751261/powershell-displays-some-git-command-results-as-error-in-console-even-though-ope for --porcelain switch

    }

#START

#entry method
function Main {

	$projectsToPublish = @(
        "DataStore",		      
        "DataStore.Impl.DocumentDb",
		"DataStore.Impl.SqlServer",
		"DataStore.Impl.Interfaces",
		"DataStore.Impl.Interfaces.LowLevel",
		"DataStore.Models"         	    
    )
    
	$unlistedProjects = @(
		"DataStore.Impl.SqlServer",
		"DataStore.Impl.Interfaces",
		"DataStore.Impl.Interfaces.LowLevel",
		"DataStore.Models" 
    )

	SetWorkingDirectory

	if (CheckForPublishTag -eq $true) {		

		$latestVersion = VerifyVersionsInSync $projectsToPublish
		    
		PackProjects $projectsToPublish

		PublishProjects $projectsToPublish $unlistedProjects $feedUri $symbolFeedUri $feedApiKey $latestVersion

        TagCommitAsPublished $latestVersion
	}
        
    Write-Host "Done."        
}

#go
Main
