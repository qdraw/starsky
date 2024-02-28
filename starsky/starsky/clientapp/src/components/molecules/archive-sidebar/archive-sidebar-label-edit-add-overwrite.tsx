import React, { useContext, useEffect, useRef, useState } from "react";
import { ArchiveContext } from "../../../contexts/archive-context";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useKeyboardEvent from "../../../hooks/use-keyboard/use-keyboard-event";
import useLocation from "../../../hooks/use-location/use-location";
import { PageType } from "../../../interfaces/IDetailView";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { ISidebarUpdate } from "../../../interfaces/ISidebarUpdate";
import localization from "../../../localization/localization.json";
import { CastToInterface } from "../../../shared/cast-to-interface";
import FetchPost from "../../../shared/fetch/fetch-post";
import { Keyboard } from "../../../shared/keyboard";
import { Language } from "../../../shared/language";
import { ClearSearchCache } from "../../../shared/search/clear-search-cache";
import { SidebarUpdate } from "../../../shared/sidebar-update";
import { URLPath } from "../../../shared/url/url-path";
import { UrlQuery } from "../../../shared/url/url-query";
import FormControl from "../../atoms/form-control/form-control";
import Notification, { NotificationType } from "../../atoms/notification/notification";
import Preloader from "../../atoms/preloader/preloader";

const ArchiveSidebarLabelEditAddOverwrite: React.FunctionComponent = () => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageAddName = language.key(localization.MessageAddName);
  const MessageOverwriteName = language.key(localization.MessageOverwriteName);
  const MessageTitleName = language.key(localization.MessageTitleName);
  const MessageWriteErrorReadOnly = language.key(localization.MessageWriteErrorReadOnly);
  const MessageErrorGenericFail = language.key(localization.MessageErrorGenericFail);
  const MessageErrorNotFoundSourceMissingRunSync = language.key(
    localization.MessageErrorNotFoundSourceMissingRunSync
  );

  const history = useLocation();
  // eslint-disable-next-line prefer-const
  let { state, dispatch } = useContext(ArchiveContext);

  // state without any context
  state = new CastToInterface().UndefinedIArchiveReadonly(state);

  // show select info
  const [select, setSelect] = useState(new URLPath().getSelect(history.location.search));
  useEffect(() => {
    setSelect(new URLPath().getSelect(history.location.search));
  }, [history.location.search]);

  // The Updated that is send to the api
  const [update, setUpdate] = useState({
    append: true,
    collections: true
  } as ISidebarUpdate);

  // Add/Hide disabled state
  const [inputEnabled, setInputEnabled] = useState(false);

  // preloading icon
  const [isLoading, setIsLoading] = useState(false);

  // for showing a notification
  const [isError, setIsError] = useState("");

  // Update the disabled state + Local variable with input data
  function handleUpdateChange(
    event: React.ChangeEvent<HTMLDivElement> | React.KeyboardEvent<HTMLDivElement>
  ) {
    const sideBarUpdate = new SidebarUpdate().Change(event, update);
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
        ? new URLPath().StringToIUrl(history.location.search).collections !== false
        : false;

    const bodyParams = new URLPath().ObjectToSearchParams(update);
    if (bodyParams.toString().length === 0) return;

    const subPaths = new URLPath().MergeSelectFileIndexItem(select, state.fileIndexItems);
    if (!subPaths) return;
    const selectParams = new URLPath().ArrayToCommaSeparatedStringOneParent(subPaths, "");

    if (selectParams.length === 0) return;
    bodyParams.append("f", selectParams);

    FetchPost(new UrlQuery().UrlUpdateApi(), bodyParams.toString())
      .then((anyData) => {
        const result = new CastToInterface().InfoFileIndexArray(anyData.data);
        result.forEach((element) => {
          if (element.status === IExifStatus.ReadOnly) setIsError(MessageWriteErrorReadOnly);
          if (element.status === IExifStatus.NotFoundSourceMissing)
            setIsError(MessageErrorNotFoundSourceMissingRunSync);
          if (element.status === IExifStatus.Ok || element.status === IExifStatus.Deleted) {
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
        // undo error message when success
        if (isError === MessageErrorGenericFail) {
          setIsError("");
        }
      })
      .catch(() => {
        setIsError(MessageErrorGenericFail);
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
      const current = tagsReference.current as HTMLDivElement;
      new Keyboard().SetFocusOnEndField(current);
    },
    []
  );

  // noinspection HtmlUnknownAttribute
  return (
    <>
      {isError !== "" ? (
        <Notification callback={() => setIsError("")} type={NotificationType.danger}>
          {isError}
        </Notification>
      ) : null}

      {isLoading ? <Preloader isWhite={false} isOverlay={false} /> : ""}

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

      {inputEnabled && select.length !== 0 ? (
        <button className="btn btn--info" data-test="overwrite" onClick={() => pushUpdate(false)}>
          Overschrijven
        </button>
      ) : (
        <button disabled className="btn btn--info disabled">
          {MessageOverwriteName}
        </button>
      )}
      {inputEnabled && select.length !== 0 ? (
        <button data-test="add" className="btn btn--default" onClick={() => pushUpdate(true)}>
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
