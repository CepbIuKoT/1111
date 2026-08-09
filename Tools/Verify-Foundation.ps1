$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $PSScriptRoot

$required = @(
    "Assets/_NorthernLands/NorthernLands.Runtime.asmdef",
    "Assets/_NorthernLands/Core/Bootstrap/GameBootstrap.cs",
    "Assets/_NorthernLands/SaveSystem/JsonSaveGameService.cs",
    "Assets/_NorthernLands/SaveSystem/PermanentProfileStore.cs",
    "Assets/_NorthernLands/Player/Input/PlayerInputRouter.cs",
    "Assets/_NorthernLands/Player/Movement/SimpleThirdPersonMotor.cs",
    "Assets/_NorthernLands/Combat/HealthComponent.cs",
    "Assets/_NorthernLands/Combat/PlayerCombatController.cs",
    "Assets/_NorthernLands/AI/TrainingEnemyController.cs",
    "Assets/_NorthernLands/UI/MobileControls/VirtualJoystick.cs",
    "Assets/_NorthernLands/Editor/CombatSandboxSetup.cs",
    "Assets/_NorthernLands/Editor/NorthernLandsProjectSetup.cs",
    "Packages/manifest.json",
    "ProjectSettings/ProjectVersion.txt",
    "README_RU.md"
)

$missing = @()
foreach ($relativePath in $required) {
    $fullPath = Join-Path $projectRoot $relativePath
    if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
        $missing += $relativePath
    }
}

if ($missing.Count -gt 0) {
    Write-Host "Проверка не пройдена. Не найдены файлы:" -ForegroundColor Red
    $missing | ForEach-Object { Write-Host " - $_" }
    exit 1
}

Write-Host "Основа проекта Северные Земли собрана правильно." -ForegroundColor Green
Write-Host "Следующая проверка выполняется внутри Unity: Tools > Northern Lands > Validate Foundation"
