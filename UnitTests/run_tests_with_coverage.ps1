# Run Unit Tests with Coverage Report
Write-Host "Running Unit Tests with Coverage..." -ForegroundColor Cyan

# Navigate to test project directory
$testProjectPath = "c:\AI_Games\LastSpikeServer\GameplaySessionTracker\UnitTests"
$outputPath = "c:\AI_Games\LastSpikeServer\GameplaySessionTracker\TestResults"

# Clean previous test results
if (Test-Path $outputPath) {
    Remove-Item $outputPath -Recurse -Force
    Write-Host "Cleaned previous test results" -ForegroundColor Yellow
}

# Run tests with code coverage
Write-Host "`nExecuting tests..." -ForegroundColor Cyan
dotnet test $testProjectPath\GameplaySessionTracker.Tests.csproj `
    --collect:"XPlat Code Coverage" `
    --results-directory:$outputPath `
    --verbosity:normal

# Check if tests passed
if ($LASTEXITCODE -ne 0) {
    Write-Host "`nTests FAILED!" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "`nTests PASSED!" -ForegroundColor Green

# Find coverage file
$coverageFile = Get-ChildItem -Path $outputPath -Filter "coverage.cobertura.xml" -Recurse | Select-Object -First 1

if ($coverageFile) {
    Write-Host "`nParsing coverage report..." -ForegroundColor Cyan
    
    # Parse coverage XML
    [xml]$coverage = Get-Content $coverageFile.FullName
    
    $lineRate = [double]$coverage.coverage.'line-rate'
    $branchRate = [double]$coverage.coverage.'branch-rate'
    $linesCovered = [int]$coverage.coverage.'lines-covered'
    $linesValid = [int]$coverage.coverage.'lines-valid'
    $branchesCovered = [int]$coverage.coverage.'branches-covered'
    $branchesValid = [int]$coverage.coverage.'branches-valid'
    
    # Calculate percentages
    $lineCoverage = $lineRate * 100
    $branchCoverage = $branchRate * 100
    
    # Display summary
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "         COVERAGE SUMMARY" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Line Coverage:   $($lineCoverage.ToString('F2'))% ($linesCovered/$linesValid lines)" -ForegroundColor $(if ($lineCoverage -ge 80) { "Green" } elseif ($lineCoverage -ge 60) { "Yellow" } else { "Red" })
    Write-Host "Branch Coverage: $($branchCoverage.ToString('F2'))% ($branchesCovered/$branchesValid branches)" -ForegroundColor $(if ($branchCoverage -ge 80) { "Green" } elseif ($branchCoverage -ge 60) { "Yellow" } else { "Red" })
    Write-Host "========================================`n" -ForegroundColor Cyan
    
    # Display per-package coverage
    Write-Host "Coverage by Package:" -ForegroundColor Cyan
    foreach ($package in $coverage.coverage.packages.package) {
        $pkgLineRate = [double]$package.'line-rate' * 100
        $pkgName = $package.name
        Write-Host "  $pkgName : $($pkgLineRate.ToString('F2'))%" -ForegroundColor $(if ($pkgLineRate -ge 80) { "Green" } elseif ($pkgLineRate -ge 60) { "Yellow" } else { "Red" })
    }
    
    Write-Host "`nCoverage report location: $($coverageFile.FullName)" -ForegroundColor Gray
    
    # Determine overall result
    if ($lineCoverage -ge 100 -and $branchCoverage -ge 100) {
        Write-Host "`n100% COVERAGE ACHIEVED!" -ForegroundColor Green
    }
    elseif ($lineCoverage -ge 90) {
        Write-Host "`nExcellent coverage!" -ForegroundColor Green
    }
    elseif ($lineCoverage -ge 80) {
        Write-Host "`nGood coverage" -ForegroundColor Yellow
    }
    else {
        Write-Host "`nCoverage could be improved" -ForegroundColor Red
    }
}
else {
    Write-Host "`nWarning: Coverage file not found" -ForegroundColor Yellow
}
