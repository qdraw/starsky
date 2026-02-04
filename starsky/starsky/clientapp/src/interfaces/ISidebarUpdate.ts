export interface ISidebarUpdate {
  tags: string;
  description: string;
  title: string;
  collections: boolean;

  // update to update/ overwrite
  append?: boolean;

  // used for replaceing
  replaceTags?: string;
  replaceDescription?: string;
  replaceTitle?: string;
}

export interface ISidebarGenericUpdate {
  [key: string]: string | undefined;
}
