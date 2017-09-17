<# 

This script is for versioning the package before publishing.
WARNING: It should only ever be run while on a RELEASE, HOTFIX or DEVELOP branch. It should never be run from a FEATURE branch.

It will update the version, commit the change, and push the commit to the origin.
(While no one should be developing on develop, if you don't push right away you may risk concurrency issues with other developers.
If you don't commit right away you might pollute the commit.)

See Readme - Versioning Semantics for more details
#>


enum VersionAction {
    ReleaseHotfixCommit
    ReleaseStableCommit
    ReleaseTestCommit
}

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

function AskYesNo {

	Param ($question)

    Write-Host "QUESTION" -foregroundcolor yellow -backgroundcolor black
	$confirmation = Read-Host "$question [y/n]?" 

	while($confirmation -ne "y" -and $confirmation -ne "n")
	{
        Write-Host "QUESTION" -foregroundcolor yellow -backgroundcolor black
		$confirmation = Read-Host "$question [y/n]"  
	}

	if ($confirmation -eq "y") {
        return $true
    } else {
        return $false
    }
}


function CalcVersionAction {

	function CalcVersionActionInner {
        if (AskYesNo("Is this a hotfix?") -eq $true) {
            return [VersionAction]::ReleaseHotfixCommit
        }

        if (AskYesNo("Is this version a major version?") -eq $true) {
            return [VersionAction]::ReleaseStableCommit
        }
        return [VersionAction]::ReleaseTestCommit
    }

    $action = CalcVersionActionInner

    return $action
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

function VerifyIndexAndTreeIsClean {

    WriteHostStep "Verify there are no outstanding change on this branch..."

    $check = "git status --porcelain"
    $result = iex $check
    WriteHost $result
    if (![string]::IsNullOrEmpty($result)) { throw "You cannot have outstanding changes on your currenct git branch if you want to run this script." }
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

function CalcNewVersion {
    
    Param([EditableVersion] $currentVersion)

    WriteHostStep "Calculating New Version..."

    $calcVersionAction = CalcVersionAction

    $newVersion = [EditableVersion]::new($currentVersion)

    if ($calcVersionAction -eq [VersionAction]::ReleaseHotfixCommit) {
        if ($currentVersion.Minor -ne 0) { throw "$currentVersion has a minor value but you are trying to release a hotfix, switch branches first" }
        $newVersion.Patch = $newVersion.Patch + 1    
    } elseif ($calcVersionAction -eq [VersionAction]::ReleaseStableCommit) {
        if ($currentVersion.Patch -ne 0) { throw "$currentVersion has a patch value but you are trying to release a stable commit, switch branches first" }
        if ($currentVersion.Minor -eq 999) { throw "$currentVersion is already stabilised but you are trying to release a stable commit, switch branches first" }
        $newVersion.Major = $newVersion.Major + 1
        $newVersion.Minor = 0
    } elseif ($calcVersionAction -eq [VersionAction]::ReleaseTestCommit) {
        if ($currentVersion.Patch -ne 0) { throw "$currentVersion has a patch value but you are trying to release a stable commit, switch branches first" }
        if ($currentVersion.Minor -eq 999) { throw "$currentVersion is already stabilised but you are trying to release a test commit, switch branches first" }
        $newVersion.Minor = $newVersion.Minor + 1
    } else {
        throw "Could not calculate new version. Cannot determine use case."
    }
    
    WriteHost "Version $currentVersion will be changed to $newVersion"

    return $newVersion
}


function ModifyProjectVersions {

	Param ([array]$projects, [EditableVersion]$newVersion)
        
    foreach ($project in $projects) {

	    $xml=New-Object XML

        $scriptFolder = GetScriptFolder

        [string] $projectFile = "$scriptFolder\$($project)\$($project).csproj"

	    $xml.Load($projectFile)
  
        $xml.Project.PropertyGroup.Version = "$newVersion"

        $xml.Save($projectFile)

    }
    
}

function Commit {
    
    Param([EditableVersion]$newVersion)

    WriteHostStep "Committing Project Files and Tagging Commit..."

    git commit -am "Updated Project Versions to $newVersion"
}

function Push { 

    Param([string] $repoPath)

    if (AskYesNo("Push Changes?") -eq $true) {
        git push --porcelain 
        #see https://stackoverflow.com/questions/12751261/powershell-displays-some-git-command-results-as-error-in-console-even-though-ope for --porcelain switch
    }
}



#START

#entry method
function Main {

    $projects = @(
        "DataStore",		      
        "DataStore.Impl.DocumentDb",
		"DataStore.Impl.SqlServer",
		"DataStore.Interfaces",
		"DataStore.Interfaces.LowLevel",
		"DataStore.Models"
    )
    
    SetWorkingDirectory

    VerifyIndexAndTreeIsClean

    $currentVersion = VerifyVersionsInSync $projects

    $newVersion = CalcNewVersion $currentVersion

    ModifyProjectVersions $projects $newVersion

    Commit $newVersion

    Push
        
    Write-Host "Done."        
}

#go
Main
