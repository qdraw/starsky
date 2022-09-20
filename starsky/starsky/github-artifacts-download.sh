#!/bin/bash

# For insiders only - requires token
# Please use: 
# ./pm2-install-latest-release.sh 
# for public builds

# Script goal:
# Download binaries with zip folder from Github Actions
# Get pm2-new-instance.sh ready to run (but not run)

# source: /opt/starsky/starsky/github-artifacts-download.sh

WORKFLOW_ID="release-on-tag-netcore-desktop-electron.yml"

# default will be overwritten
RUNTIME="linux-arm"
case $(uname -m) in
  "aarch64")
    RUNTIME="linux-arm64"
    ;;

  "armv7l")
    RUNTIME="linux-arm"
    ;;

  "arm64")
    if [ $(uname) = "Darwin" ]; then
        RUNTIME="starsky-mac-desktop"
    fi
    ;;

  "x86_64")
    if [ $(uname) = "Darwin" ]; then
        # server: RUNTIME="osx-x64"
        RUNTIME="starsky-mac-desktop"
    fi
    # there is no linux desktop
    if [ $(uname) = "Linux" ]; then
        RUNTIME="linux-x64"
    fi
    ;;
esac

CURRENT_DIR=$(dirname "$0")
OUTPUT_DIR=$CURRENT_DIR

# get arguments
ARGUMENTS=("$@")

echo ${ARGUMENTS}

