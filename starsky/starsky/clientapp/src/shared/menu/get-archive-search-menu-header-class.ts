export function GetArchiveSearchMenuHeaderClass(
  sidebar: boolean | undefined,
  select: string[] | undefined
): string {
  if (sidebar) {
    return "header header--main header--select header--edit";
  } else if (select) {
    return "header header--main header--select";
  } else {
    return "header header--main";
  }
}
