import { ISidebarUpdate } from "../interfaces/ISidebarUpdate";

export class SidebarUpdate {
  /**
   * Cast Update Fields to ISidebarUpdate
   * @param fieldName e.g. tags
   * @param fieldValue the value
   * @param updateSidebar 'From Object'
   */
  public CastToISideBarUpdate = (
    fieldName: string,
    fieldValue: string,
    updateSidebar: ISidebarUpdate
  ): ISidebarUpdate => {
    if (!fieldName) return updateSidebar;
    if (!fieldValue) {
      delete (updateSidebar as Partial<ISidebarUpdate>)[fieldName as keyof ISidebarUpdate];
      return updateSidebar;
    }

    fieldValue = fieldValue.replace(/\n/g, "");
    switch (fieldName.toLowerCase()) {
      case "tags":
        updateSidebar.tags = fieldValue;
        break;
      case "description":
        updateSidebar.description = fieldValue;
        break;
      case "title":
        updateSidebar.title = fieldValue;
        break;
      // now the updated types
      case "replace-tags":
        updateSidebar.replaceTags = fieldValue;
        break;
      case "replace-description":
        updateSidebar.replaceDescription = fieldValue;
        break;
      case "replace-title":
        updateSidebar.replaceTitle = fieldValue;
        break;
    }
    return updateSidebar;
  };

  public Change = (
    event: React.ChangeEvent<HTMLDivElement> | React.KeyboardEvent<HTMLDivElement>,
    update: ISidebarUpdate
  ): ISidebarUpdate | null => {
    const fieldValue = event.currentTarget.textContent
      ? event.currentTarget.textContent.trim()
      : "";
    const fieldName = event.currentTarget.dataset["name"];

    if (!fieldName) return null;

    return new SidebarUpdate().CastToISideBarUpdate(fieldName, fieldValue, update);
  };

  public IsFormUsed = (updateSidebar: ISidebarUpdate): boolean => {
    let totalChars = 0;
    if (updateSidebar.tags) {
      totalChars += updateSidebar.tags.length;
    }
    if (updateSidebar.description) {
      totalChars += updateSidebar.description.length;
    }
    if (updateSidebar.title) {
      totalChars += updateSidebar.title.length;
    }
    return totalChars !== 0;
  };
}
