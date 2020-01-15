import React, { useEffect, useState } from "react";
import { ArchiveContext } from '../contexts/archive-context';
import useGlobalSettings from '../hooks/use-globalSettings';
import useLocation from '../hooks/use-location';
import { IExifStatus } from '../interfaces/IExifStatus';
import { ISidebarUpdate } from '../interfaces/ISidebarUpdate';
import { CastToInterface } from '../shared/cast-to-interface';
import FetchPost from '../shared/fetch-post';
import { Language } from '../shared/language';
import { SidebarUpdate } from '../shared/sidebar-update';
import { URLPath } from '../shared/url-path';
import Preloader from './preloader';

const ArchiveSidebarLabelEditAddOverwrite: React.FunctionComponent = () => {

  const settings = useGlobalSettings();
  const MessageAddName = new Language(settings.language).text("Toevoegen", "Add to");
  const MessageOverwriteName = new Language(settings.language).text("Overschrijven", "Overwrite");
  const MessageTitleName = new Language(settings.language).text("Titel", "Title");

  var history = useLocation();
  let { state, dispatch } = React.useContext(ArchiveContext);

  // state without any context
  state = new CastToInterface().UndefinedIArchiveReadonly(state);

  // show select info
  const [select, setSelect] = React.useState(new URLPath().getSelect(history.location.search));
  useEffect(() => {
    setSelect(new URLPath().getSelect(history.location.search));
  }, [history.location.search]);

  // The Updated that is send to the api
  const [update, setUpdate] = React.useState({
    append: true,
    collections: true,
  } as ISidebarUpdate);

  // Add/Hide disabled state
  const [isInputEnabled, setInputEnabled] = React.useState(false);

  // preloading icon
  const [isLoading, setIsLoading] = useState(false);

  // Update the disabled state + Local variable with input data
  function handleUpdateChange(event: React.ChangeEvent<HTMLDivElement> | React.KeyboardEvent<HTMLDivElement>) {
    var sideBarUpdate = new SidebarUpdate().Change(event, update);
    if (!sideBarUpdate) return;
    setUpdate(sideBarUpdate);
    setInputEnabled(new SidebarUpdate().IsFormUsed(update));
  }

  /**
   * To update the archive
   * @param append to Add to the existing 
   */
  function pushUpdate(append: boolean) {
    // loading + update button
    setIsLoading(true);
    setInputEnabled(false);

    update.append = append;

    var bodyParams = new URLPath().ObjectToSearchParams(update);
    if (bodyParams.toString().length === 0) return;

    var subPaths = new URLPath().MergeSelectFileIndexItem(select, state.fileIndexItems);
    if (!subPaths) return;
    var selectParams = new URLPath().ArrayToCommaSeperatedStringOneParent(subPaths, "");

    if (selectParams.length === 0) return;
    bodyParams.append("f", selectParams);

    FetchPost("/api/update", bodyParams.toString()).then((anyData) => {
      var result = new CastToInterface().InfoFileIndexArray(anyData.data);
      result.forEach(element => {
        if (element.status !== IExifStatus.Ok) return;
        dispatch({ type: 'update', ...element, select: [element.fileName] });
      });

      // loading + update button
      setIsLoading(false);
      setInputEnabled(true);

    }).catch(() => {
      // loading + update button
      setIsLoading(false);
      setInputEnabled(true);
    })
  }

  // noinspection HtmlUnknownAttribute
  return (
    <>
      {isLoading ? <Preloader isDetailMenu={false} isOverlay={false} /> : ""}

      <h4>Tags:</h4>
      <div data-name="tags"
        onInput={handleUpdateChange}
        suppressContentEditableWarning={true}
        contentEditable={!state.isReadOnly && select.length !== 0}
        className={!state.isReadOnly && select.length !== 0 ? "form-control" : "form-control disabled"}>
      </div>
      <h4>Info:</h4>
      <div
        onInput={handleUpdateChange}
        data-name="description"
        suppressContentEditableWarning={true}
        contentEditable={!state.isReadOnly && select.length !== 0}
        className={!state.isReadOnly && select.length !== 0 ? "form-control" : "form-control disabled"}>
      </div>
      <h4>{MessageTitleName}:</h4>
      <div data-name="title"
        onInput={handleUpdateChange}
        suppressContentEditableWarning={true}
        contentEditable={!state.isReadOnly && select.length !== 0}
        className={!state.isReadOnly && select.length !== 0 ? "form-control" : "form-control disabled"}>
      </div>

      {isInputEnabled && select.length !== 0 ? <button
        className="btn btn--info" data-test="overwrite"
        onClick={() => pushUpdate(false)}>Overschrijven</button> :
        <button disabled className="btn btn--info disabled" >{MessageOverwriteName}</button>}
      {isInputEnabled && select.length !== 0 ?
        <button data-test="add" className="btn btn--default" onClick={() => pushUpdate(true)}>{MessageAddName}</button> :
        <button disabled className="btn btn--default disabled" >{MessageAddName}</button>}
    </>
  );
};
export default ArchiveSidebarLabelEditAddOverwrite
