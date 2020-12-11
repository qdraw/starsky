export default function OsBuildKey() {
  // osKey is used in the build script

  switch (process.platform) {
    case "darwin":
      return "mac";
    case "win32": // the 32 does not say anything it can also be a x64 version
      return "win";
    default:
      return "";
  }
}
