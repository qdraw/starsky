#!/bin/bash

#!/usr/bin/env bash

set -euo pipefail

SONAR_URL="https://sonarcloud.io/api/measures/component?component=starsky&metricKeys=vulnerabilities,bugs,code_smells,security_hotspots"

ISSUE_TITLE="SonarCloud Issues Detected"
ISSUE_LABEL="sonarcloud"

# Required env vars:
# GITHUB_TOKEN
# GITHUB_REPO (format: owner/repo)

echo "Fetching SonarCloud metrics..."
response=$(curl -s "$SONAR_URL")

# Extract values
bugs=$(echo "$response" | jq -r '.component.measures[] | select(.metric=="bugs") | .value')
vulns=$(echo "$response" | jq -r '.component.measures[] | select(.metric=="vulnerabilities") | .value')
smells=$(echo "$response" | jq -r '.component.measures[] | select(.metric=="code_smells") | .value')
hotspots=$(echo "$response" | jq -r '.component.measures[] | select(.metric=="security_hotspots") | .value')

bugs=${bugs:-0}
vulns=${vulns:-0}
smells=${smells:-0}
hotspots=${hotspots:-0}

echo "Bugs: $bugs, Vulnerabilities: $vulns, Code Smells: $smells, Hotspots: $hotspots"

has_issues=false
if [[ "$bugs" != "0" || "$vulns" != "0" || "$smells" != "0" || "$hotspots" != "0" ]]; then
  has_issues=true
fi

ISSUE_BODY=$(cat <<EOF
## SonarCloud Issues

- 🐞 Bugs: $bugs
- 🔐 Vulnerabilities: $vulns
- 🧹 Code Smells: $smells
- 🔥 Security Hotspots: $hotspots

[View in SonarCloud](https://sonarcloud.io/project/overview?id=starsky)
EOF
)

echo "Checking for existing GitHub issue with label '$ISSUE_LABEL'..."

existing_issue=$(curl -s -H "Authorization: token $GITHUB_TOKEN" \
  "https://api.github.com/repos/$GITHUB_REPO/issues?state=open&labels=$ISSUE_LABEL" \
  | jq -r '.[0].number')

if [[ "$has_issues" == true ]]; then
  if [[ -z "$existing_issue" || "$existing_issue" == "null" ]]; then
    echo "Creating new GitHub issue..."
    curl -s -X POST \
      -H "Authorization: token $GITHUB_TOKEN" \
      -H "Accept: application/vnd.github+json" \
      "https://api.github.com/repos/$GITHUB_REPO/issues" \
      -d "$(jq -n \
        --arg title "$ISSUE_TITLE" \
        --arg body "$ISSUE_BODY" \
        --arg label "$ISSUE_LABEL" \
        '{title: $title, body: $body, labels: [$label]}')"
  else
    echo "Updating existing issue #$existing_issue..."
    curl -s -X PATCH \
      -H "Authorization: token $GITHUB_TOKEN" \
      -H "Accept: application/vnd.github+json" \
      "https://api.github.com/repos/$GITHUB_REPO/issues/$existing_issue" \
      -d "$(jq -n --arg body "$ISSUE_BODY" '{body: $body}')"
  fi
else
  if [[ -n "$existing_issue" && "$existing_issue" != "null" ]]; then
    echo "Closing existing issue #$existing_issue (no issues left)..."
    curl -s -X PATCH \
      -H "Authorization: token $GITHUB_TOKEN" \
      -H "Accept: application/vnd.github+json" \
      "https://api.github.com/repos/$GITHUB_REPO/issues/$existing_issue" \
      -d '{"state":"closed"}'
  else
    echo "No issues and no existing GitHub issue."
  fi
fi