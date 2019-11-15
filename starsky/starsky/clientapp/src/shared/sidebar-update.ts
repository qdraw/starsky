import { ISidebarUpdate } from '../interfaces/ISidebarUpdate';

export class SidebarUpdate {

  /**
  * Cast Update Fields to ISidebarUpdate
  * @param fieldName e.g. tags
  * @param fieldValue the value
  * @param updateSidebar 'From Object'
  */
  public CastToISideBarUpdate = (fieldName: string, fieldValue: string, updateSidebar: ISidebarUpdate): ISidebarUpdate => {
    if (!fieldName) return updateSidebar;
    if (!fieldValue) return updateSidebar;

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
  }

  public Change = (event: React.ChangeEvent<HTMLDivElement> | React.KeyboardEvent<HTMLDivElement>, update: ISidebarUpdate): ISidebarUpdate | null => {
    if (!event.currentTarget.textContent) return null;
    let fieldValue = event.currentTarget.textContent.trim();
    let fieldName = event.currentTarget.dataset["name"];

    if (!fieldName) return null;
    if (!fieldValue) return null;

    return new SidebarUpdate().CastToISideBarUpdate(fieldName, fieldValue, update);
  }

  public IsFormUsed = (updateSidebar: ISidebarUpdate): boolean => {
    var totalChars = 0;
    if (updateSidebar.tags) {
      totalChars += updateSidebar.tags.length
    }
    if (updateSidebar.description) {
      totalChars += updateSidebar.description.length
    }
    if (updateSidebar.title) {
      totalChars += updateSidebar.title.length
    }
    return totalChars !== 0
  }

}