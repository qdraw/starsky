Example calls

> Warning: do not commit package-lock.json output of this script, this is missing typescript

```
cd starsky-tools/build-tools
pwsh audit-npm.ps1 -failOnVulnLevel 'moderate' -action outdated -depth 0 -targetFolder ../../starsky-tools/docs/
pwsh audit-npm.ps1 -failOnVulnLevel 'moderate' -action outdated -depth 0 -targetFolder ../../starsky-tools/dropbox-import/
pwsh audit-npm.ps1 -failOnVulnLevel 'moderate' -action outdated -depth 0 -targetFolder ../../starsky-tools/end2end/
pwsh audit-npm.ps1 -failOnVulnLevel 'moderate' -action outdated -depth 0 -targetFolder ../../starsky-tools/mail
pwsh audit-npm.ps1 -failOnVulnLevel 'moderate' -action outdated -depth 0 -targetFolder ../../starsky-tools/mock
pwsh audit-npm.ps1 -failOnVulnLevel 'moderate' -action outdated -depth 0 -targetFolder ../../starsky-tools/thumbnail/
pwsh audit-npm.ps1  -failOnVulnLevel 'moderate' -action outdated -depth 0 -targetFolder ../../starsky/starsky/clientapp/
```

> Don't commit the output of package-lock.json
