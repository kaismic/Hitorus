# Script to generate .resx files from a template and a content file.

#region User-defined variables (SET THESE MANUALLY BEFORE RUNNING THE SCRIPT BLOCK BELOW)

# Uncomment to also update Hitorus.Api
# $inputDirs = @('src\Hitorus.Api', 'src\Hitorus.Web')
$inputDirs = @('src\Hitorus.Web')

$outputDir = 'Localization'
$templateFile = 'resx-template.resx'
$contentFile = 'localization-data.txt'

#endregion

#region Script Implementation

foreach ($inputDir in $inputDirs) {
    Write-Host "Creating localization resource files in $inputDir."

    # --- Resolve initial paths to be absolute ---
    # This section modifies the user-supplied variables $templateFile, $contentFile, $outputDir
    # to hold their absolute path equivalents.
    $resolvedTemplateFile = [IO.Path]::Combine($PSScriptRoot, $templateFile)
    $resolvedContentFile = [IO.Path]::Combine($PSScriptRoot, $inputDir, $contentFile)
    $resolvedOutputDir = [IO.Path]::Combine($PSScriptRoot, $inputDir, $outputDir) # Normalize, does not require existence

    # Validate paths (now absolute) and create output directory if needed
    if (-not (Test-Path $resolvedTemplateFile -PathType Leaf)) {
        Write-Error "Template file not found or is not a file after resolution: $resolvedTemplateFile"
        exit 1
    }
    if (-not (Test-Path $resolvedContentFile -PathType Leaf)) {
        Write-Error "Content file not found or is not a file after resolution: $resolvedContentFile"
        exit 1
    }
    
    if (-not (Test-Path $resolvedOutputDir)) {
        Write-Host "Base output directory '$resolvedOutputDir' not found. Creating it."
        try {
            New-Item -ItemType Directory -Path $resolvedOutputDir -Force -ErrorAction Stop | Out-Null
            Write-Host "Base output directory '$resolvedOutputDir' created successfully."
        } catch {
            Write-Error "Failed to create base output directory '$resolvedOutputDir'. Error: $($_.Exception.Message)"
            exit 1
        }
    } elseif (-not (Get-Item $resolvedOutputDir).PSIsContainer) {
        Write-Error "Base output path '$resolvedOutputDir' exists but is not a directory."
        exit 1
    }
    
    Write-Host "Starting .resx generation process..."
    Write-Host "Template File (absolute): $resolvedTemplateFile"
    Write-Host "Content File (absolute): $resolvedContentFile"
    Write-Host "Output Directory (absolute): $resolvedOutputDir"
    
    # Read template content
    try {
        $templateContent = Get-Content $resolvedTemplateFile -Raw -ErrorAction Stop
    } catch {
        Write-Error "Failed to read template file '$resolvedTemplateFile'. Error: $($_.Exception.Message)"
        exit 1
    }
    
    # Read content file
    try {
        $fileContentRaw = Get-Content $resolvedContentFile -Raw -ErrorAction Stop
    } catch {
        Write-Error "Failed to read content file '$resolvedContentFile'. Error: $($_.Exception.Message)"
        exit 1
    }
    
    # --- Parse Content File ---
    $separatorChar = '|'
    $languages = @() 
    
    $partsByDelimiter = $fileContentRaw -split '(\r?\n---\r?\n)'
    
    if ($partsByDelimiter.Count -eq 0) {
        Write-Error "Content file '$resolvedContentFile' appears to be empty or does not follow expected format."
        exit 1
    }
    
    $headerPart = $partsByDelimiter[0].Trim()
    $actualSectionStrings = [System.Collections.Generic.List[string]]::new()
    if ($partsByDelimiter.Count -gt 1) {
        for ($i = 2; $i -lt $partsByDelimiter.Count; $i += 2) {
            $actualSectionStrings.Add($partsByDelimiter[$i].Trim())
        }
    }
    
    if ($headerPart -match '(?m)^Separator=(.)$') { 
        $separatorChar = $Matches[1]
    } 
    $escSeparator = [regex]::Escape($separatorChar)
    Write-Host "Using separator character: '$separatorChar'"
    
    if ($headerPart -match '(?m)^Languages=([^\r\n]+)$') {
        $languages = [string[]]@($Matches[1].Split(',') | ForEach-Object { $_.Trim() })
    }
    
    if ($languages.Count -gt 0) {
        Write-Host "Languages specified: $($languages -join ', ')"
    } else {
        Write-Host "No additional languages specified in the content file."
    }
    
    # --- Process Each Section ---
    if ($actualSectionStrings.Count -eq 0) {
        Write-Warning "No sections found after the header in '$resolvedContentFile'. Ensure sections are separated by '---' on its own line, following any header definitions."
    }
    
    foreach ($sectionBlock in $actualSectionStrings) {
        if ([string]::IsNullOrWhiteSpace($sectionBlock)) {
            Write-Verbose "Skipping an entirely empty section block."
            continue
        }
    
        $sectionLines = $sectionBlock.Split([Environment]::NewLine) | ForEach-Object { $_.Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    
        if ($sectionLines.Count -eq 0) {
            Write-Warning "Encountered a section block that became empty after trimming. Skipping."
            continue
        }
    
        $sectionFullName = $sectionLines[0] # e.g., "Pages\SearchPage" or "Components\GalleryBrowseItem"
        Write-Host "Processing section: '$sectionFullName'"
        $resourceStrings = $sectionLines[1..($sectionLines.Count - 1)]
    
        # Determine subdirectory and base file name from sectionFullName
        $baseFileName = Split-Path -Path $sectionFullName -Leaf         # "SearchPage"
        $relativeSubPath = Split-Path -Path $sectionFullName -Parent   # "Pages" or ""
    
        $finalSectionOutputDir = $resolvedOutputDir
        if (-not [string]::IsNullOrWhiteSpace($relativeSubPath)) {
            $finalSectionOutputDir = Join-Path $resolvedOutputDir $relativeSubPath
        }
    
        # Ensure the target subdirectory for the current section exists
        if (-not (Test-Path $finalSectionOutputDir -PathType Container)) {
            if (Test-Path $finalSectionOutputDir -PathType Leaf) {
                Write-Error "  A file exists at the target directory path '$finalSectionOutputDir'. Cannot create directory for section '$sectionFullName'."
                continue 
            }
            Write-Host "  Target directory for section '$finalSectionOutputDir' not found. Creating it."
            try {
                New-Item -ItemType Directory -Path $finalSectionOutputDir -Force -ErrorAction Stop | Out-Null
                Write-Host "  Successfully created directory: '$finalSectionOutputDir'"
            } catch {
                Write-Error "  Failed to create target directory '$finalSectionOutputDir' for section '$sectionFullName'. Error: $($_.Exception.Message)"
                continue 
            }
        }
        
        # --- Generate default .resx file (e.g., Pages\SearchPage.resx) ---
        $defaultResxEntries = [System.Collections.Generic.List[string]]::new()
        foreach ($stringEntry in $resourceStrings) {
            $parts = $stringEntry.Split($escSeparator)
            if ($parts.Count -ge 2) { 
                $key = $parts[0].Trim()
                $defaultValue = $parts[1].Trim()
                $entry = "  <data name=""$key"" xml:space=""preserve"">`n    <value>$defaultValue</value>`n  </data>"
                $defaultResxEntries.Add($entry)
            } else {
                Write-Warning "  Skipping malformed resource string in section '$sectionFullName' (not enough parts): '$stringEntry'"
            }
        }
    
        $allDefaultDataElements = $defaultResxEntries -join "`n"
        $dataToInsertDefault = "" 
        if (-not [string]::IsNullOrEmpty($allDefaultDataElements)) {
            $dataToInsertDefault = "`n" + $allDefaultDataElements 
        }
        
        $outputContentDefault = $templateContent -replace "(\s*</root>)", ($dataToInsertDefault + "`n</root>")
        $outputFilePathDefault = Join-Path $finalSectionOutputDir "$baseFileName.resx"
        try {
            Set-Content -Path $outputFilePathDefault -Value $outputContentDefault -Encoding UTF8 -ErrorAction Stop # Set-Content overwrites by default
            Write-Host "  Generated: $outputFilePathDefault"
        } catch {
            Write-Error "  Failed to write file '$outputFilePathDefault'. Error: $($_.Exception.Message)"
        }
    
        # --- Generate language-specific .resx files (e.g., Pages\SearchPage.ko.resx) ---
        for ($langIndex = 0; $langIndex -lt $languages.Count; $langIndex++) {
            $currentLang = $languages[0]
            Write-Host "Current lang: $currentLang"
            $langResxEntries = [System.Collections.Generic.List[string]]::new()
    
            foreach ($stringEntry in $resourceStrings) {
                $parts = $stringEntry.Split($escSeparator)
                $expectedPartIndexForLang = $langIndex + 2
                if ($parts.Count -gt $expectedPartIndexForLang) { 
                    $key = $parts[0].Trim()
                    $langValue = $parts[$expectedPartIndexForLang].Trim()
                    
                    if (-not [string]::IsNullOrEmpty($langValue)) {
                         $entry = "  <data name=""$key"" xml:space=""preserve"">`n    <value>$langValue</value>`n  </data>"
                         $langResxEntries.Add($entry)
                    } else {
                        Write-Verbose "  Skipping empty translation for key '$key' in language '$currentLang' for section '$sectionFullName'."
                    }
                } else {
                     Write-Verbose "  No translation string provided for key '$($parts[0].Trim())' in language '$currentLang' for section '$sectionFullName' (line: '$stringEntry')."
                }
            }
    
            if ($langResxEntries.Count -gt 0) {
                $allLangDataElements = $langResxEntries -join "`n"
                $dataToInsertLang = "`n" + $allLangDataElements 
                
                $outputContentLang = $templateContent -replace "(\s*</root>)", ($dataToInsertLang + "`n</root>")
                $outputFilePathLang = Join-Path $finalSectionOutputDir "$baseFileName.$currentLang.resx"
                try {
                    Set-Content -Path $outputFilePathLang -Value $outputContentLang -Encoding UTF8 -ErrorAction Stop # Set-Content overwrites by default
                    Write-Host "  Generated: $outputFilePathLang"
                } catch {
                    Write-Error "  Failed to write file '$outputFilePathLang'. Error: $($_.Exception.Message)"
                }
            } else {
                Write-Host "  No entries to generate for language '$currentLang' in section '$sectionFullName' (all translations were missing or empty)."
            }
        }
    }
}


Write-Host "Script finished."

#endregion