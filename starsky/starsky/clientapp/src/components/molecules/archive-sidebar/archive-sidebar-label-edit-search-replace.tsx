import React, { useEffect, useState } from "react";
import { ArchiveContext } from "../../../contexts/archive-context";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useLocation from "../../../hooks/use-location/use-location";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { PageType } from "../../../interfaces/IDetailView";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import { ISidebarGenericUpdate, ISidebarUpdate } from "../../../interfaces/ISidebarUpdate";
import localization from "../../../localization/localization.json";
import { CastToInterface } from "../../../shared/cast-to-interface";
import FetchPost from "../../../shared/fetch/fetch-post";
import { Language } from "../../../shared/language";
import { ClearSearchCache } from "../../../shared/search/clear-search-cache";
import { SidebarUpdate } from "../../../shared/sidebar-update";
import { URLPath } from "../../../shared/url/url-path";
import { UrlQuery } from "../../../shared/url/url-query";
import FormControl from "../../atoms/form-control/form-control";
import Notification, { NotificationType } from "../../atoms/notification/notification";
import Preloader from "../../atoms/preloader/preloader";

const ArchiveSidebarLabelEditSearchReplace: React.FunctionComponent = () => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageTagsWithColon = language.key(localization.MessageTagsWithColon);
  const MessageInfoWithColon = language.key(localization.MessageInfoWithColon);
  const MessageTitleWithColon = language.key(localization.MessageTitleWithColon);
  const MessageSearchAndReplaceNameLong = language.key(
    localization.MessageSearchAndReplaceNameLong
  );
  const MessageWriteErrorReadOnly = language.key(localization.MessageWriteErrorReadOnly);
  const MessageErrorGenericFail = language.key(localization.MessageErrorGenericFail);
  const MessageErrorNotFoundSourceMissingRunSync = language.key(
    localization.MessageErrorNotFoundSourceMissingRunSync
  );

  const history = useLocation();
  // eslint-disable-next-line prefer-const
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
  const [inputEnabled, setInputEnabled] = React.useState(false);

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

  const Capitalize = (s: string) => {
    return s.charAt(0).toUpperCase() + s.slice(1);
  };

  /**
   * To search and replace
   */
  function prepareBodyParams(selectPaths: string) {
    const bodyParams = new URLSearchParams();
    bodyParams.append("f", selectPaths);
    bodyParams.append(
      "collections",
      state.pageType !== PageType.Search
        ? (new URLPath().StringToIUrl(history.location.search).collections !== false).toString()
        : "false"
    );
    return bodyParams;
  }

  function handleFetchPostResponse(anyData: IConnectionDefault) {
    const result = new CastToInterface().InfoFileIndexArray(anyData.data as IFileIndexItem[]);
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
    // undo error message when success
    if (isError === MessageErrorGenericFail) {
      setIsError("");
    }

    ClearSearchCache(history.location.search);
  }

  function handleFetchPostError() {
    setIsError(MessageErrorGenericFail);
    // loading + update button
    setIsLoading(false);
    setInputEnabled(true);
  }

  function pushSearchAndReplace() {
    // loading + update button
    setIsLoading(true);
    setInputEnabled(false);

    update.append = false;
    const subPaths = new URLPath().MergeSelectFileIndexItem(select, state.fileIndexItems);
    if (!subPaths) return;
    const selectPaths = new URLPath().ArrayToCommaSeparatedStringOneParent(subPaths, "");

    if (selectPaths.length === 0) return;

    const bodyParams = prepareBodyParams(selectPaths);

    for (const key of Object.entries(update)) {
      const fieldName = key[0];
      const fieldValue = key[1];

      if (fieldName && !fieldName.startsWith("replace") && fieldValue.length >= 1) {
        bodyParams.set("fieldName", fieldName);
        bodyParams.set("search", fieldValue);

        const replaceFieldName = "replace" + Capitalize(fieldName);
        const replaceAnyValue = (update as unknown as ISidebarGenericUpdate)[replaceFieldName];
        const replaceValue: string = replaceAnyValue ?? "";

        bodyParams.set("replace", replaceValue);

        FetchPost(new UrlQuery().UrlReplaceApi(), bodyParams.toString())
          .then(handleFetchPostResponse)
          .catch(handleFetchPostError);
      }
    }
  }

  return (
    <>
      {isError !== "" ? (
        <Notification callback={() => setIsError("")} type={NotificationType.danger}>
          {isError}
        </Notification>
      ) : null}

      {isLoading ? <Preloader isWhite={false} isOverlay={false} /> : ""}

      <h4>{MessageTagsWithColon}</h4>
      <FormControl
        spellcheck={true}
        onInput={handleUpdateChange}
        name="tags"
        className="form-control--half inline-block"
        contentEditable={!state.isReadOnly && select.length !== 0}
      >
        &nbsp;
      </FormControl>
      <span className="arrow-to"></span>
      <FormControl
        spellcheck={true}
        onInput={handleUpdateChange}
        name="replace-tags"
        className="form-control--half inline-block"
        contentEditable={!state.isReadOnly && select.length !== 0}
      >
        &nbsp;
      </FormControl>

      <h4>{MessageInfoWithColon}</h4>
      <FormControl
        spellcheck={true}
        onInput={handleUpdateChange}
        name="description"
        className="form-control--half inline-block"
        contentEditable={!state.isReadOnly && select.length !== 0}
      >
        &nbsp;
      </FormControl>
      <span className="arrow-to"></span>
      <FormControl
        spellcheck={true}
        onInput={handleUpdateChange}
        name="replace-description"
        className="form-control--half inline-block"
        contentEditable={!state.isReadOnly && select.length !== 0}
      >
        &nbsp;
      </FormControl>

      <h4>{MessageTitleWithColon}</h4>
      <FormControl
        spellcheck={true}
        onInput={handleUpdateChange}
        name="title"
        className="form-control--half inline-block"
        contentEditable={!state.isReadOnly && select.length !== 0}
      >
        &nbsp;
      </FormControl>
      <span className="arrow-to"></span>
      <FormControl
        spellcheck={true}
        onInput={handleUpdateChange}
        name="replace-title"
        className="form-control--half inline-block"
        contentEditable={!state.isReadOnly && select.length !== 0}
      >
        &nbsp;
      </FormControl>

      {inputEnabled && select.length !== 0 ? (
        <button
          className="btn btn--default"
          data-test="replace-button"
          onClick={() => pushSearchAndReplace()}
        >
          {MessageSearchAndReplaceNameLong}
        </button>
      ) : (
        <button disabled className="btn btn--default disabled">
          {MessageSearchAndReplaceNameLong}
        </button>
      )}
    </>
  );
};
export default ArchiveSidebarLabelEditSearchReplace;
