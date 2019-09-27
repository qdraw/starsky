import React, { useEffect } from "react";
import { ArchiveContext } from '../contexts/archive-context';
import useLocation from '../hooks/use-location';
import { ISidebarUpdate } from '../interfaces/ISidebarUpdate';
import FetchPost from '../shared/fetch-post';
import { SidebarUpdate } from '../shared/sidebar-update';
import { URLPath } from '../shared/url-path';


const ArchiveSidebarLabelEditAddOverwrite: React.FunctionComponent = () => {

  var history = useLocation();
  let { state, dispatch } = React.useContext(ArchiveContext);
  var archive = state;

  // show select info
  const [select, setSelect] = React.useState(new URLPath().getSelect(history.location.search));
  useEffect(() => {
    setSelect(new URLPath().getSelect(history.location.search));
  }, [history.location.search]);

  // The Updated that is send to the api
  const [update, setUpdate] = React.useState({
    append: true,
    collections: true,
  } as ISidebarUpdate)

  // Add/Hide disabled state
  const [isInputEnabled, setInputEnabled] = React.useState(false);

  // Update the disabled state + Local variable with input data
  function handleUpdateChange(event: React.ChangeEvent<HTMLDivElement> | React.KeyboardEvent<HTMLDivElement>) {
    let fieldValue = event.currentTarget.innerText.trim();
    let fieldName = event.currentTarget.dataset["name"];
    if (!fieldName) return;
    if (!fieldValue) return;

    var updateSidebar = new SidebarUpdate().CastToISideBarUpdate(fieldName, fieldValue, update);
    setUpdate(updateSidebar);
    setInputEnabled(new SidebarUpdate().IsFormUsed(update));
  }

  /**
   * To update the archive
   * @param append to Add to the existing 
   */
  function pushUpdate(append: boolean) {
    update.append = append;

    var bodyParams = new URLPath().ObjectToSearchParams(update);
    if (bodyParams.toString().length === 0) return;

    var subPaths = new URLPath().MergeSelectFileIndexItem(select, archive.fileIndexItems);
    if (!subPaths) return;
    var selectParams = new URLPath().ArrayToCommaSeperatedStringOneParent(subPaths, "")

    if (selectParams.length === 0) return;
    bodyParams.append("f", selectParams)

    FetchPost("/api/update", bodyParams.toString());

    dispatch({ type: 'update', ...update, select });
  }

  return (
    <>
      <h4>Tags:</h4>
      <div data-name="tags"
        onInput={handleUpdateChange}
        suppressContentEditableWarning={true}
        contentEditable={!archive.isReadOnly && select.length !== 0}
        className={!archive.isReadOnly && select.length !== 0 ? "form-control" : "form-control disabled"}>
      </div>
      <h4>Info</h4>
      <div
        onInput={handleUpdateChange}
        data-name="description"
        suppressContentEditableWarning={true}
        contentEditable={!archive.isReadOnly && select.length !== 0}
        className={!archive.isReadOnly && select.length !== 0 ? "form-control" : "form-control disabled"}>
      </div>
      <h4>Titel</h4>
      <div data-name="title"
        onInput={handleUpdateChange}
        suppressContentEditableWarning={true}
        contentEditable={!archive.isReadOnly && select.length !== 0}
        className={!archive.isReadOnly && select.length !== 0 ? "form-control" : "form-control disabled"}>
      </div>

      {isInputEnabled && select.length !== 0 ? <button className="btn btn--info"
        onClick={() => pushUpdate(false)}>Overschrijven</button> :
        <button disabled className="btn btn--default disabled" >Overschrijven</button>}
      {isInputEnabled && select.length !== 0 ?
        <button className="btn btn--default" onClick={() => pushUpdate(true)}>Toevoegen</button> :
        <button disabled className="btn btn--default disabled" >Toevoegen</button>}
    </>
  );
};
export default ArchiveSidebarLabelEditAddOverwrite
