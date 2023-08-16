Invoke-RestMethod -Uri "https://anavarro9731.visualstudio.com/defaultcollection/soap/_apis/git/repositories/soap/items?api-version=1.0&scopepath=src/pwsh-bootstrap/load-modules.psm1" -ContentType "text/plain; charset=UTF-8" -OutFile ".\load-modules.psm1"
Import-Module ".\load-modules.psm1" -Verbose -Global -Force
Remove-Item ".\load-modules.psm1" -Verbose -Recurse
Load-Modules

#expose this function like a CmdLet
function global:Run {

	Param(
		[switch]$PrepareNewVersion,
		[switch]$BuildAndTest,
		[switch]$PackAndPublish,
        [switch]$CreateRelease,
        [Alias('nuget-key')]
        [string] $nugetApiKey,
        [Alias('ado-pat')]
        [string] $azureDevopsPat,
        [string] $forceVersion
	)
	
	if ($PrepareNewVersion) {
        Prepare-NewVersion -projects @(
            "DataStore",		      
            "DataStore.Providers.CosmosDb",
            "DataStore.Interfaces",
            "DataStore.Interfaces.LowLevel",
            "DataStore.Models",
			"DataStore.Tests"
        ) `
        -githubUserName "anavarro9731" `
        -repository "datastore" `
        -forceVersion $forceVersion
    	}

	if ($BuildAndTest) {
		Build-And-Test -testProjects @(
            "DataStore.Tests" 
		)
	}

    if ($PackAndPublish) {
        Pack-And-Publish -allProjects @(
            "DataStore",		      
            "DataStore.Providers.CosmosDb",
            "DataStore.Interfaces",
            "DataStore.Interfaces.LowLevel",
            "DataStore.Models"       	         	    			
        ) -unlistedProjects @(
            "DataStore.Models" 
        ) `
        -nugetApiKey $nugetApiKey `
        -nugetFeedUri "https://api.nuget.org/v3/index.json"
    }

    if ($CreateRelease) {
        Create-Release -projects @(
        "DataStore",
        "DataStore.Providers.CosmosDb",
        "DataStore.Interfaces",
        "DataStore.Interfaces.LowLevel",
        "DataStore.Models",
        "DataStore.Tests"
        ) `
        -githubUserName "anavarro9731" `
        -repository "soap"
    }
}