for ((i = 1; i <= $#; i++ )); do
  CURRENT=$(($i-1))
  if [[ ${ARGUMENTS[CURRENT]} == "--help" ]];
  then
    echo "--runtime linux-arm OR --runtime osx-x64 OR --runtime win-x64"
    echo "     (or as fallback:) --runtime "$RUNTIME
    echo "--branch master"
    echo "--token anything"
    echo "--output output_dir default folder_of_this_file"
    exit 0
  fi
  
  if [ $i -gt 1 ]; then
    PREV=$(($i-2))
    
    if [[ ${ARGUMENTS[PREV]} == "--runtime" ]];
    then
        RUNTIME="${ARGUMENTS[CURRENT]}"
    fi

    if [[ ${ARGUMENTS[PREV]} == "--token" ]];
    then
        STARSKY_GITHUB_PAT="${ARGUMENTS[CURRENT]}"
    fi
    
    if [[ ${ARGUMENTS[PREV]} == "--output" ]];
    then
        OUTPUT_DIR="${ARGUMENTS[CURRENT]}"
    fi
  fi
done

# add slash if not exists
LAST_CHAR_OUTPUT_DIR=${OUTPUT_DIR:length-1:1}
[[ $LAST_CHAR_OUTPUT_DIR != "/" ]] && OUTPUT_DIR="$OUTPUT_DIR/"; :

# rename
VERSION=$RUNTIME

if [[ $VERSION != *desktop ]]
then
    VERSION_ZIP="starsky-"$RUNTIME".zip"
else
   VERSION_ZIP=$RUNTIME".zip"
fi 

if [ ! -d $OUTPUT_DIR ]; then
    echo "FAIL "$OUTPUT_DIR" does not exist "
    exit 1
fi

# output dir should have slash at end
if [ -f $OUTPUT_DIR"Startup.cs" ]; then
    echo "FAIL: You should not run this folder from the source folder"
    echo "copy this file to the location to run it from"
    echo "end script due failure"
    exit 1
fi

if [[ -z $STARSKY_GITHUB_PAT ]]; then
  echo "enter your PAT: and press enter"
  read STARSKY_GITHUB_PAT
fi

echo ""

ACTIONS_WORKFLOW_URL="https://api.github.com/repos/qdraw/starsky/actions/workflows/"$WORKFLOW_ID"/runs?status=completed&per_page=1&exclude_pull_requests=true"

echo "V: "$VERSION " zip: " $VERSION_ZIP
echo "OUT" $OUTPUT_DIR
echo ">: "$ACTIONS_WORKFLOW_URL
RESULT_ACTIONS_WORKFLOW=$(curl --user :$STARSKY_GITHUB_PAT -sS $ACTIONS_WORKFLOW_URL)

# example:
#RESULT_ACTIONS_WORKFLOW='{ "total_count": 3, "workflow_runs": [ { "id": 1637008355, "name": "Create Desktop Release on tag for .Net Core and Electron", "node_id": "WFR_kwLODj5Xv85hksPj", "head_branch": "master", "head_sha": "7c6a81022c8209911c725c9a09c25fcb027294ce", "run_number": 3, "event": "workflow_dispatch", "status": "completed", "conclusion": "success", "workflow_id": 17105174, "check_suite_id": 4792148100, "check_suite_node_id": "CS_kwDODj5Xv88AAAABHaJghA", "url": "https://api.github.com/repos/qdraw/starsky/actions/runs/1637008355", "html_url": "https://github.com/qdraw/starsky/actions/runs/1637008355", "pull_requests": [ ], "created_at": "2021-12-30T09:35:21Z", "updated_at": "2021-12-30T09:54:53Z", "run_attempt": 1, "run_started_at": "2021-12-30T09:35:21Z", "jobs_url": "https://api.github.com/repos/qdraw/starsky/actions/runs/1637008355/jobs", "logs_url": "https://api.github.com/repos/qdraw/starsky/actions/runs/1637008355/logs", "check_suite_url": "https://api.github.com/repos/qdraw/starsky/check-suites/4792148100", "artifacts_url": "https://api.github.com/repos/qdraw/starsky/actions/runs/1637008355/artifacts", "cancel_url": "https://api.github.com/repos/qdraw/starsky/actions/runs/1637008355/cancel", "rerun_url": "https://api.github.com/repos/qdraw/starsky/actions/runs/1637008355/rerun", "previous_attempt_url": null, "workflow_url": "https://api.github.com/repos/qdraw/starsky/actions/workflows/17105174", "head_commit": { "id": "7c6a81022c8209911c725c9a09c25fcb027294ce", "tree_id": "0da90264c2c9018b9b2c3a1648fc519175f4de9c", "message": "add comment", "timestamp": "2021-12-30T08:48:45Z", "author": { "name": "Dion", "email": "dionvanvelde@gmail.com" }, "committer": { "name": "Dion", "email": "dionvanvelde@gmail.com" } }, "repository": { "id": 238966719, "node_id": "MDEwOlJlcG9zaXRvcnkyMzg5NjY3MTk=", "name": "starsky", "full_name": "qdraw/starsky", "private": false, "owner": { "login": "qdraw", "id": 1826996, "node_id": "MDQ6VXNlcjE4MjY5OTY=", "avatar_url": "https://avatars.githubusercontent.com/u/1826996?v=4", "gravatar_id": "", "url": "https://api.github.com/users/qdraw", "html_url": "https://github.com/qdraw", "followers_url": "https://api.github.com/users/qdraw/followers", "following_url": "https://api.github.com/users/qdraw/following{/other_user}", "gists_url": "https://api.github.com/users/qdraw/gists{/gist_id}", "starred_url": "https://api.github.com/users/qdraw/starred{/owner}{/repo}", "subscriptions_url": "https://api.github.com/users/qdraw/subscriptions", "organizations_url": "https://api.github.com/users/qdraw/orgs", "repos_url": "https://api.github.com/users/qdraw/repos", "events_url": "https://api.github.com/users/qdraw/events{/privacy}", "received_events_url": "https://api.github.com/users/qdraw/received_events", "type": "User", "site_admin": false }, "html_url": "https://github.com/qdraw/starsky", "description": "Accelerator to find and organize images driven by meta information. Browse and search images in your own cloud.", "fork": false, "url": "https://api.github.com/repos/qdraw/starsky", "forks_url": "https://api.github.com/repos/qdraw/starsky/forks", "keys_url": "https://api.github.com/repos/qdraw/starsky/keys{/key_id}", "collaborators_url": "https://api.github.com/repos/qdraw/starsky/collaborators{/collaborator}", "teams_url": "https://api.github.com/repos/qdraw/starsky/teams", "hooks_url": "https://api.github.com/repos/qdraw/starsky/hooks", "issue_events_url": "https://api.github.com/repos/qdraw/starsky/issues/events{/number}", "events_url": "https://api.github.com/repos/qdraw/starsky/events", "assignees_url": "https://api.github.com/repos/qdraw/starsky/assignees{/user}", "branches_url": "https://api.github.com/repos/qdraw/starsky/branches{/branch}", "tags_url": "https://api.github.com/repos/qdraw/starsky/tags", "blobs_url": "https://api.github.com/repos/qdraw/starsky/git/blobs{/sha}", "git_tags_url": "https://api.github.com/repos/qdraw/starsky/git/tags{/sha}", "git_refs_url": "https://api.github.com/repos/qdraw/starsky/git/refs{/sha}", "trees_url": "https://api.github.com/repos/qdraw/starsky/git/trees{/sha}", "statuses_url": "https://api.github.com/repos/qdraw/starsky/statuses/{sha}", "languages_url": "https://api.github.com/repos/qdraw/starsky/languages", "stargazers_url": "https://api.github.com/repos/qdraw/starsky/stargazers", "contributors_url": "https://api.github.com/repos/qdraw/starsky/contributors", "subscribers_url": "https://api.github.com/repos/qdraw/starsky/subscribers", "subscription_url": "https://api.github.com/repos/qdraw/starsky/subscription", "commits_url": "https://api.github.com/repos/qdraw/starsky/commits{/sha}", "git_commits_url": "https://api.github.com/repos/qdraw/starsky/git/commits{/sha}", "comments_url": "https://api.github.com/repos/qdraw/starsky/comments{/number}", "issue_comment_url": "https://api.github.com/repos/qdraw/starsky/issues/comments{/number}", "contents_url": "https://api.github.com/repos/qdraw/starsky/contents/{+path}", "compare_url": "https://api.github.com/repos/qdraw/starsky/compare/{base}...{head}", "merges_url": "https://api.github.com/repos/qdraw/starsky/merges", "archive_url": "https://api.github.com/repos/qdraw/starsky/{archive_format}{/ref}", "downloads_url": "https://api.github.com/repos/qdraw/starsky/downloads", "issues_url": "https://api.github.com/repos/qdraw/starsky/issues{/number}", "pulls_url": "https://api.github.com/repos/qdraw/starsky/pulls{/number}", "milestones_url": "https://api.github.com/repos/qdraw/starsky/milestones{/number}", "notifications_url": "https://api.github.com/repos/qdraw/starsky/notifications{?since,all,participating}", "labels_url": "https://api.github.com/repos/qdraw/starsky/labels{/name}", "releases_url": "https://api.github.com/repos/qdraw/starsky/releases{/id}", "deployments_url": "https://api.github.com/repos/qdraw/starsky/deployments" }, "head_repository": { "id": 238966719, "node_id": "MDEwOlJlcG9zaXRvcnkyMzg5NjY3MTk=", "name": "starsky", "full_name": "qdraw/starsky", "private": false, "owner": { "login": "qdraw", "id": 1826996, "node_id": "MDQ6VXNlcjE4MjY5OTY=", "avatar_url": "https://avatars.githubusercontent.com/u/1826996?v=4", "gravatar_id": "", "url": "https://api.github.com/users/qdraw", "html_url": "https://github.com/qdraw", "followers_url": "https://api.github.com/users/qdraw/followers", "following_url": "https://api.github.com/users/qdraw/following{/other_user}", "gists_url": "https://api.github.com/users/qdraw/gists{/gist_id}", "starred_url": "https://api.github.com/users/qdraw/starred{/owner}{/repo}", "subscriptions_url": "https://api.github.com/users/qdraw/subscriptions", "organizations_url": "https://api.github.com/users/qdraw/orgs", "repos_url": "https://api.github.com/users/qdraw/repos", "events_url": "https://api.github.com/users/qdraw/events{/privacy}", "received_events_url": "https://api.github.com/users/qdraw/received_events", "type": "User", "site_admin": false }, "html_url": "https://github.com/qdraw/starsky", "description": "Accelerator to find and organize images driven by meta information. Browse and search images in your own cloud.", "fork": false, "url": "https://api.github.com/repos/qdraw/starsky", "forks_url": "https://api.github.com/repos/qdraw/starsky/forks", "keys_url": "https://api.github.com/repos/qdraw/starsky/keys{/key_id}", "collaborators_url": "https://api.github.com/repos/qdraw/starsky/collaborators{/collaborator}", "teams_url": "https://api.github.com/repos/qdraw/starsky/teams", "hooks_url": "https://api.github.com/repos/qdraw/starsky/hooks", "issue_events_url": "https://api.github.com/repos/qdraw/starsky/issues/events{/number}", "events_url": "https://api.github.com/repos/qdraw/starsky/events", "assignees_url": "https://api.github.com/repos/qdraw/starsky/assignees{/user}", "branches_url": "https://api.github.com/repos/qdraw/starsky/branches{/branch}", "tags_url": "https://api.github.com/repos/qdraw/starsky/tags", "blobs_url": "https://api.github.com/repos/qdraw/starsky/git/blobs{/sha}", "git_tags_url": "https://api.github.com/repos/qdraw/starsky/git/tags{/sha}", "git_refs_url": "https://api.github.com/repos/qdraw/starsky/git/refs{/sha}", "trees_url": "https://api.github.com/repos/qdraw/starsky/git/trees{/sha}", "statuses_url": "https://api.github.com/repos/qdraw/starsky/statuses/{sha}", "languages_url": "https://api.github.com/repos/qdraw/starsky/languages", "stargazers_url": "https://api.github.com/repos/qdraw/starsky/stargazers", "contributors_url": "https://api.github.com/repos/qdraw/starsky/contributors", "subscribers_url": "https://api.github.com/repos/qdraw/starsky/subscribers", "subscription_url": "https://api.github.com/repos/qdraw/starsky/subscription", "commits_url": "https://api.github.com/repos/qdraw/starsky/commits{/sha}", "git_commits_url": "https://api.github.com/repos/qdraw/starsky/git/commits{/sha}", "comments_url": "https://api.github.com/repos/qdraw/starsky/comments{/number}", "issue_comment_url": "https://api.github.com/repos/qdraw/starsky/issues/comments{/number}", "contents_url": "https://api.github.com/repos/qdraw/starsky/contents/{+path}", "compare_url": "https://api.github.com/repos/qdraw/starsky/compare/{base}...{head}", "merges_url": "https://api.github.com/repos/qdraw/starsky/merges", "archive_url": "https://api.github.com/repos/qdraw/starsky/{archive_format}{/ref}", "downloads_url": "https://api.github.com/repos/qdraw/starsky/downloads", "issues_url": "https://api.github.com/repos/qdraw/starsky/issues{/number}", "pulls_url": "https://api.github.com/repos/qdraw/starsky/pulls{/number}", "milestones_url": "https://api.github.com/repos/qdraw/starsky/milestones{/number}", "notifications_url": "https://api.github.com/repos/qdraw/starsky/notifications{?since,all,participating}", "labels_url": "https://api.github.com/repos/qdraw/starsky/labels{/name}", "releases_url": "https://api.github.com/repos/qdraw/starsky/releases{/id}", "deployments_url": "https://api.github.com/repos/qdraw/starsky/deployments" } }, { "id": 1636879980, "name": "Create Desktop Release on tag for .Net Core and Electron", "node_id": "WFR_kwLODj5Xv85hkM5s", "head_branch": "master", "head_sha": "7c6a81022c8209911c725c9a09c25fcb027294ce", "run_number": 2, "event": "workflow_dispatch", "status": "completed", "conclusion": "success", "workflow_id": 17105174, "check_suite_id": 4791859849, "check_suite_node_id": "CS_kwDODj5Xv88AAAABHZ36iQ", "url": "https://api.github.com/repos/qdraw/starsky/actions/runs/1636879980", "html_url": "https://github.com/qdraw/starsky/actions/runs/1636879980", "pull_requests": [ ], "created_at": "2021-12-30T08:49:03Z", "updated_at": "2021-12-30T09:08:08Z", "run_attempt": 1, "run_started_at": "2021-12-30T08:49:03Z", "jobs_url": "https://api.github.com/repos/qdraw/starsky/actions/runs/1636879980/jobs", "logs_url": "https://api.github.com/repos/qdraw/starsky/actions/runs/1636879980/logs", "check_suite_url": "https://api.github.com/repos/qdraw/starsky/check-suites/4791859849", "artifacts_url": "https://api.github.com/repos/qdraw/starsky/actions/runs/1636879980/artifacts", "cancel_url": "https://api.github.com/repos/qdraw/starsky/actions/runs/1636879980/cancel", "rerun_url": "https://api.github.com/repos/qdraw/starsky/actions/runs/1636879980/rerun", "previous_attempt_url": null, "workflow_url": "https://api.github.com/repos/qdraw/starsky/actions/workflows/17105174", "head_commit": { "id": "7c6a81022c8209911c725c9a09c25fcb027294ce", "tree_id": "0da90264c2c9018b9b2c3a1648fc519175f4de9c", "message": "add comment", "timestamp": "2021-12-30T08:48:45Z", "author": { "name": "Dion", "email": "dionvanvelde@gmail.com" }, "committer": { "name": "Dion", "email": "dionvanvelde@gmail.com" } }, "repository": { "id": 238966719, "node_id": "MDEwOlJlcG9zaXRvcnkyMzg5NjY3MTk=", "name": "starsky", "full_name": "qdraw/starsky", "private": false, "owner": { "login": "qdraw", "id": 1826996, "node_id": "MDQ6VXNlcjE4MjY5OTY=", "avatar_url": "https://avatars.githubusercontent.com/u/1826996?v=4", "gravatar_id": "", "url": "https://api.github.com/users/qdraw", "html_url": "https://github.com/qdraw", "followers_url": "https://api.github.com/users/qdraw/followers", "following_url": "https://api.github.com/users/qdraw/following{/other_user}", "gists_url": "https://api.github.com/users/qdraw/gists{/gist_id}", "starred_url": "https://api.github.com/users/qdraw/starred{/owner}{/repo}", "subscriptions_url": "https://api.github.com/users/qdraw/subscriptions", "organizations_url": "https://api.github.com/users/qdraw/orgs", "repos_url": "https://api.github.com/users/qdraw/repos", "events_url": "https://api.github.com/users/qdraw/events{/privacy}", "received_events_url": "https://api.github.com/users/qdraw/received_events", "type": "User", "site_admin": false }, "html_url": "https://github.com/qdraw/starsky", "description": "Accelerator to find and organize images driven by meta information. Browse and search images in your own cloud.", "fork": false, "url": "https://api.github.com/repos/qdraw/starsky", "forks_url": "https://api.github.com/repos/qdraw/starsky/forks", "keys_url": "https://api.github.com/repos/qdraw/starsky/keys{/key_id}", "collaborators_url": "https://api.github.com/repos/qdraw/starsky/collaborators{/collaborator}", "teams_url": "https://api.github.com/repos/qdraw/starsky/teams", "hooks_url": "https://api.github.com/repos/qdraw/starsky/hooks", "issue_events_url": "https://api.github.com/repos/qdraw/starsky/issues/events{/number}", "events_url": "https://api.github.com/repos/qdraw/starsky/events", "assignees_url": "https://api.github.com/repos/qdraw/starsky/assignees{/user}", "branches_url": "https://api.github.com/repos/qdraw/starsky/branches{/branch}", "tags_url": "https://api.github.com/repos/qdraw/starsky/tags", "blobs_url": "https://api.github.com/repos/qdraw/starsky/git/blobs{/sha}", "git_tags_url": "https://api.github.com/repos/qdraw/starsky/git/tags{/sha}", "git_refs_url": "https://api.github.com/repos/qdraw/starsky/git/refs{/sha}", "trees_url": "https://api.github.com/repos/qdraw/starsky/git/trees{/sha}", "statuses_url": "https://api.github.com/repos/qdraw/starsky/statuses/{sha}", "languages_url": "https://api.github.com/repos/qdraw/starsky/languages", "stargazers_url": "https://api.github.com/repos/qdraw/starsky/stargazers", "contributors_url": "https://api.github.com/repos/qdraw/starsky/contributors", "subscribers_url": "https://api.github.com/repos/qdraw/starsky/subscribers", "subscription_url": "https://api.github.com/repos/qdraw/starsky/subscription", "commits_url": "https://api.github.com/repos/qdraw/starsky/commits{/sha}", "git_commits_url": "https://api.github.com/repos/qdraw/starsky/git/commits{/sha}", "comments_url": "https://api.github.com/repos/qdraw/starsky/comments{/number}", "issue_comment_url": "https://api.github.com/repos/qdraw/starsky/issues/comments{/number}", "contents_url": "https://api.github.com/repos/qdraw/starsky/contents/{+path}", "compare_url": "https://api.github.com/repos/qdraw/starsky/compare/{base}...{head}", "merges_url": "https://api.github.com/repos/qdraw/starsky/merges", "archive_url": "https://api.github.com/repos/qdraw/starsky/{archive_format}{/ref}", "downloads_url": "https://api.github.com/repos/qdraw/starsky/downloads", "issues_url": "https://api.github.com/repos/qdraw/starsky/issues{/number}", "pulls_url": "https://api.github.com/repos/qdraw/starsky/pulls{/number}", "milestones_url": "https://api.github.com/repos/qdraw/starsky/milestones{/number}", "notifications_url": "https://api.github.com/repos/qdraw/starsky/notifications{?since,all,participating}", "labels_url": "https://api.github.com/repos/qdraw/starsky/labels{/name}", "releases_url": "https://api.github.com/repos/qdraw/starsky/releases{/id}", "deployments_url": "https://api.github.com/repos/qdraw/starsky/deployments" }, "head_repository": { "id": 238966719, "node_id": "MDEwOlJlcG9zaXRvcnkyMzg5NjY3MTk=", "name": "starsky", "full_name": "qdraw/starsky", "private": false, "owner": { "login": "qdraw", "id": 1826996, "node_id": "MDQ6VXNlcjE4MjY5OTY=", "avatar_url": "https://avatars.githubusercontent.com/u/1826996?v=4", "gravatar_id": "", "url": "https://api.github.com/users/qdraw", "html_url": "https://github.com/qdraw", "followers_url": "https://api.github.com/users/qdraw/followers", "following_url": "https://api.github.com/users/qdraw/following{/other_user}", "gists_url": "https://api.github.com/users/qdraw/gists{/gist_id}", "starred_url": "https://api.github.com/users/qdraw/starred{/owner}{/repo}", "subscriptions_url": "https://api.github.com/users/qdraw/subscriptions", "organizations_url": "https://api.github.com/users/qdraw/orgs", "repos_url": "https://api.github.com/users/qdraw/repos", "events_url": "https://api.github.com/users/qdraw/events{/privacy}", "received_events_url": "https://api.github.com/users/qdraw/received_events", "type": "User", "site_admin": false }, "html_url": "https://github.com/qdraw/starsky", "description": "Accelerator to find and organize images driven by meta information. Browse and search images in your own cloud.", "fork": false, "url": "https://api.github.com/repos/qdraw/starsky", "forks_url": "https://api.github.com/repos/qdraw/starsky/forks", "keys_url": "https://api.github.com/repos/qdraw/starsky/keys{/key_id}", "collaborators_url": "https://api.github.com/repos/qdraw/starsky/collaborators{/collaborator}", "teams_url": "https://api.github.com/repos/qdraw/starsky/teams", "hooks_url": "https://api.github.com/repos/qdraw/starsky/hooks", "issue_events_url": "https://api.github.com/repos/qdraw/starsky/issues/events{/number}", "events_url": "https://api.github.com/repos/qdraw/starsky/events", "assignees_url": "https://api.github.com/repos/qdraw/starsky/assignees{/user}", "branches_url": "https://api.github.com/repos/qdraw/starsky/branches{/branch}", "tags_url": "https://api.github.com/repos/qdraw/starsky/tags", "blobs_url": "https://api.github.com/repos/qdraw/starsky/git/blobs{/sha}", "git_tags_url": "https://api.github.com/repos/qdraw/starsky/git/tags{/sha}", "git_refs_url": "https://api.github.com/repos/qdraw/starsky/git/refs{/sha}", "trees_url": "https://api.github.com/repos/qdraw/starsky/git/trees{/sha}", "statuses_url": "https://api.github.com/repos/qdraw/starsky/statuses/{sha}", "languages_url": "https://api.github.com/repos/qdraw/starsky/languages", "stargazers_url": "https://api.github.com/repos/qdraw/starsky/stargazers", "contributors_url": "https://api.github.com/repos/qdraw/starsky/contributors", "subscribers_url": "https://api.github.com/repos/qdraw/starsky/subscribers", "subscription_url": "https://api.github.com/repos/qdraw/starsky/subscription", "commits_url": "https://api.github.com/repos/qdraw/starsky/commits{/sha}", "git_commits_url": "https://api.github.com/repos/qdraw/starsky/git/commits{/sha}", "comments_url": "https://api.github.com/repos/qdraw/starsky/comments{/number}", "issue_comment_url": "https://api.github.com/repos/qdraw/starsky/issues/comments{/number}", "contents_url": "https://api.github.com/repos/qdraw/starsky/contents/{+path}", "compare_url": "https://api.github.com/repos/qdraw/starsky/compare/{base}...{head}", "merges_url": "https://api.github.com/repos/qdraw/starsky/merges", "archive_url": "https://api.github.com/repos/qdraw/starsky/{archive_format}{/ref}", "downloads_url": "https://api.github.com/repos/qdraw/starsky/downloads", "issues_url": "https://api.github.com/repos/qdraw/starsky/issues{/number}", "pulls_url": "https://api.github.com/repos/qdraw/starsky/pulls{/number}", "milestones_url": "https://api.github.com/repos/qdraw/starsky/milestones{/number}", "notifications_url": "https://api.github.com/repos/qdraw/starsky/notifications{?since,all,participating}", "labels_url": "https://api.github.com/repos/qdraw/starsky/labels{/name}", "releases_url": "https://api.github.com/repos/qdraw/starsky/releases{/id}", "deployments_url": "https://api.github.com/repos/qdraw/starsky/deployments" } }, { "id": 1636835300, "name": "Create Desktop Release on tag for .Net Core and Electron", "node_id": "WFR_kwLODj5Xv85hkB_k", "head_branch": "master", "head_sha": "2d237de506d19642e3010d26af9c5c2da0bd35bf", "run_number": 1, "event": "push", "status": "completed", "conclusion": "success", "workflow_id": 17105174, "check_suite_id": 4791757190, "check_suite_node_id": "CS_kwDODj5Xv88AAAABHZxphg", "url": "https://api.github.com/repos/qdraw/starsky/actions/runs/1636835300", "html_url": "https://github.com/qdraw/starsky/actions/runs/1636835300", "pull_requests": [ ], "created_at": "2021-12-30T08:30:33Z", "updated_at": "2021-12-30T08:49:44Z", "run_attempt": 1, "run_started_at": "2021-12-30T08:30:33Z", "jobs_url": "https://api.github.com/repos/qdraw/starsky/actions/runs/1636835300/jobs", "logs_url": "https://api.github.com/repos/qdraw/starsky/actions/runs/1636835300/logs", "check_suite_url": "https://api.github.com/repos/qdraw/starsky/check-suites/4791757190", "artifacts_url": "https://api.github.com/repos/qdraw/starsky/actions/runs/1636835300/artifacts", "cancel_url": "https://api.github.com/repos/qdraw/starsky/actions/runs/1636835300/cancel", "rerun_url": "https://api.github.com/repos/qdraw/starsky/actions/runs/1636835300/rerun", "previous_attempt_url": null, "workflow_url": "https://api.github.com/repos/qdraw/starsky/actions/workflows/17105174", "head_commit": { "id": "2d237de506d19642e3010d26af9c5c2da0bd35bf", "tree_id": "74711c390396f5595cc717f1e362ea9ce8ae7b15", "message": "Update and rename release-on-tag-netcore-electron.yml to release-on-tag-netcore-desktop-electron.yml", "timestamp": "2021-12-30T08:30:31Z", "author": { "name": "Dion van Velde", "email": "qdraw@users.noreply.github.com" }, "committer": { "name": "GitHub", "email": "noreply@github.com" } }, "repository": { "id": 238966719, "node_id": "MDEwOlJlcG9zaXRvcnkyMzg5NjY3MTk=", "name": "starsky", "full_name": "qdraw/starsky", "private": false, "owner": { "login": "qdraw", "id": 1826996, "node_id": "MDQ6VXNlcjE4MjY5OTY=", "avatar_url": "https://avatars.githubusercontent.com/u/1826996?v=4", "gravatar_id": "", "url": "https://api.github.com/users/qdraw", "html_url": "https://github.com/qdraw", "followers_url": "https://api.github.com/users/qdraw/followers", "following_url": "https://api.github.com/users/qdraw/following{/other_user}", "gists_url": "https://api.github.com/users/qdraw/gists{/gist_id}", "starred_url": "https://api.github.com/users/qdraw/starred{/owner}{/repo}", "subscriptions_url": "https://api.github.com/users/qdraw/subscriptions", "organizations_url": "https://api.github.com/users/qdraw/orgs", "repos_url": "https://api.github.com/users/qdraw/repos", "events_url": "https://api.github.com/users/qdraw/events{/privacy}", "received_events_url": "https://api.github.com/users/qdraw/received_events", "type": "User", "site_admin": false }, "html_url": "https://github.com/qdraw/starsky", "description": "Accelerator to find and organize images driven by meta information. Browse and search images in your own cloud.", "fork": false, "url": "https://api.github.com/repos/qdraw/starsky", "forks_url": "https://api.github.com/repos/qdraw/starsky/forks", "keys_url": "https://api.github.com/repos/qdraw/starsky/keys{/key_id}", "collaborators_url": "https://api.github.com/repos/qdraw/starsky/collaborators{/collaborator}", "teams_url": "https://api.github.com/repos/qdraw/starsky/teams", "hooks_url": "https://api.github.com/repos/qdraw/starsky/hooks", "issue_events_url": "https://api.github.com/repos/qdraw/starsky/issues/events{/number}", "events_url": "https://api.github.com/repos/qdraw/starsky/events", "assignees_url": "https://api.github.com/repos/qdraw/starsky/assignees{/user}", "branches_url": "https://api.github.com/repos/qdraw/starsky/branches{/branch}", "tags_url": "https://api.github.com/repos/qdraw/starsky/tags", "blobs_url": "https://api.github.com/repos/qdraw/starsky/git/blobs{/sha}", "git_tags_url": "https://api.github.com/repos/qdraw/starsky/git/tags{/sha}", "git_refs_url": "https://api.github.com/repos/qdraw/starsky/git/refs{/sha}", "trees_url": "https://api.github.com/repos/qdraw/starsky/git/trees{/sha}", "statuses_url": "https://api.github.com/repos/qdraw/starsky/statuses/{sha}", "languages_url": "https://api.github.com/repos/qdraw/starsky/languages", "stargazers_url": "https://api.github.com/repos/qdraw/starsky/stargazers", "contributors_url": "https://api.github.com/repos/qdraw/starsky/contributors", "subscribers_url": "https://api.github.com/repos/qdraw/starsky/subscribers", "subscription_url": "https://api.github.com/repos/qdraw/starsky/subscription", "commits_url": "https://api.github.com/repos/qdraw/starsky/commits{/sha}", "git_commits_url": "https://api.github.com/repos/qdraw/starsky/git/commits{/sha}", "comments_url": "https://api.github.com/repos/qdraw/starsky/comments{/number}", "issue_comment_url": "https://api.github.com/repos/qdraw/starsky/issues/comments{/number}", "contents_url": "https://api.github.com/repos/qdraw/starsky/contents/{+path}", "compare_url": "https://api.github.com/repos/qdraw/starsky/compare/{base}...{head}", "merges_url": "https://api.github.com/repos/qdraw/starsky/merges", "archive_url": "https://api.github.com/repos/qdraw/starsky/{archive_format}{/ref}", "downloads_url": "https://api.github.com/repos/qdraw/starsky/downloads", "issues_url": "https://api.github.com/repos/qdraw/starsky/issues{/number}", "pulls_url": "https://api.github.com/repos/qdraw/starsky/pulls{/number}", "milestones_url": "https://api.github.com/repos/qdraw/starsky/milestones{/number}", "notifications_url": "https://api.github.com/repos/qdraw/starsky/notifications{?since,all,participating}", "labels_url": "https://api.github.com/repos/qdraw/starsky/labels{/name}", "releases_url": "https://api.github.com/repos/qdraw/starsky/releases{/id}", "deployments_url": "https://api.github.com/repos/qdraw/starsky/deployments" }, "head_repository": { "id": 238966719, "node_id": "MDEwOlJlcG9zaXRvcnkyMzg5NjY3MTk=", "name": "starsky", "full_name": "qdraw/starsky", "private": false, "owner": { "login": "qdraw", "id": 1826996, "node_id": "MDQ6VXNlcjE4MjY5OTY=", "avatar_url": "https://avatars.githubusercontent.com/u/1826996?v=4", "gravatar_id": "", "url": "https://api.github.com/users/qdraw", "html_url": "https://github.com/qdraw", "followers_url": "https://api.github.com/users/qdraw/followers", "following_url": "https://api.github.com/users/qdraw/following{/other_user}", "gists_url": "https://api.github.com/users/qdraw/gists{/gist_id}", "starred_url": "https://api.github.com/users/qdraw/starred{/owner}{/repo}", "subscriptions_url": "https://api.github.com/users/qdraw/subscriptions", "organizations_url": "https://api.github.com/users/qdraw/orgs", "repos_url": "https://api.github.com/users/qdraw/repos", "events_url": "https://api.github.com/users/qdraw/events{/privacy}", "received_events_url": "https://api.github.com/users/qdraw/received_events", "type": "User", "site_admin": false }, "html_url": "https://github.com/qdraw/starsky", "description": "Accelerator to find and organize images driven by meta information. Browse and search images in your own cloud.", "fork": false, "url": "https://api.github.com/repos/qdraw/starsky", "forks_url": "https://api.github.com/repos/qdraw/starsky/forks", "keys_url": "https://api.github.com/repos/qdraw/starsky/keys{/key_id}", "collaborators_url": "https://api.github.com/repos/qdraw/starsky/collaborators{/collaborator}", "teams_url": "https://api.github.com/repos/qdraw/starsky/teams", "hooks_url": "https://api.github.com/repos/qdraw/starsky/hooks", "issue_events_url": "https://api.github.com/repos/qdraw/starsky/issues/events{/number}", "events_url": "https://api.github.com/repos/qdraw/starsky/events", "assignees_url": "https://api.github.com/repos/qdraw/starsky/assignees{/user}", "branches_url": "https://api.github.com/repos/qdraw/starsky/branches{/branch}", "tags_url": "https://api.github.com/repos/qdraw/starsky/tags", "blobs_url": "https://api.github.com/repos/qdraw/starsky/git/blobs{/sha}", "git_tags_url": "https://api.github.com/repos/qdraw/starsky/git/tags{/sha}", "git_refs_url": "https://api.github.com/repos/qdraw/starsky/git/refs{/sha}", "trees_url": "https://api.github.com/repos/qdraw/starsky/git/trees{/sha}", "statuses_url": "https://api.github.com/repos/qdraw/starsky/statuses/{sha}", "languages_url": "https://api.github.com/repos/qdraw/starsky/languages", "stargazers_url": "https://api.github.com/repos/qdraw/starsky/stargazers", "contributors_url": "https://api.github.com/repos/qdraw/starsky/contributors", "subscribers_url": "https://api.github.com/repos/qdraw/starsky/subscribers", "subscription_url": "https://api.github.com/repos/qdraw/starsky/subscription", "commits_url": "https://api.github.com/repos/qdraw/starsky/commits{/sha}", "git_commits_url": "https://api.github.com/repos/qdraw/starsky/git/commits{/sha}", "comments_url": "https://api.github.com/repos/qdraw/starsky/comments{/number}", "issue_comment_url": "https://api.github.com/repos/qdraw/starsky/issues/comments{/number}", "contents_url": "https://api.github.com/repos/qdraw/starsky/contents/{+path}", "compare_url": "https://api.github.com/repos/qdraw/starsky/compare/{base}...{head}", "merges_url": "https://api.github.com/repos/qdraw/starsky/merges", "archive_url": "https://api.github.com/repos/qdraw/starsky/{archive_format}{/ref}", "downloads_url": "https://api.github.com/repos/qdraw/starsky/downloads", "issues_url": "https://api.github.com/repos/qdraw/starsky/issues{/number}", "pulls_url": "https://api.github.com/repos/qdraw/starsky/pulls{/number}", "milestones_url": "https://api.github.com/repos/qdraw/starsky/milestones{/number}", "notifications_url": "https://api.github.com/repos/qdraw/starsky/notifications{?since,all,participating}", "labels_url": "https://api.github.com/repos/qdraw/starsky/labels{/name}", "releases_url": "https://api.github.com/repos/qdraw/starsky/releases{/id}", "deployments_url": "https://api.github.com/repos/qdraw/starsky/deployments" } } ] }'

ARTIFACTS_URL=$(grep -E -o "\"artifacts_url\":.+\"" <<< $RESULT_ACTIONS_WORKFLOW)
ARTIFACTS_URL=$(grep -E -o "https:\/\/(\w|\.|\/)+" <<< $ARTIFACTS_URL)
ARTIFACTS_URL=($ARTIFACTS_URL) # make array
ARTIFACTS_URL="${ARTIFACTS_URL[0]}" # first of array

if [[ $ARTIFACTS_URL != *artifacts ]]
then
  echo "url "$ARTIFACTS_URL" should end with zip";
  exit 1
fi

echo ">: "$ARTIFACTS_URL

CREATED_AT=$(grep -E -o "\"created_at\": \"(\d|-|T|:)+" <<< $RESULT_ACTIONS_WORKFLOW)
echo ">: "$CREATED_AT "UTC"

RESULT_ARTIFACTS=$(curl --user :$STARSKY_GITHUB_PAT -sS $ARTIFACTS_URL)
# RESULT_ARTIFACTS='{ "total_count": 8, "artifacts": [ { "id": 134130455, "node_id": "MDg6QXJ0aWZhY3QxMzQxMzA0NTU=", "name": "linux-arm", "size_in_bytes": 59554261, "url": "https://api.github.com/repos/qdraw/starsky/actions/artifacts/134130455", "archive_download_url": "https://api.github.com/repos/qdraw/starsky/actions/artifacts/134130455/zip", "expired": false, "created_at": "2021-12-30T09:54:52Z", "updated_at": "2021-12-30T09:54:53Z", "expires_at": "2022-03-30T09:50:13Z" }, { "id": 134130456, "node_id": "MDg6QXJ0aWZhY3QxMzQxMzA0NTY=", "name": "linux-arm64", "size_in_bytes": 57566841, "url": "https://api.github.com/repos/qdraw/starsky/actions/artifacts/134130456", "archive_download_url": "https://api.github.com/repos/qdraw/starsky/actions/artifacts/134130456/zip", "expired": false, "created_at": "2021-12-30T09:54:52Z", "updated_at": "2021-12-30T09:54:53Z", "expires_at": "2022-03-30T09:50:20Z" }, { "id": 134130457, "node_id": "MDg6QXJ0aWZhY3QxMzQxMzA0NTc=", "name": "linux-x64", "size_in_bytes": 57625720, "url": "https://api.github.com/repos/qdraw/starsky/actions/artifacts/134130457", "archive_download_url": "https://api.github.com/repos/qdraw/starsky/actions/artifacts/134130457/zip", "expired": false, "created_at": "2021-12-30T09:54:52Z", "updated_at": "2021-12-30T09:54:53Z", "expires_at": "2022-03-30T09:50:32Z" }, { "id": 134130458, "node_id": "MDg6QXJ0aWZhY3QxMzQxMzA0NTg=", "name": "osx-x64", "size_in_bytes": 57167629, "url": "https://api.github.com/repos/qdraw/starsky/actions/artifacts/134130458", "archive_download_url": "https://api.github.com/repos/qdraw/starsky/actions/artifacts/134130458/zip", "expired": false, "created_at": "2021-12-30T09:54:52Z", "updated_at": "2021-12-30T09:54:53Z", "expires_at": "2022-03-30T09:50:39Z" }, { "id": 134130459, "node_id": "MDg6QXJ0aWZhY3QxMzQxMzA0NTk=", "name": "starsky-mac-desktop", "size_in_bytes": 135746768, "url": "https://api.github.com/repos/qdraw/starsky/actions/artifacts/134130459", "archive_download_url": "https://api.github.com/repos/qdraw/starsky/actions/artifacts/134130459/zip", "expired": false, "created_at": "2021-12-30T09:54:52Z", "updated_at": "2021-12-30T09:54:53Z", "expires_at": "2022-03-30T09:54:31Z" }, { "id": 134130460, "node_id": "MDg6QXJ0aWZhY3QxMzQxMzA0NjA=", "name": "starsky-tools-slack-notification", "size_in_bytes": 3190, "url": "https://api.github.com/repos/qdraw/starsky/actions/artifacts/134130460", "archive_download_url": "https://api.github.com/repos/qdraw/starsky/actions/artifacts/134130460/zip", "expired": false, "created_at": "2021-12-30T09:54:52Z", "updated_at": "2021-12-30T09:54:53Z", "expires_at": "2022-03-30T09:50:44Z" }, { "id": 134130461, "node_id": "MDg6QXJ0aWZhY3QxMzQxMzA0NjE=", "name": "starsky-win-desktop", "size_in_bytes": 139103431, "url": "https://api.github.com/repos/qdraw/starsky/actions/artifacts/134130461", "archive_download_url": "https://api.github.com/repos/qdraw/starsky/actions/artifacts/134130461/zip", "expired": false, "created_at": "2021-12-30T09:54:52Z", "updated_at": "2021-12-30T09:54:53Z", "expires_at": "2022-03-30T09:53:47Z" }, { "id": 134130462, "node_id": "MDg6QXJ0aWZhY3QxMzQxMzA0NjI=", "name": "win7-x64", "size_in_bytes": 58232697, "url": "https://api.github.com/repos/qdraw/starsky/actions/artifacts/134130462", "archive_download_url": "https://api.github.com/repos/qdraw/starsky/actions/artifacts/134130462/zip", "expired": false, "created_at": "2021-12-30T09:54:52Z", "updated_at": "2021-12-30T09:54:53Z", "expires_at": "2022-03-30T09:50:26Z" } ] }'

# ([0-9a-zA-Z]|\/|:| |,|\"|_|\.|\t|\n|\r|-)

DOWNLOAD_URL=$(echo $RESULT_ARTIFACTS|tr -d '\n')
INDEX_DOWNLOAD_URL=$(echo $DOWNLOAD_URL | grep -aob $VERSION"\"" --color=never | \grep -oE '^[0-9]+')
DOWNLOAD_URL="${DOWNLOAD_URL:INDEX_DOWNLOAD_URL}"

DOWNLOAD_URL=$(grep -E -o "\"archive_download_url\": \"(\d|\.|\w|\:|\/)+" <<< $DOWNLOAD_URL)

DOWNLOAD_URL=$(echo "$DOWNLOAD_URL" | sed "s/\"archive_download_url\": \"//")
DOWNLOAD_URL=($DOWNLOAD_URL) # make array
DOWNLOAD_URL="${DOWNLOAD_URL[0]}" # first of array

if [[ $DOWNLOAD_URL != *zip ]]
then
  echo "url "$DOWNLOAD_URL" should end with zip";
  exit 1
fi
echo ">: $DOWNLOAD_URL"

# check if hash is already downloaded
GITHUB_HEAD_SHA=$(echo $RESULT_ARTIFACTS|tr -d '\n')
GITHUB_HEAD_SHA=$(grep -E -o "\"head_sha\": \"(\d|\.|\w|\:|\/)+" <<< $GITHUB_HEAD_SHA)
GITHUB_HEAD_SHA=$(echo "$GITHUB_HEAD_SHA" | sed "s/\"head_sha\": \"//")
GITHUB_HEAD_SHA=($GITHUB_HEAD_SHA) # make array
GITHUB_HEAD_SHA="${GITHUB_HEAD_SHA[0]}" # first of array

GITHUB_HEAD_SHA_CACHE_FILE="${OUTPUT_DIR}${VERSION_ZIP}.sha-cache.txt"
echo "check for GITHUB_HEAD_SHA_CACHE_FILE $GITHUB_HEAD_SHA_CACHE_FILE"

LAST_GITHUB_HEAD_SHA=0
if [[ -f "$GITHUB_HEAD_SHA_CACHE_FILE" ]]; then
    LAST_GITHUB_HEAD_SHA="$(cat $GITHUB_HEAD_SHA_CACHE_FILE)"
    LAST_GITHUB_HEAD_SHA=`echo $LAST_GITHUB_HEAD_SHA | sed -e 's/^[[:space:]]*//'`
fi 

if [[ $LAST_GITHUB_HEAD_SHA == $GITHUB_HEAD_SHA ]]; then
    echo "$GITHUB_HEAD_SHA exists."
    echo ">>      Skips download of file"
    exit 0;
else 
    echo $GITHUB_HEAD_SHA" does not exists"
fi
    
# set the new hash
echo $GITHUB_HEAD_SHA > $GITHUB_HEAD_SHA_CACHE_FILE
# END check if hash is already downloaded


mkdir -p $OUTPUT_DIR

OUTPUT_ZIP_PATH="${OUTPUT_DIR}${VERSION_ZIP}"
echo "output file: "$OUTPUT_ZIP_PATH
 
curl -sS -L --user :$STARSKY_GITHUB_PAT $DOWNLOAD_URL -o "${OUTPUT_ZIP_PATH}_tmp.zip"
if [ ! -f "${OUTPUT_ZIP_PATH}_tmp.zip" ]; then
    echo "${OUTPUT_ZIP_PATH}_tmp.zip" " is NOT downloaded"
    exit 1
fi

if [ -f "$OUTPUT_ZIP_PATH" ]; then
    rm ${OUTPUT_ZIP_PATH}
fi

# contains an zip in a zip
unzip -q -o -j "${OUTPUT_ZIP_PATH}_tmp.zip" -d "${OUTPUT_DIR}temp"

if [ ! -f "${OUTPUT_DIR}temp/${VERSION_ZIP}" ]; then
    echo "${OUTPUT_DIR}temp/${VERSION_ZIP}" " is NOT unpacked"

    rm -rf "${OUTPUT_DIR}temp"

    exit 1
fi

# move file 
mv "${OUTPUT_DIR}temp/${VERSION_ZIP}" $OUTPUT_ZIP_PATH
rm -rf "${OUTPUT_DIR}temp"
rm "${OUTPUT_ZIP_PATH}_tmp.zip"

echo "zip is downloaded"

if [[ $VERSION != *desktop ]]
then
    echo "YEAH > download for "$RUNTIME" looks ok"
    echo "get pm2-new-instance.sh installer file" "${OUTPUT_DIR}pm2-new-instance.sh"
    unzip -p "starsky-"$RUNTIME".zip" "pm2-new-instance.sh" > "${OUTPUT_DIR}__pm2-new-instance.sh"
    
    if [ -f "${OUTPUT_DIR}__pm2-new-instance.sh" ]; then
        # check if file contains something
        if [ -s "${OUTPUT_DIR}__pm2-new-instance.sh" ]; then
           mv "${OUTPUT_DIR}__pm2-new-instance.sh" "${OUTPUT_DIR}pm2-new-instance.sh"
        else 
            rm "${OUTPUT_DIR}__pm2-new-instance.sh"
        fi
        
        chmod +rwx "${OUTPUT_DIR}pm2-new-instance.sh"
        echo "run for the setup:"
        # output dir should have slash at end
        echo $OUTPUT_DIR"pm2-new-instance.sh"
    else 
        echo " pm2-new-instance.sh is missing, please download it yourself and run it"
        exit 1
    fi
fi 

exit 0