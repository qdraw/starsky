import * as fs from "fs";
import * as path from "path";

function copyFileSync(source: string, target: string) {
  let targetFile = target;

  // If target is a directory, a new file with the same name will be created
  if (fs.existsSync(target)) {
    if (fs.lstatSync(target).isDirectory()) {
      targetFile = path.join(target, path.basename(source));
    }
  }

  fs.writeFileSync(targetFile, fs.readFileSync(source));
}

export function copyFolderRecursiveSync(
  source: string,
  target: string,
  match: RegExp = null
) {
  let files = [];

  // Check if folder needs to be created or integrated

  const targetFolder = path.join(target, path.basename(source));

  if (!fs.existsSync(targetFolder)) {
    fs.mkdirSync(targetFolder);
  }

  // Copy
  if (fs.lstatSync(source).isDirectory()) {
    files = fs.readdirSync(source);
    files.forEach((file: string) => {
      const curSource = path.join(source, file);
      if (fs.lstatSync(curSource).isDirectory()) {
        copyFolderRecursiveSync(curSource, targetFolder, match);
      } else if (match === null) {
        copyFileSync(curSource, targetFolder);
      } else if (match.test(curSource)) {
        copyFileSync(curSource, targetFolder);
      }
    });
  }
}

export function copyWithId(identifier: string, to: string) {
  const from = path.join(__dirname, "..", "..", "..", "starsky", identifier);

  console.log(`copy from ${from} to -> ${to}`);

  fs.mkdirSync(to, {recursive:true});

  copyFolderRecursiveSync(from, to);

  // make write if exists
  const appStarskyPath = path.join(to, "starsky");
  try {
    fs.chmodSync(appStarskyPath, 0o755);
  } catch (error) {
    // do nothing
  }

  

  // if (fs.existsSync(to)) {
  //   try {
  //     fs.rmSync(to, { recursive: true });
  //   } catch (err) {
  //     console.log(err);
  //   }
  // }

  // const afterCopyPath = path.join(__dirname, "..", "..", identifier);

  // try {
  //   fs.renameSync(afterCopyPath, to);
  // } catch (error) {
  //   console.log(error);
  // }

  console.log("-- copy folder done --");
}
