import React, { useEffect } from "react";
import { ArchiveContext } from '../contexts/archive-context';
import useLocation from '../hooks/use-location';
import { ISidebarUpdate } from '../interfaces/ISidebarUpdate';
import { SidebarUpdate } from '../shared/sidebar-update';
import { URLPath } from '../shared/url-path';


const ArchiveSidebarLabelEditSearchReplace: React.FunctionComponent = () => {

  var history = useLocation();
  let { state, dispatch } = React.useContext(ArchiveContext);
  var archive = state;

  // show select info
  const [select, setSelect] = React.useState(new URLPath().getSelect(history.location.search));
  useEffect(() => {
    setSelect(new URLPath().getSelect(history.location.search));
  }, [history.location.search]);

  // The Updated that is send to the api
  const [update, setUpdate] = React.useState({} as ISidebarUpdate)

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
   * To search and replace
   */
  function pushSearchAndReplace() {
    update.append = false;
    var subPaths = new URLPath().MergeSelectFileIndexItem(select, archive.fileIndexItems);
    if (!subPaths) return;
    var selectPaths = new URLPath().ArrayToCommaSeperatedStringOneParent(subPaths, "")

    if (selectPaths.length === 0) return;

    var bodyParams = new URLSearchParams();
    bodyParams.append("f", selectPaths);

    for (let key of Object.entries(update)) {
      if (key[1] && key[1].length >= 1) {
        console.log(key);
        bodyParams.set("fieldName", key[0]);
        bodyParams.set("search", key[1]);

        // var replace: string = (updateReplace as any)[key[0]] ? (updateReplace as any)[key[0]] : "";
        // bodyParams.set("replace", replace);

        // if (key[0] === "tags") {
        //   var regexer = new RegExp(key[1], "g")
        //   update.tags = update.tags.replace(regexer, replace)
        // }

        // FetchPost("/api/replace", bodyParams.toString())
        dispatch({ type: 'update', ...update, select });
      }
    }
  }

  return (
    <>
      <h4>Tags:</h4>
      <div data-name="tags"
        onInput={handleUpdateChange}
        suppressContentEditableWarning={true}
        contentEditable={!archive.isReadOnly && select.length !== 0}
        className={!archive.isReadOnly && select.length !== 0 ? "form-control form-control--half inline-block" : "form-control form-control--half inline-block disabled"}>
        &nbsp;
          </div>
      <div data-name="replaceToTags"
        onInput={handleUpdateChange}
        suppressContentEditableWarning={true}
        contentEditable={!archive.isReadOnly && select.length !== 0}
        className={!archive.isReadOnly && select.length !== 0 ? "form-control form-control--half inline-block" : "form-control form-control--half inline-block disabled"}>
        &nbsp;
          </div>
      <h4>Info</h4>
      <div
        onInput={handleUpdateChange}
        data-name="description"
        suppressContentEditableWarning={true}
        contentEditable={!archive.isReadOnly && select.length !== 0}
        className={!archive.isReadOnly && select.length !== 0 ? "form-control form-control--half inline-block" : "form-control form-control--half inline-block disabled"}>
        &nbsp;
          </div>
      <div
        onInput={handleUpdateChange}
        data-name="replaceTodescription"
        suppressContentEditableWarning={true}
        contentEditable={!archive.isReadOnly && select.length !== 0}
        className={!archive.isReadOnly && select.length !== 0 ? "form-control form-control--half inline-block" : "form-control form-control--half inline-block disabled"}>
        &nbsp;
          </div>
      <h4>Titel</h4>
      <div data-name="title"
        onInput={handleUpdateChange}
        suppressContentEditableWarning={true}
        contentEditable={!archive.isReadOnly && select.length !== 0}
        className={!archive.isReadOnly && select.length !== 0 ? "form-control form-control--half inline-block" : "form-control form-control--half inline-block disabled"}>
        &nbsp;
          </div>
      <div data-name="replaceTotitle"
        onInput={handleUpdateChange}
        suppressContentEditableWarning={true}
        contentEditable={!archive.isReadOnly && select.length !== 0}
        className={!archive.isReadOnly && select.length !== 0 ? "form-control form-control--half inline-block" : "form-control form-control--half inline-block disabled"}>
        &nbsp;
          </div>

      <div className="warning-box">Test functionaliteit is aangezet</div>

      {isInputEnabled && select.length !== 0 ?
        <button className="btn btn--default" onClick={() => pushSearchAndReplace()}>Vervangen</button> :
        <button disabled className="btn btn--default disabled" >Vervangen</button>}
    </>
  );
};
export default ArchiveSidebarLabelEditSearchReplace
