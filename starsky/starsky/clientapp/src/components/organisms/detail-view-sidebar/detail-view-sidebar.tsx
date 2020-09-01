import { Link } from '@reach/router';
import React, { memo, useEffect, useRef } from "react";
import { useDetailViewContext } from '../../../contexts/detailview-context';
import useFetch from '../../../hooks/use-fetch';
import useGlobalSettings from '../../../hooks/use-global-settings';
import useKeyboardEvent from '../../../hooks/use-keyboard-event';
import useLocation from '../../../hooks/use-location';
import { IExifStatus } from '../../../interfaces/IExifStatus';
import { IFileIndexItem } from '../../../interfaces/IFileIndexItem';
import AspectRatio from '../../../shared/aspect-ratio';
import { CastToInterface } from '../../../shared/cast-to-interface';
import { isValidDate, parseDate, parseRelativeDate, parseTime } from '../../../shared/date';
import FetchPost from '../../../shared/fetch-post';
import { ClearFileListCache } from '../../../shared/filelist-cache';
import { Keyboard } from '../../../shared/keyboard';
import { Language } from '../../../shared/language';
import { URLPath } from '../../../shared/url-path';
import { UrlQuery } from '../../../shared/url-query';
import FormControl from '../../atoms/form-control/form-control';
import ColorClassSelect from '../../molecules/color-class-select/color-class-select';
import ModalDatetime from '../modal-edit-date-time/modal-edit-datetime';

interface IDetailViewSidebarProps {
  filePath: string,
  status: IExifStatus
}

