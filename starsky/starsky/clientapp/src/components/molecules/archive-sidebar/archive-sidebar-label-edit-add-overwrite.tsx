import React, { useEffect, useRef, useState } from "react";
import { ArchiveContext } from "../../../contexts/archive-context";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useKeyboardEvent from "../../../hooks/use-keyboard/use-keyboard-event";
import useLocation from "../../../hooks/use-location";
import { PageType } from "../../../interfaces/IDetailView";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { ISidebarUpdate } from "../../../interfaces/ISidebarUpdate";
import { CastToInterface } from "../../../shared/cast-to-interface";
import FetchPost from "../../../shared/fetch-post";
import { Keyboard } from "../../../shared/keyboard";
import { Language } from "../../../shared/language";
import { ClearSearchCache } from "../../../shared/search/clear-search-cache";
import { SidebarUpdate } from "../../../shared/sidebar-update";
import { URLPath } from "../../../shared/url-path";
import { UrlQuery } from "../../../shared/url-query";
import FormControl from "../../atoms/form-control/form-control";
import Notification, {
  NotificationType
} from "../../atoms/notification/notification";
import Preloader from "../../atoms/preloader/preloader";

const ArchiveSidebarLabelEditAddOverwrite: React.FunctionComponent = () => {
  const settings = useGlobalSettings();
  const MessageAddName = new Language(settings.language).text(
    "Toevoegen",
    "Add to"
  );
  const MessageOverwriteName = new Language(settings.language).text(
    "Overschrijven",
    "Overwrite"
  );
  const MessageTitleName = new Language(settings.language).text(
    "Titel",
    "Title"
  );
  const MessageErrorReadOnly = new Language(settings.language).text(
    "Eén of meerdere bestanden zijn alleen lezen. " +
      "Alleen de bestanden met schrijfrechten zijn geupdate.",
    "One or more files are read only. " +
      "Only the files with write permissions have been updated."
  );
  const MessageErrorNotFoundSourceMissing = new Language(
    settings.language
  ).text(
    "Eén of meerdere bestanden zijn al verdwenen. " +
      "Alleen de bestanden die wel aanwezig zijn geupdate. Draai een handmatige sync",
    "One or more files are already gone. " +
      "Only the files that are present are updated. Run a manual sync"
  );

  var history = useLocation();
  let { state, dispatch } = React.useContext(ArchiveContext);

  // state without any context
  state = new CastToInterface().UndefinedIArchiveReadonly(state);

  // show select info
  const [select, setSelect] = React.useState(
    new URLPath().getSelect(history.location.search)
  );
  useEffect(() => {
    setSelect(new URLPath().getSelect(history.location.search));
  }, [history.location.search]);

  // The Updated that is send to the api
  const [update, setUpdate] = React.useState({
    append: true,
    collections: true
  } as ISidebarUpdate);

  // Add/Hide disabled state
  const [isInputEnabled, setInputEnabled] = React.useState(false);

  // preloading icon
  const [isLoading, setIsLoading] = useState(false);

  // for showing a notification
  const [isError, setIsError] = useState("");

  // Update the disabled state + Local variable with input data
  function handleUpdateChange(
    event:
      | React.ChangeEvent<HTMLDivElement>
      | React.KeyboardEvent<HTMLDivElement>
  ) {
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
    update.collections =
      state.pageType !== PageType.Search
        ? new URLPath().StringToIUrl(history.location.search).collections !==
          false
        : false;

    var bodyParams = new URLPath().ObjectToSearchParams(update);
    if (bodyParams.toString().length === 0) return;

    var subPaths = new URLPath().MergeSelectFileIndexItem(
      select,
      state.fileIndexItems
    );
    if (!subPaths) return;
    var selectParams = new URLPath().ArrayToCommaSeperatedStringOneParent(
      subPaths,
      ""
    );

    if (selectParams.length === 0) return;
    bodyParams.append("f", selectParams);

    FetchPost(new UrlQuery().UrlUpdateApi(), bodyParams.toString())
      .then((anyData) => {
        var result = new CastToInterface().InfoFileIndexArray(anyData.data);
        result.forEach((element) => {
          if (element.status === IExifStatus.ReadOnly)
            setIsError(MessageErrorReadOnly);
          if (element.status === IExifStatus.NotFoundSourceMissing)
            setIsError(MessageErrorNotFoundSourceMissing);
          if (
            element.status === IExifStatus.Ok ||
            element.status === IExifStatus.Deleted
          ) {
            dispatch({
              type: "update",
              ...element,
              select: [element.fileName]
            });
          }
        });

        // loading + update button
        setIsLoading(false);
        setInputEnabled(true);
        ClearSearchCache(history.location.search);
      })
      .catch(() => {
        // loading + update button
        setIsLoading(false);
        setInputEnabled(true);
      });
  }

  // To fast go the tags field
  const tagsReference = useRef<HTMLDivElement>(null);
  useKeyboardEvent(
    /^([ti])$/,
    (event: KeyboardEvent) => {
      if (new Keyboard().isInForm(event)) return;
      event.preventDefault();
      var current = tagsReference.current as HTMLDivElement;
      new Keyboard().SetFocusOnEndField(current);
    },
    []
  );

  // noinspection HtmlUnknownAttribute
  return (
    <>
      {isError !== "" ? (
        <Notification
          callback={() => setIsError("")}
          type={NotificationType.danger}
        >
          {isError}
        </Notification>
      ) : null}

      {isLoading ? <Preloader isDetailMenu={false} isOverlay={false} /> : ""}

      <h4>Tags:</h4>
      <FormControl
        spellcheck={true}
        reference={tagsReference}
        onInput={handleUpdateChange}
        name="tags"
        contentEditable={!state.isReadOnly && select.length !== 0}
      ></FormControl>

      <h4>Info:</h4>
      <FormControl
        spellcheck={true}
        onInput={handleUpdateChange}
        name="description"
        contentEditable={!state.isReadOnly && select.length !== 0}
      ></FormControl>

      <h4>{MessageTitleName}:</h4>
      <FormControl
        spellcheck={true}
        onInput={handleUpdateChange}
        name="title"
        contentEditable={!state.isReadOnly && select.length !== 0}
      ></FormControl>

      {isInputEnabled && select.length !== 0 ? (
        <button
          className="btn btn--info"
          data-test="overwrite"
          onClick={() => pushUpdate(false)}
        >
          Overschrijven
        </button>
      ) : (
        <button disabled className="btn btn--info disabled">
          {MessageOverwriteName}
        </button>
      )}
      {isInputEnabled && select.length !== 0 ? (
        <button
          data-test="add"
          className="btn btn--default"
          onClick={() => pushUpdate(true)}
        >
          {MessageAddName}
        </button>
      ) : (
        <button disabled className="btn btn--default disabled">
          {MessageAddName}
        </button>
      )}
    </>
  );
};
export default ArchiveSidebarLabelEditAddOverwrite;
