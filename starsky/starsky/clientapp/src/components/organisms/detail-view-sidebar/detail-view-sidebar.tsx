import { Link } from "@reach/router";
import React, { memo, useEffect, useRef } from "react";
import { DetailViewAction } from "../../../contexts/detailview-context";
import useFetch from "../../../hooks/use-fetch";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useKeyboardEvent from "../../../hooks/use-keyboard/use-keyboard-event";
import useLocation from "../../../hooks/use-location";
import { IDetailView } from "../../../interfaces/IDetailView";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import { AsciiNull } from "../../../shared/ascii-null";
import AspectRatio from "../../../shared/aspect-ratio";
import BytesFormat from "../../../shared/bytes-format";
import { CastToInterface } from "../../../shared/cast-to-interface";
import { ClipboardHelper } from "../../../shared/clipboard-helper";
import { isValidDate, parseRelativeDate } from "../../../shared/date";
import { FileListCache } from "../../../shared/filelist-cache";
import { Keyboard } from "../../../shared/keyboard";
import { Language } from "../../../shared/language";
import { ClearSearchCache } from "../../../shared/search/clear-search-cache";
import { URLPath } from "../../../shared/url-path";
import { UrlQuery } from "../../../shared/url-query";
import DetailViewExifStatus from "../../atoms/detailview-exifstatus/detailview-exifstatus";
import DetailViewInfoMakeModelAperture from "../../atoms/detailview-info-make-model-aperture/detailview-info-make-model-aperture";
import FormControl from "../../atoms/form-control/form-control";
import Notification from "../../atoms/notification/notification";
import ColorClassSelect from "../../molecules/color-class-select/color-class-select";
import DetailViewInfoDateTime from "../detailview-info-datetime/detailview-info-datetime";
import DetailViewInfoLocation from "../detailview-info-location/detailview-info-location";
import { UpdateChange } from "./update-change";

interface IDetailViewSidebarProps {
  filePath: string;
  status: IExifStatus;
  state: IDetailView;
  dispatch: React.Dispatch<DetailViewAction>;
}

