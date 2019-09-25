import React, { memo, useEffect } from "react";
import { ArchiveContext } from '../contexts/archive-context';
import useLocation from '../hooks/use-location';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import FetchPost from '../shared/fetch-post';
import { URLPath } from '../shared/url-path';
import SwitchButton from './switch-button';

interface ISidebarUpdate {
  tags: string,
  description: string,
  title: string,
  append: boolean,
  collections: boolean,
}

interface IDetailViewSidebarLabelEditProps {
  subPath: string;
  fileIndexItems: Array<IFileIndexItem>,
  isReadOnly: boolean
}

const ArchiveSidebarLabelEdit: React.FunctionComponent<IDetailViewSidebarLabelEditProps> = memo((archive) => {

  var history = useLocation();
  let { state, dispatch } = React.useContext(ArchiveContext);

  // show select info
  const [select, setSelect] = React.useState(new URLPath().getSelect(history.location.search));
  useEffect(() => {
    setSelect(new URLPath().getSelect(history.location.search));
  }, [history.location.search]);


  const [update, setUpdate] = React.useState({
    append: true,
    collections: true,
  } as ISidebarUpdate)

  const [updateReplace, setUpdateReplace] = React.useState({
    append: true,
    collections: true,
  } as ISidebarUpdate)

  const [isInputEnabled, setInputEnabled] = React.useState(false);


  function handleUpdateChange(event: React.ChangeEvent<HTMLDivElement> | React.KeyboardEvent<HTMLDivElement>) {
    let fieldValue = event.currentTarget.innerText.trim();
    let fieldName = event.currentTarget.dataset["name"];
    if (!fieldName) return;
    if (!fieldValue) return;

    var updateSidebar = updated(fieldName, fieldValue, update);
    setUpdate(updateSidebar);
    setInputEnabled(isFormUsed());
  }

  function handleUpdateReplaceChange(event: React.ChangeEvent<HTMLDivElement> | React.KeyboardEvent<HTMLDivElement>) {
    let fieldValue = event.currentTarget.innerText.trim();
    let fieldName = event.currentTarget.dataset["name"];
    if (!fieldName) return;
    if (!fieldValue) return;

    var updateSidebar = updated(fieldName, fieldValue, updateReplace);
    setUpdateReplace(updateSidebar);
  }

  /**
   * Cast Update Fields to ISidebarUpdate
   * @param fieldName e.g. tags
   * @param fieldValue the value
   * @param updateSidebar 'From Object'
   */
  function updated(fieldName: string, fieldValue: string, updateSidebar: ISidebarUpdate): ISidebarUpdate {
    if (!fieldName) return updateSidebar;
    if (!fieldValue) return updateSidebar;

    fieldValue = fieldValue.replace(/\n/g, "");
    switch (fieldName) {
      case "tags":
        updateSidebar.tags = fieldValue;
        break;
      case "description":
        updateSidebar.description = fieldValue;
        break;
      case "title":
        updateSidebar.title = fieldValue;
        break;
    }
    return updateSidebar;
  }

  function isFormUsed(): boolean {
    var totalChars = 0;
    if (update.tags) {
      totalChars += update.tags.length
    }
    if (update.description) {
      totalChars += update.description.length
    }
    if (update.title) {
      totalChars += update.title.length
    }
    return totalChars !== 0
  }

  /**
   * append=true&collections=true&tags=update
   * @param toUpdate 
   */
  function updateToUrlSearchParams(toUpdate: ISidebarUpdate): URLSearchParams {
    var bodyParams = new URLSearchParams();
    for (let key of Object.entries(toUpdate)) {
      if (key[1] && key[1].length >= 1) {
        bodyParams.set(key[0], key[1]);
      }
      if (key[1] === true || key[1] === false) {
        bodyParams.set(key[0], key[1]);
      }
    }
    return bodyParams;
  }

  /**
   * To update the archive
   * @param append to Add to the existing 
   */
  function pushUpdate(append: boolean) {
    update.append = append;

    var bodyParams = updateToUrlSearchParams(update);
    if (bodyParams.toString().length === 0) return;

    var subPaths = new URLPath().MergeSelectFileIndexItem(select, archive.fileIndexItems);
    if (!subPaths) return;
    var selectParams = new URLPath().ArrayToCommaSeperatedStringOneParent(subPaths, "")

    if (selectParams.length === 0) return;
    bodyParams.append("f", selectParams)

    FetchPost("/api/update", bodyParams.toString());

    dispatch({ type: 'update', ...update, select });
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
        var replace: string = (updateReplace as any)[key[0]] ? (updateReplace as any)[key[0]] : "";
        bodyParams.set("replace", replace);

        if (key[0] === "tags") {
          var regexer = new RegExp(key[1], "g")
          update.tags = update.tags.replace(regexer, replace)
        }

        FetchPost("/api/replace", bodyParams.toString())
        dispatch({ type: 'update', ...update, select });
      }
    }
  }

  const [isReplaceMode, setReplaceMode] = React.useState(false)

  return (
    <div className="content--text">
      <SwitchButton isEnabled={!archive.isReadOnly} leftLabel="Wijzigen" rightLabel="Vervangen" onToggle={(value) => setReplaceMode(value)}></SwitchButton>

      {!isReplaceMode ?
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

        </> : null}

      {isReplaceMode && !localStorage.getItem('beta_replace') ? <>
        <h4>Work in progress</h4>
      </> : null}
      {}

      {isReplaceMode && localStorage.getItem('beta_replace') ?
        <>
          <h4>Tags:</h4>
          <div data-name="tags"
            onInput={handleUpdateChange}
            suppressContentEditableWarning={true}
            contentEditable={!archive.isReadOnly && select.length !== 0}
            className={!archive.isReadOnly && select.length !== 0 ? "form-control form-control--half inline-block" : "form-control form-control--half inline-block disabled"}>
            &nbsp;
          </div>
          <div data-name="tags"
            onInput={handleUpdateReplaceChange}
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
            onInput={handleUpdateReplaceChange}
            data-name="description"
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
          <div data-name="title"
            onInput={handleUpdateReplaceChange}
            suppressContentEditableWarning={true}
            contentEditable={!archive.isReadOnly && select.length !== 0}
            className={!archive.isReadOnly && select.length !== 0 ? "form-control form-control--half inline-block" : "form-control form-control--half inline-block disabled"}>
            &nbsp;
          </div>

          {isInputEnabled && select.length !== 0 ?
            <button className="btn btn--default" onClick={() => pushSearchAndReplace()}>Vervangen</button> :
            <button disabled className="btn btn--default disabled" >Vervangen</button>}


        </> : null}

    </div >);
});
export default ArchiveSidebarLabelEdit