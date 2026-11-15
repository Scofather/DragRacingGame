# Ollama Unity C# Dev Assistant
# Usage:
#   .\ollama-assist.ps1 -File Assets\Scripts\GameManager.cs -Action review
#   .\ollama-assist.ps1 -File Assets\Scripts\CarPhysics.cs -Action explain
#   .\ollama-assist.ps1 -File Assets\Scripts\CarPhysics.cs -Action improve
#   .\ollama-assist.ps1 -File Assets\Scripts\CarPhysics.cs -Action bugs
#   .\ollama-assist.ps1 -File Assets\Scripts\GameManager.cs -Ask "How do I add nitro boost?"
#   .\ollama-assist.ps1 -Ask "How do I implement camera shake in Unity 6?"

param(
    [string]$File,
    [ValidateSet("review", "explain", "improve", "bugs")]
    [string]$Action,
    [string]$Ask,
    [string]$Model = "qwen2.5-coder:7b"
)

$ctx = "You are an expert Unity 6 URP C# developer. The project is a drag racing game with car physics (6-gear auto-shift, RPM, tire smoke), weather system, race timer, countdown, and post-race stats. Be concise and practical. When suggesting code changes, show only the relevant snippet."

if (-not $File -and -not $Ask) {
    Write-Host ""
    Write-Host "Ollama Unity Dev Assistant  (model: $Model)" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  Review a script for bugs and issues:" -ForegroundColor Yellow
    Write-Host "    .\ollama-assist.ps1 -File Assets\Scripts\GameManager.cs -Action review"
    Write-Host ""
    Write-Host "  Explain what a script does:" -ForegroundColor Yellow
    Write-Host "    .\ollama-assist.ps1 -File Assets\Scripts\CarPhysics.cs -Action explain"
    Write-Host ""
    Write-Host "  Suggest improvements:" -ForegroundColor Yellow
    Write-Host "    .\ollama-assist.ps1 -File Assets\Scripts\CarPhysics.cs -Action improve"
    Write-Host ""
    Write-Host "  Find bugs only:" -ForegroundColor Yellow
    Write-Host "    .\ollama-assist.ps1 -File Assets\Scripts\RaceTimer.cs -Action bugs"
    Write-Host ""
    Write-Host "  Ask about a specific script:" -ForegroundColor Yellow
    Write-Host "    .\ollama-assist.ps1 -File Assets\Scripts\GameManager.cs -Ask `"How do I add nitro boost?`""
    Write-Host ""
    Write-Host "  Ask a general Unity question:" -ForegroundColor Yellow
    Write-Host "    .\ollama-assist.ps1 -Ask `"Best way to do camera shake in Unity 6 URP?`""
    Write-Host ""
    Write-Host "  Use a different model:" -ForegroundColor Yellow
    Write-Host "    .\ollama-assist.ps1 -Ask `"...`" -Model qwen3:4b"
    Write-Host ""
    Write-Host "  Available models:" -ForegroundColor DarkGray
    ollama list
    exit 0
}

if ($File) {
    if (-not (Test-Path $File)) {
        Write-Error "File not found: $File"
        exit 1
    }
    $code = Get-Content $File -Raw
    $name = Split-Path $File -Leaf

    if ($Ask) {
        $prompt = "$ctx`n`nScript ($name):`n``````csharp`n$code`n``````n`nQuestion: $Ask"
    } elseif ($Action -eq "review") {
        $prompt = "$ctx`n`nReview this Unity C# script. List bugs, bad patterns, and missed Unity best practices. Reference line numbers where relevant.`n`nScript ($name):`n``````csharp`n$code`n``````"
    } elseif ($Action -eq "explain") {
        $prompt = "$ctx`n`nExplain what this script does, its key mechanics, and how it integrates with the drag racing game.`n`nScript ($name):`n``````csharp`n$code`n``````"
    } elseif ($Action -eq "improve") {
        $prompt = "$ctx`n`nSuggest specific improvements: performance, readability, Unity 6 best practices. Show code snippets for each suggestion.`n`nScript ($name):`n``````csharp`n$code`n``````"
    } elseif ($Action -eq "bugs") {
        $prompt = "$ctx`n`nFind only actual bugs and logic errors (not style issues) in this script. For each bug state: what's wrong, why it's a bug, and the fix.`n`nScript ($name):`n``````csharp`n$code`n``````"
    } else {
        $prompt = "$ctx`n`nScript ($name):`n``````csharp`n$code`n``````"
    }
} else {
    $prompt = "$ctx`n`n$Ask"
}

Write-Host ""
Write-Host "[$Model]" -ForegroundColor DarkCyan
Write-Host ""
$prompt | ollama run $Model
Write-Host ""
