import * as fs from "fs";
import * as path from "path";

function copyContent(
  source: string,
  target: string,
  match: RegExp,
  toRelativeFolder: string = null,
) {
  const files = fs.readdirSync(source);

  let targetFolder = target;
  if (toRelativeFolder !== null) {
    targetFolder = path.join(target, toRelativeFolder);

    if (!fs.existsSync(targetFolder)) {
      fs.mkdirSync(targetFolder);
    }
  }

  // eslint-disable-next-line no-restricted-syntax
  for (const file of files) {
    const curSource = path.join(source, file);
    if (fs.lstatSync(curSource).isDirectory()) {
      let newToRelative = file;
      if (toRelativeFolder != null) {
        newToRelative = path.join(toRelativeFolder, file);
      }
      copyContent(curSource, target, match, newToRelative);
      continue;
    }
    if (match.test(curSource)) {
      const targetFile = path.join(targetFolder, path.basename(curSource));
      fs.copyFileSync(curSource, targetFile);
    }
  }
}

function htmlCopy() {
  const srcFolder = path.join(__dirname, "..", "..", "src");
  const buildFolder = path.join(__dirname, "..", "..", "build");

  copyContent(srcFolder, buildFolder, /(.html|.css|.svg|.woff|.woff2)$/);
}

htmlCopy();
