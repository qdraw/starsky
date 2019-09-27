export interface ISidebarUpdate {
  tags: string,
  description: string,
  title: string,
  collections: boolean,

  // update to update/ overwrite
  append?: boolean,

  // used for replaceing
  replaceToTags?: string,
  replaceToDescription?: string,
  replaceTotitle?: string,
}
