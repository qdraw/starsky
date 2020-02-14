import React, { useEffect, useState } from "react";
import { ArchiveContext } from '../contexts/archive-context';
import useGlobalSettings from '../hooks/use-global-settings';
import useLocation from '../hooks/use-location';
import { IExifStatus } from '../interfaces/IExifStatus';
import { ISidebarUpdate } from '../interfaces/ISidebarUpdate';
import { CastToInterface } from '../shared/cast-to-interface';
import FetchPost from '../shared/fetch-post';
import { Language } from '../shared/language';
import { SidebarUpdate } from '../shared/sidebar-update';
import { URLPath } from '../shared/url-path';
import FormControl from './form-control';
import Preloader from './preloader';


const ArchiveSidebarLabelEditSearchReplace: React.FunctionComponent = () => {

  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageSearchAndReplaceName = language.text("Zoeken en vervangen", "Search and replace");
  const MessageTitleName = language.text("Titel", "Title");

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
  const [update, setUpdate] = React.useState({} as ISidebarUpdate);

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

  const Capitalize = (s: string) => {
    return s.charAt(0).toUpperCase() + s.slice(1)
  };

  /**
   * To search and replace
   */
  function pushSearchAndReplace() {

    // loading + update button
    setIsLoading(true);
    setInputEnabled(false);

    update.append = false;
    var subPaths = new URLPath().MergeSelectFileIndexItem(select, state.fileIndexItems);
    if (!subPaths) return;
    var selectPaths = new URLPath().ArrayToCommaSeperatedStringOneParent(subPaths, "");

    if (selectPaths.length === 0) return;

    var bodyParams = new URLSearchParams();
    bodyParams.append("f", selectPaths);

    for (let key of Object.entries(update)) {
      var fieldName = key[0];
      var fieldValue = key[1];

      if (fieldName && !fieldName.startsWith("replace") && fieldValue.length >= 1) {
        bodyParams.set("fieldName", fieldName);
        bodyParams.set("search", fieldValue);

        var replaceFieldName = "replace" + Capitalize(fieldName);
        var replaceAnyValue = (update as any)[replaceFieldName];
        var replaceValue: string = replaceAnyValue ? replaceAnyValue : "";

        bodyParams.set("replace", replaceValue);

        FetchPost("/api/replace", bodyParams.toString()).then((anyData) => {
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
    }
  }

  // noinspection HtmlUnknownAttribute
  return (
    <>
      {isLoading ? <Preloader isDetailMenu={false} isOverlay={false} /> : ""}

      <h4>Tags:</h4>
      <FormControl onInput={handleUpdateChange} name="tags" className="form-control--half inline-block"
        contentEditable={!state.isReadOnly && select.length !== 0}>
        &nbsp;
      </FormControl>
      <span className="arrow-to"></span>
      <FormControl onInput={handleUpdateChange} name="replace-tags" className="form-control--half inline-block"
        contentEditable={!state.isReadOnly && select.length !== 0}>
        &nbsp;
      </FormControl>

      <h4>Info:</h4>
      <FormControl onInput={handleUpdateChange} name="description" className="form-control--half inline-block"
        contentEditable={!state.isReadOnly && select.length !== 0}>
        &nbsp;
      </FormControl>
      <span className="arrow-to"></span>
      <FormControl onInput={handleUpdateChange} name="replace-description" className="form-control--half inline-block"
        contentEditable={!state.isReadOnly && select.length !== 0}>
        &nbsp;
      </FormControl>

      <h4>{MessageTitleName}:</h4>
      <FormControl onInput={handleUpdateChange} name="title" className="form-control--half inline-block"
        contentEditable={!state.isReadOnly && select.length !== 0}>
        &nbsp;
      </FormControl>
      <span className="arrow-to"></span>
      <FormControl onInput={handleUpdateChange} name="replace-title" className="form-control--half inline-block"
        contentEditable={!state.isReadOnly && select.length !== 0}>
        &nbsp;
      </FormControl>

      {isInputEnabled && select.length !== 0 ?
        <button className="btn btn--default" onClick={() => pushSearchAndReplace()}>{MessageSearchAndReplaceName}</button> :
        <button disabled className="btn btn--default disabled">{MessageSearchAndReplaceName}</button>}
    </>
  );
};
export default ArchiveSidebarLabelEditSearchReplace
