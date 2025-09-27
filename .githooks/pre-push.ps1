param($remoteName, $remoteUrl)
$stdin = [Console]::In
while ($line = $stdin.ReadLine()) {
  $parts = $line -split '\s+'
  $localRef = $parts[0]
  $remoteRef = $parts[2]
  if ($localRef -eq 'refs/heads/main' -or $remoteRef -eq 'refs/heads/main') {
    Write-Host "Direct pushes to 'main' are disabled. Use a feature branch and open a PR."
    exit 1
  }
}
exit 0