const DetailViewSidebar: React.FunctionComponent<IDetailViewSidebarProps> =
  memo(({ state, dispatch, ...props }) => {
    // content
    const settings = useGlobalSettings();
    const language = new Language(settings.language);
    const MessageTitleName = language.text("Titel", "Title");
    const MessageInfoName = "Info";
    const MessageColorClassification = language.text(
      "Kleur-Classificatie",
      "Color Classification"
    );
    const MessageDateTimeAgoEdited = language.text(
      "geleden bewerkt",
      "ago edited"
    );
    const MessageDateLessThan1Minute = language.text(
      "minder dan één minuut",
      "less than one minute"
    );
    const MessageDateMinutes = language.text("minuten", "minutes");
    const MessageDateHour = language.text("uur", "hour");

    const MessageCopiedLabels = language.text(
      "De labels zijn gekopieerd",
      "The labels have been copied"
    );
    const MessagePasteLabels = language.text(
      "De labels zijn overschreven",
      "The labels have been overwritten"
    );

    const history = useLocation();

    const [fileIndexItem, setFileIndexItem] = React.useState(
      state
        ? state.fileIndexItem
        : ({
            filePath: "/",
            status: IExifStatus.ServerError,
            lastEdited: ""
          } as IFileIndexItem)
    );

    useEffect(() => {
      if (!state) return;
      setFileIndexItem(state.fileIndexItem);
    }, [state]);

    const [collections, setCollections] = React.useState([] as string[]);

    // To Get information from Info Api
    const location = new UrlQuery().UrlQueryInfoApi(props.filePath);
    const infoResponseObject = useFetch(location, "get");

    useEffect(() => {
      if (!infoResponseObject.data) return;
      const infoFileIndexItem = new CastToInterface().InfoFileIndexArray(
        infoResponseObject.data
      );
      if (!infoFileIndexItem) return;
      updateCollections(infoFileIndexItem);

      dispatch({
        type: "update",
        ...infoFileIndexItem[0],
        lastEdited: "",
        filePath: infoFileIndexItem[0].filePath
      });
    }, [dispatch, infoResponseObject]);

    function updateCollections(infoFileIndexItem: IFileIndexItem[]) {
      const collectionsList: string[] = [];
      infoFileIndexItem.forEach((element) => {
        collectionsList.push(element.filePath);
      });
      setCollections(collectionsList);
    }
    // For the display
    const [isFormEnabled, setFormEnabled] = React.useState(true);
    useEffect(() => {
      if (!fileIndexItem.status) return;
      switch (fileIndexItem.status) {
        case IExifStatus.Deleted:
        case IExifStatus.ReadOnly:
        case IExifStatus.ServerError:
        case IExifStatus.NotFoundSourceMissing:
          setFormEnabled(false);
          break;
        default:
          setFormEnabled(true);
          break;
      }
    }, [fileIndexItem.status]);

    function handleChange(event: React.ChangeEvent<HTMLDivElement>) {
      let value = event.currentTarget.textContent;
      let name = event.currentTarget.dataset["name"];

      if (!name) return;
      if (!value) value = AsciiNull();
      new UpdateChange(
        fileIndexItem,
        setFileIndexItem,
        dispatch,
        history,
        state
      ).Update([[name, value]]);
    }

    const descriptionReference = useRef<HTMLDivElement>(null);
    const titleReference = useRef<HTMLDivElement>(null);
    const [copyPasteAction, setCopyPasteAction] = React.useState("");

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
      [props]
    );

    useKeyboardEvent(
      /^c$/,
      (event: KeyboardEvent) => {
        if (new Keyboard().isInForm(event)) return;
        event.preventDefault();
        const copy = new ClipboardHelper().Copy(
          tagsReference,
          descriptionReference,
          titleReference
        );
        if (!copy) return;
        setCopyPasteAction(MessageCopiedLabels);
      },
      [props]
    );

    // next page the message should be gone
    useEffect(() => {
      setCopyPasteAction("");
    }, [fileIndexItem.filePath]);

    useKeyboardEvent(
      /^v$/,
      (event: KeyboardEvent) => {
        if (new Keyboard().isInForm(event)) return;
        event.preventDefault();

        const updateChange = new UpdateChange(
          fileIndexItem,
          setFileIndexItem,
          dispatch,
          history,
          state
        );

        const paste = new ClipboardHelper().PasteAsync(updateChange.Update);

        if (!paste) return;
        setCopyPasteAction(MessagePasteLabels);
      },
      [props]
    );

    if (!fileIndexItem) {
      return <>No status</>;
    }

    // noinspection HtmlUnknownAttribute
    return (
      <div className="detailview-sidebar" data-test="detailview-sidebar">
        {copyPasteAction ? (
          <Notification callback={() => setCopyPasteAction("")}>
            {copyPasteAction}
          </Notification>
        ) : null}
        <DetailViewExifStatus status={fileIndexItem.status} />
        <div className="content--header">Tags</div>
        <div className="content--text">
          <FormControl
            onBlur={handleChange}
            name="tags"
            data-test="detailview-sidebar-tags"
            maxlength={1024}
            reference={tagsReference}
            contentEditable={isFormEnabled}
          >
            {fileIndexItem.tags}
          </FormControl>
        </div>
        <div className="content--header">
          {/* Info & Title */}
          {MessageInfoName} &amp; {MessageTitleName}
        </div>
        <div className="content--text">
          <h4>{MessageInfoName}</h4>
          <FormControl
            onBlur={handleChange}
            maxlength={1024}
            name="description"
            reference={descriptionReference}
            contentEditable={isFormEnabled}
          >
            {fileIndexItem.description}
          </FormControl>
          <h4>{MessageTitleName}</h4>
          <FormControl
            onBlur={handleChange}
            name="title"
            maxlength={1024}
            reference={titleReference}
            contentEditable={isFormEnabled}
          >
            {fileIndexItem.title}
          </FormControl>
        </div>
        <div className="content--header">{MessageColorClassification}</div>
        <div className="content--text">
          <ColorClassSelect
            collections={
              new URLPath().StringToIUrl(history.location.search)
                .collections !== false
            }
            onToggle={(result) => {
              setFileIndexItem({
                ...fileIndexItem,
                lastEdited: new Date().toString(),
                colorClass: result
              });
              dispatch({
                type: "update",
                filePath: fileIndexItem.filePath,
                lastEdited: new Date().toString(),
                colorclass: result
              });
              ClearSearchCache(history.location.search);
              new FileListCache().CacheCleanEverything();
            }}
            filePath={fileIndexItem.filePath}
            currentColorClass={fileIndexItem.colorClass}
            isEnabled={isFormEnabled}
          />
        </div>
        {fileIndexItem.latitude ||
        fileIndexItem.longitude ||
        isValidDate(fileIndexItem.dateTime) ||
        isValidDate(fileIndexItem.lastEdited) ||
        fileIndexItem.make ||
        fileIndexItem.model ||
        fileIndexItem.aperture ||
        fileIndexItem.focalLength ? (
          <div className="content--header">Details</div>
        ) : null}

        <div className="content--text">
          <DetailViewInfoDateTime
            fileIndexItem={fileIndexItem}
            isFormEnabled={isFormEnabled}
            setFileIndexItem={setFileIndexItem}
            dispatch={dispatch}
          />

          {isValidDate(fileIndexItem.lastEdited) ? (
            <div className="box" data-test="lastEdited">
              <div className="icon icon--last-edited"></div>
              <b>
                {language.token(
                  parseRelativeDate(
                    fileIndexItem.lastEdited,
                    settings.language
                  ),
                  ["{lessThan1Minute}", "{minutes}", "{hour}"],
                  [
                    MessageDateLessThan1Minute,
                    MessageDateMinutes,
                    MessageDateHour
                  ]
                )}
              </b>
              <p>{MessageDateTimeAgoEdited}</p>
            </div>
          ) : (
            ""
          )}

          <DetailViewInfoMakeModelAperture fileIndexItem={fileIndexItem} />

          {fileIndexItem.lensModel ? (
            <div className="box">
              <div className="icon icon--lens" />
              <b>
                <span data-test="lens-model" title={fileIndexItem.lensModel}>
                  Lens model
                </span>
              </b>
              <p>{fileIndexItem.lensModel}</p>
            </div>
          ) : null}

          <DetailViewInfoLocation
            fileIndexItem={fileIndexItem}
            isFormEnabled={isFormEnabled}
            {...fileIndexItem}
            dispatch={dispatch}
            setFileIndexItem={setFileIndexItem}
          />

          {collections.map((item, index) => (
            // some senarios details is set off, this is linked from details
            <Link
              to={new UrlQuery().updateFilePathHash(
                history.location.search + "&details=true",
                item
              )}
              key={item}
              className={index !== 1 ? "box" : "box box--child"}
              data-test="collections"
            >
              {index !== 1 ? <div className="icon icon--photo" /> : null}
              <b>{new URLPath().getChild(item)}</b>
              <p>
                {index === 1 ? <>In een collectie:</> : null} {index + 1} van{" "}
                {collections.length}.
                {item === fileIndexItem.filePath &&
                fileIndexItem.imageWidth !== 0 &&
                fileIndexItem.imageHeight !== 0 ? (
                  <span>
                    &nbsp;&nbsp;{fileIndexItem.imageWidth}&times;
                    {fileIndexItem.imageHeight} pixels
                    {new AspectRatio().ratio(
                      fileIndexItem.imageWidth,
                      fileIndexItem.imageHeight
                    ) ? (
                      <>
                        &nbsp;&nbsp;ratio:{" "}
                        {new AspectRatio().ratio(
                          fileIndexItem.imageWidth,
                          fileIndexItem.imageHeight
                        )}
                      </>
                    ) : null}
                    {fileIndexItem.size ? (
                      <>&nbsp;&nbsp;{BytesFormat(fileIndexItem.size, 1)}</>
                    ) : null}
                  </span>
                ) : null}
              </p>
            </Link>
          ))}
        </div>
      </div>
    );
  });
export default DetailViewSidebar;
