export interface IBatchRenameRequest {
  filePaths: string[];
  pattern: string;
  collections: boolean;
}