const DetailViewSidebar: React.FunctionComponent<IDetailViewSidebarProps> = memo((props) => {

  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageTitleName = language.text("Titel", "Title");
  const MessageInfoName = "Info";
  const MessageColorClassification = language.text("Kleur-Classificatie", "Color Classification")
  const MessageDateTimeAgoEdited = language.text("geleden bewerkt", "ago edited");
  const MessageDateLessThan1Minute = language.text("minder dan één minuut", "less than one minute");
  const MessageDateMinutes = language.text("minuten", "minutes");
  const MessageDateHour = language.text("uur", "hour");
  const MessageNounUnknown = language.text("Onbekende", "Unknown");
  const MessageLocation = language.text("locatie", "location");
  const MessageReadOnlyFile = language.text("Alleen lezen bestand", "Read only file");
  const MessageNotFoundSourceMissing = language.text("Mist in de index", "Misses in the index");
  const MessageServerError = language.text("Er is iets mis met de input", "Something is wrong with the input");
  const MessageDeleted = language.text("Staat in de prullenmand", "Is in the trash");
  const MessageDeletedRestoreInstruction = language.text("'Zet terug uit prullenmand' om het item te bewerken", "'Restore from Trash' to edit the item");
  const MessageCreationDate = language.text("Aanmaakdatum", "Creation date");
  const MessageCreationDateUnknownTime = language.text("is op een onbekend moment", "is at an unknown time");

  let { state, dispatch } = useDetailViewContext();
  var history = useLocation();

  const [fileIndexItem, setFileIndexItem] = React.useState(state ? state.fileIndexItem : { status: IExifStatus.ServerError } as IFileIndexItem);
  useEffect(() => {
    if (!state) return;
    setFileIndexItem(state.fileIndexItem);
  }, [state]);

  const [collections, setCollections] = React.useState([] as string[]);

  // To Get information from Info Api
  var location = new UrlQuery().UrlQueryInfoApi(props.filePath);
  const responseObject = useFetch(location, 'get');
  useEffect(() => {
    if (!responseObject.data) return;
    var infoFileIndexItem = new CastToInterface().InfoFileIndexArray(responseObject.data);
    updateCollections(infoFileIndexItem);
    dispatch({ 'type': 'update', ...infoFileIndexItem[0], lastEdited: '' })
  }, [dispatch, responseObject]);

  // use time from state and not the update api
  useEffect(() => {
    // there is a bug in the api
    dispatch({ 'type': 'update', lastEdited: fileIndexItem.lastEdited })
  }, [dispatch, fileIndexItem.lastEdited]);

  function updateCollections(infoFileIndexItem: IFileIndexItem[]) {
    var collectionsList: string[] = [];
    infoFileIndexItem.forEach(element => {
      collectionsList.push(element.filePath)
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

    // allow emthy requests
    var nullChar = "\0";
    if (!value) value = nullChar;

    // compare
    var fileIndexObject: any = fileIndexItem;

    if (!fileIndexObject[name] === undefined) return; //to update emthy start to first fill

    var currentString: string = fileIndexObject[name];
    if (value === currentString) return;

    var updateObject: any = { f: fileIndexItem.filePath };
    updateObject[name] = value.trim();

    var bodyParams = new URLPath().ObjectToSearchParams(updateObject).toString().replace(/%00/ig, nullChar);
    console.log(bodyParams);

    FetchPost(new UrlQuery().UrlUpdateApi(), bodyParams).then(item => {
      if (item.statusCode !== 200 || !item.data) return;

      var currentItem = item.data[0] as IFileIndexItem;
      currentItem.lastEdited = new Date().toISOString();

      setFileIndexItem(currentItem);
      dispatch({ 'type': 'update', ...currentItem });

      ClearFileListCache(history.location.search);
      // clear search cache
      var searchTag = new URLPath().StringToIUrl(history.location.search).t;
      if (!searchTag) return;
      FetchPost(new UrlQuery().UrlSearchRemoveCacheApi(), `t=${searchTag}`);
    });
  }

  // To fast go the tags field
  const tagsReference = useRef<HTMLDivElement>(null);
  useKeyboardEvent(/^([ti])$/, (event: KeyboardEvent) => {
    if (new Keyboard().isInForm(event)) return;
    event.preventDefault();
    var current = tagsReference.current as HTMLDivElement;
    new Keyboard().SetFocusOnEndField(current);
  }, [props]);

  const [isModalDatetimeOpen, setModalDatetimeOpen] = React.useState(false);

  // noinspection HtmlUnknownAttribute
  return (<div className="detailview-sidebar">
    {fileIndexItem.status === IExifStatus.Deleted || fileIndexItem.status === IExifStatus.ReadOnly
      || fileIndexItem.status === IExifStatus.NotFoundSourceMissing || fileIndexItem.status === IExifStatus.ServerError ? <>
        <div className="content--header">
          Status
        </div>
        <div className="content content--text">
          {fileIndexItem.status === IExifStatus.Deleted ? <><div className="warning-box">{MessageDeleted}</div>
            {MessageDeletedRestoreInstruction}</> : null}
          {fileIndexItem.status === IExifStatus.NotFoundSourceMissing ? <><div className="warning-box">{MessageNotFoundSourceMissing}</div> </> : null}
          {fileIndexItem.status === IExifStatus.ReadOnly ? <><div className="warning-box">{MessageReadOnlyFile}</div> </> : null}
          {fileIndexItem.status === IExifStatus.ServerError ? <><div className="warning-box">{MessageServerError}</div> </> : null}
        </div>
      </> : null}
    <div className="content--header">
      Tags
    </div>
    <div className="content--text">
      <FormControl onBlur={handleChange}
        name="tags"
        maxlength={1024}
        reference={tagsReference}
        contentEditable={isFormEnabled}>
        {fileIndexItem.tags}
      </FormControl>
    </div>

    <div className="content--header">
      {MessageInfoName} &amp; {MessageTitleName}
    </div>
    <div className="content--text">
      <h4>{MessageInfoName}</h4>
      <FormControl onBlur={handleChange}
        maxlength={1024}
        name="description"
        contentEditable={isFormEnabled}>
        {fileIndexItem.description}
      </FormControl>
      <h4>{MessageTitleName}</h4>
      <FormControl onBlur={handleChange}
        name="title"
        maxlength={1024}
        contentEditable={isFormEnabled}>
        {fileIndexItem.title}
      </FormControl>
    </div>

    <div className="content--header">
      {MessageColorClassification}
    </div>
    <div className="content--text">
      <ColorClassSelect
        collections={new URLPath().StringToIUrl(history.location.search).collections !== false}
        onToggle={() => {
          setFileIndexItem({ ...fileIndexItem, lastEdited: new Date().toString() });
          dispatch({ 'type': 'update', lastEdited: '' });
          ClearFileListCache(history.location.search);
        }}
        filePath={fileIndexItem.filePath}
        currentColorClass={fileIndexItem.colorClass}
        isEnabled={isFormEnabled} />
    </div>

    {fileIndexItem.latitude || fileIndexItem.longitude || isValidDate(fileIndexItem.dateTime) || isValidDate(fileIndexItem.lastEdited) ||
      fileIndexItem.make || fileIndexItem.model || fileIndexItem.aperture || fileIndexItem.focalLength ?
      <div className="content--header">
        Details
      </div> : null}

    {/* when the image is created */}
    {isModalDatetimeOpen ? <ModalDatetime
      subPath={fileIndexItem.filePath}
      dateTime={fileIndexItem.dateTime}
      handleExit={(result) => {
        setModalDatetimeOpen(false);
        if (!result || !result[0]) return;
        setFileIndexItem(result[0]);
        dispatch({ 'type': 'update', ...result[0], lastEdited: '' })
      }} isOpen={true} /> : null}

    <div className="content--text">

      <button className="box" disabled={!isFormEnabled} data-test="dateTime" onClick={() => setModalDatetimeOpen(true)}>
        {isFormEnabled ? <div className="icon icon--right icon--edit" /> : null}
        <div className="icon icon--date" />
        {isValidDate(fileIndexItem.dateTime) ? <>
          <b>{parseDate(fileIndexItem.dateTime, settings.language)}</b>
          <p>{parseTime(fileIndexItem.dateTime)}</p></> : null}
        {!isValidDate(fileIndexItem.dateTime) ? <>
          <b>{MessageCreationDate}</b>
          <p>{MessageCreationDateUnknownTime}</p>
        </> : null}
      </button>

      {isValidDate(fileIndexItem.lastEdited) ?
        <div className="box" data-test="lastEdited">
          <div className="icon icon--last-edited"></div>
          <b>{
            language.token(parseRelativeDate(
              fileIndexItem.lastEdited, settings.language),
              ["{lessThan1Minute}", "{minutes}", "{hour}"],
              [MessageDateLessThan1Minute, MessageDateMinutes, MessageDateHour])
          }</b>
          <p>{MessageDateTimeAgoEdited}</p>
        </div> : ""}

      {fileIndexItem.make && fileIndexItem.model && fileIndexItem.aperture && fileIndexItem.focalLength ?
        <div className="box">
          <div className="icon icon--shutter-speed" />
          <b>
            <span data-test="make">{fileIndexItem.make}&nbsp;</span>
            <span data-test="model">{fileIndexItem.model}</span>
          </b>
          <p>
            f/<span data-test="aperture">{fileIndexItem.aperture}</span>&nbsp;&nbsp;&nbsp;
            {fileIndexItem.shutterSpeed} sec&nbsp;&nbsp;&nbsp;
            <span data-test="focalLength">{fileIndexItem.focalLength.toFixed(1)}</span> mm&nbsp;&nbsp;&nbsp;
            {fileIndexItem.isoSpeed !== 0 ? <>ISO {fileIndexItem.isoSpeed}</> : null}
          </p>
        </div> : ""}

      {fileIndexItem.latitude && fileIndexItem.longitude ?
        <a className="box" target="_blank" rel="noopener noreferrer" href={"https://www.openstreetmap.org/?mlat=" +
          fileIndexItem.latitude + "&mlon=" + fileIndexItem.longitude + "#map=16/" +
          fileIndexItem.latitude + "/" + fileIndexItem.longitude}>
          <div className="icon icon--location" />
          {fileIndexItem.locationCity && fileIndexItem.locationCountry ?
            <>
              <b>{fileIndexItem.locationCity}</b>
              <p>{fileIndexItem.locationCountry}</p>
            </> : <>
              <b>{MessageNounUnknown}</b>
              <p>{MessageLocation}</p>
            </>}
        </a> : ""}

      {collections.map((item, index) => (
        <Link to={new UrlQuery().updateFilePathHash(history.location.search, item)}
          key={index} className={index !== 1 ? "box" : "box box--child"} data-test="collections">
          {index !== 1 ? <div className="icon icon--photo" /> : null}
          <b>{new URLPath().getChild(item)}</b>
          <p>
            {index === 1 ? <>In een collectie:</> : null} {index + 1} van {collections.length}.
            {item === fileIndexItem.filePath && fileIndexItem.imageWidth !== 0 && fileIndexItem.imageHeight !== 0 ?
              <span>&nbsp;&nbsp;{fileIndexItem.imageWidth}&times;{fileIndexItem.imageHeight} pixels&nbsp;&nbsp;{
                new AspectRatio().ratio(fileIndexItem.imageWidth, fileIndexItem.imageHeight) ? <>
                  ratio: {new AspectRatio().ratio(fileIndexItem.imageWidth, fileIndexItem.imageHeight)}
                </> : null
              }
              </span>
              : null}
          </p>
        </Link>
      ))}

    </div>

  </div>);
});
export default DetailViewSidebar
