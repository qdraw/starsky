#!/usr/bin/node

const fs = require("fs");
const path = require("path");
const { parseAndWrite } = require("./openapi");

const parentDirectory = path.join(__dirname, "..", "..");
const docsDirectory = path.join(__dirname, "..", "docs");

function copyFileSync(input, to) {
  const inputPath = path.join(parentDirectory, input);

  const relativeParentInputFolder = path.dirname(to);

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
  
  if (relativeParentInputFolder !== ".") {
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

copyFileSync("history.md", "advanced-options/history.md");
copyFileSync("starsky/telemetry.md", "advanced-options/starsky/telemetry.md");

touchSync("advanced-options/__do_not_edit_history_md");

touchSync("advanced-options/starsky/__do_not_edit_this__folder");

writeFile(
  "advanced-options/starsky/_category_.json",
  JSON.stringify({
    label: "Applications Guide",
    position: 2,
    link: {
      type: "generated-index",
      description: "CLI tools/ Applications",
    },
  })
);

copyFileSync("starsky/readme.md", "advanced-options/starsky/readme.md");

copyFileSync("starsky/starsky/readme.md", "advanced-options/starsky/starsky/readme.md");
copyFileSync(
  "starsky/starsky/readme-docker-hub.md",
  "advanced-options/starsky/starsky/readme-docker-hub.md"
);

copyFileSync(
  "starsky/starsky/readme-docker-development.md",
  "advanced-options/starsky/starsky/readme-docker-development.md"
);

copyFileSync(
  "starsky/starsky/clientapp/readme.md",
  "advanced-options/starsky/starsky/clientapp/readme.md"
);
copyFileSync(
  "starsky/starskyimportercli/readme.md",
  "advanced-options/starsky/starskyimportercli/readme.md"
);
copyFileSync(
  "starsky/starskygeocli/readme.md",
  "advanced-options/starsky/starskygeocli/readme.md"
);
copyFileSync(
  "starsky/starskywebhtmlcli/readme.md",
  "advanced-options/starsky/starskywebhtmlcli/readme.md"
);
copyFileSync(
  "starsky/starskywebftpcli/readme.md",
  "advanced-options/starsky/starskywebftpcli/readme.md"
);
copyFileSync(
  "starsky/starskyadmincli/readme.md",
  "advanced-options/starsky/starskyadmincli/readme.md"
);
copyFileSync(
  "starsky/starskysynchronizecli/readme.md",
  "advanced-options/starsky/starskysynchronizecli/readme.md"
);

copyFileSync(
  "starsky/starskythumbnailcli/readme.md",
  "advanced-options/starsky/starskythumbnailcli/readme.md"
);

copyFileSync(
  "starsky/starskybusinesslogic/readme.md",
  "advanced-options/starsky/starskybusinesslogic/readme.md"
);

copyFileSync("starskydesktop/readme.md", "advanced-options/starskydesktop/readme.md");

touchSync("advanced-options/starskydesktop/__do_not_edit_this__folder");

copyFileSync(
  "starskydesktop/docs-assets/starskyapp-mac-gatekeeper.jpg",
  "advanced-options/starskydesktop/docs-assets/starskyapp-mac-gatekeeper.jpg"
);

copyFileSync(
  "starskydesktop/docs-assets/starskyapp-remote-options-v040.jpg",
  "advanced-options/starskydesktop/docs-assets/starskyapp-remote-options-v040.jpg"
);

copyFileSync("starsky/starskytest/readme.md", "advanced-options/starsky/starskytest/readme.md");

copyFileSync("starsky-tools/readme.md", "advanced-options/starsky-tools/readme.md");

touchSync("advanced-options/starsky-tools/__do_not_edit_this__folder");

copyFileSync(
  "starsky-tools/build-tools/readme.md",
  "advanced-options/starsky-tools/build-tools/readme.md"
);

copyFileSync(
  "starsky-tools/dropbox-import/readme.md",
  "advanced-options/starsky-tools/dropbox-import/readme.md"
);

copyFileSync(
  "starsky-tools/end2end/readme.md",
  "advanced-options/starsky-tools/end2end/readme.md"
);

copyFileSync(
  "starsky-tools/localtunnel/readme.md",
  "advanced-options/starsky-tools/localtunnel/readme.md"
);

copyFileSync("starsky-tools/mail/readme.md", "advanced-options/starsky-tools/mail/readme.md");

copyFileSync(
  "starsky-tools/thumbnail/readme.md",
  "advanced-options/starsky-tools/thumbnail/readme.md"
);

copyFileSync(
  "starsky-tools/release-tools/readme.md",
  "advanced-options/starsky-tools/release-tools/readme.md"
);

copyFileSync(
  "starsky-tools/slack-notification/readme.md",
  "advanced-options/starsky-tools/slack-notification/readme.md"
);

copyFileSync("starsky-tools/sync/readme.md", "advanced-options/starsky-tools/sync/readme.md");


parseAndWrite();
