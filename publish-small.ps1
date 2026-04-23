param(
    [string]$Configuration = "Release",
    [string]$PublishProfile = "Properties/PublishProfiles/SmallFrameworkDependent.pubxml",
    [string]$Project = "translation.csproj",
    [string]$OutputZip = "release-small.zip"
)

$publishDir = Join-Path -Path (Resolve-Path ".") -ChildPath "publish_temp"
if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }

Write-Output "Publishing $Project using profile $PublishProfile (config: $Configuration) ..."
dotnet publish $Project -c $Configuration -p:PublishProfile=$PublishProfile -o $publishDir

if (-not (Test-Path $publishDir)) { Write-Error "Publish failed or output not found." ; exit 1 }

# Show sizes of files in publish folder
Write-Output "Published files:" 
Get-ChildItem $publishDir -Recurse | Sort-Object Length -Descending | Select-Object FullName, @{Name='SizeKB';Expression={[math]::Round($_.Length/1KB,2)}} | Format-Table -AutoSize -Wrap

# Create zip of publish folder
if (Test-Path $OutputZip) { Remove-Item $OutputZip -Force }
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($publishDir, $OutputZip)

Write-Output "Created $OutputZip (size: $((Get-Item $OutputZip).Length/1MB) MB)"
Write-Output "NOTE: This publish is framework-dependent and requires the target machine to have the matching .NET runtime installed."
Write-Output "If you need a portable single-file that includes the runtime, set SelfContained=true in a different publish profile (this will increase size)."

# Cleanup
# Remove-Item $publishDir -Recurse -Force

Write-Output "Done."