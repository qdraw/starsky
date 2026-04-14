export interface IFolderPickerMessage {
  action: "selectFolder" | "folderSelected";
  folderPath?: string;
}
