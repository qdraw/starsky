#!/usr/bin/node

const fs = require("fs");
const { spawnSync } = require("child_process");
const path = require("path");
const { env, exit } = require("process");

const parentDirectory = path.join(__dirname, "..", "..");
const docsDirectory = path.join(__dirname, "..", "docs");

function copyFileSync(input, to) {
  const inputPath = path.join(parentDirectory, input);

  const relativeParentInputFolder = path.dirname(input);

  if (relativeParentInputFolder != ".") {
    const relativeParentInputFolderPath = path.join(
      docsDirectory,
      relativeParentInputFolder
    );
    fs.mkdirSync(relativeParentInputFolderPath, { recursive: true });
  }

  fs.copyFileSync(inputPath, path.join(docsDirectory, to));
}

function touchSync(to) {
  const filename = path.join(docsDirectory, to);

  const relativeParentInputFolder = path.dirname(to);

  if (relativeParentInputFolder != ".") {
    const relativeParentInputFolderPath = path.join(
      docsDirectory,
      relativeParentInputFolder
    );
    fs.mkdirSync(relativeParentInputFolderPath, { recursive: true });
  }

  const time = new Date();
  try {
    fs.utimesSync(filename, time, time);
  } catch (e) {
    let fd = fs.openSync(filename, "a");
    fs.closeSync(fd);
  }
}

function writeFile(to, content) {
  const filename = path.join(docsDirectory, to);
  fs.writeFileSync(filename, content);
}

copyFileSync("history.md", "history.md");
touchSync("starsky/__do_not_edit_this__folder");

writeFile(
  "starsky/_category_.json",
  JSON.stringify({
    label: "Applications Guide",
    position: 2,
    link: {
      type: "generated-index",
      description: "CLI tools/ Applications",
    },
  })
);

copyFileSync("starsky/readme.md", "starsky/readme.md");

copyFileSync("starsky/starsky/readme.md", "starsky/starsky/readme.md");
copyFileSync(
  "starsky/starsky/readme-docker-hub.md",
  "starsky/starsky/readme-docker-hub.md"
);

copyFileSync(
  "starsky/starsky/readme-docker-development.md",
  "starsky/starsky/readme-docker-development.md"
);

copyFileSync(
  "starsky/starsky/clientapp/readme.md",
  "starsky/starsky/clientapp/readme.md"
);
copyFileSync(
  "starsky/starskyimportercli/readme.md",
  "starsky/starskyimportercli/readme.md"
);
copyFileSync(
  "starsky/starskygeocli/readme.md",
  "starsky/starskygeocli/readme.md"
);
copyFileSync(
  "starsky/starskywebhtmlcli/readme.md",
  "starsky/starskywebhtmlcli/readme.md"
);
copyFileSync(
  "starsky/starskywebftpcli/readme.md",
  "starsky/starskywebftpcli/readme.md"
);
copyFileSync(
  "starsky/starskyadmincli/readme.md",
  "starsky/starskyadmincli/readme.md"
);
copyFileSync(
  "starsky/starskysynchronizecli/readme.md",
  "starsky/starskysynchronizecli/readme.md"
);

copyFileSync(
  "starsky/starskythumbnailcli/readme.md",
  "starsky/starskythumbnailcli/readme.md"
);

copyFileSync(
  "starsky/starskybusinesslogic/readme.md",
  "starsky/starskybusinesslogic/readme.md"
);

copyFileSync("starskydesktop/readme.md", "starskydesktop/readme.md");
touchSync("starskydesktop/__do_not_edit_this__folder");

copyFileSync(
  "starskydesktop/docs-assets/starskyapp-mac-gatekeeper.jpg",
  "starskydesktop/docs-assets/starskyapp-mac-gatekeeper.jpg"
);

copyFileSync(
  "starskydesktop/docs-assets/starskyapp-remote-options-v040.jpg",
  "starskydesktop/docs-assets/starskyapp-remote-options-v040.jpg"
);

copyFileSync("starsky/starskytest/readme.md", "starsky/starskytest/readme.md");

copyFileSync("starsky-tools/readme.md", "starsky-tools/readme.md");

copyFileSync(
  "starsky-tools/build-tools/readme.md",
  "starsky-tools/build-tools/readme.md"
);

copyFileSync("starsky-tools/docs/readme.md", "starsky-tools/docs/readme.md");

copyFileSync(
  "starsky-tools/dropbox-import/readme.md",
  "starsky-tools/dropbox-import/readme.md"
);

copyFileSync(
  "starsky-tools/end2end/readme.md",
  "starsky-tools/end2end/readme.md"
);

copyFileSync(
  "starsky-tools/localtunnel/readme.md",
  "starsky-tools/localtunnel/readme.md"
);

copyFileSync("starsky-tools/mail/readme.md", "starsky-tools/mail/readme.md");

copyFileSync(
  "starsky-tools/thumbnail/readme.md",
  "starsky-tools/thumbnail/readme.md"
);

copyFileSync(
  "starsky-tools/release-tools/readme.md",
  "starsky-tools/release-tools/readme.md"
);

copyFileSync(
  "starsky-tools/slack-notification/readme.md",
  "starsky-tools/slack-notification/readme.md"
);

copyFileSync("starsky-tools/sync/readme.md", "starsky-tools/sync/readme.md");
