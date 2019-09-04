import React, { memo, useEffect, useRef } from "react";
import useFetch from '../hooks/use-fetch';
import useKeyboardEvent from '../hooks/use-keyboard-event';
import { IExifStatus } from '../interfaces/IExifStatus';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { CastToInterface } from '../shared/cast-to-interface';
import { isValidDate, parseDate, parseRelativeDate, parseTime } from '../shared/date';
import { Keyboard } from '../shared/keyboard';
import { Query } from '../shared/query';
import ColorClassSelect from './color-class-select';
interface IDetailViewSidebarProps {
  fileIndexItem: IFileIndexItem,
  filePath: string,
  status: IExifStatus
}

const DetailViewSidebar: React.FunctionComponent<IDetailViewSidebarProps> = memo((props) => {

  const [fileIndexItem, setFileIndexItem] = React.useState(props.fileIndexItem);

  var location = new Query().UrlQueryInfoApi(props.filePath);
  const responseObject = useFetch(location, 'get');
  useEffect(() => {
    if (!responseObject) return;
    var infoFileIndexItem = new CastToInterface().InfoFileIndexArray(responseObject);
    // there is a bug in the api
    infoFileIndexItem[0].lastEdited = fileIndexItem.lastEdited;
    setFileIndexItem(infoFileIndexItem[0]);
  }, [responseObject]);

  // var isFormEnabled = true;
  // const [isEnabled, setIsEnabled] = React.useState(true);
  // useEffect(() => {
  //   if (!fileIndexItem.status) return;
  //   if (fileIndexItem.status == "Ok") return;
  //   console.log(fileIndexItem.status);

  //   setIsEnabled(false);
  // }, [fileIndexItem]);

  // For the display
  const [isFormEnabled, setFormEnabled] = React.useState(true);
  useEffect(() => {
    if (!props.fileIndexItem.status) return;
    console.log("sdfsdf", props.fileIndexItem.status);

    switch (props.fileIndexItem.status) {
      case IExifStatus.Deleted:
      case IExifStatus.ReadOnly:
        setFormEnabled(false);
        break;
      default:
        setFormEnabled(true);
        break;
    }
  }, [props.status]);


  function handleChange(event: React.ChangeEvent<HTMLDivElement>) {
    let value = event.currentTarget.innerText;
    let name = event.currentTarget.dataset["name"];

    if (!name) return;
    if (!value) return;

    // compare
    var fileIndexObject: any = fileIndexItem
    if (!fileIndexObject[name] === undefined) return; //to update emthy start to first fill

    var currentString: string = fileIndexObject[name];
    if (value === currentString) return;

    // Sorry no empty strings are supported
    if (event.currentTarget.innerText.length === 1) {
      fileIndexObject[name] = "."
      console.log('not supported');
    }

    new Query().queryUpdateApi(props.filePath, name, event.currentTarget.innerText).then(item => {
      setFileIndexItem(item[0]);
    });
  }

  // To fast go the tags field
  const tagsReference = useRef<HTMLDivElement>(null);
  useKeyboardEvent(/^(t|i)$/, (event: KeyboardEvent) => {
    if (new Keyboard().isInForm(event)) return;
    event.preventDefault();
    var current = tagsReference.current as HTMLDivElement;
    new Keyboard().SetFocusOnEndField(current);
  }, [props])


  return (<div className="sidebar">
    {fileIndexItem.status === IExifStatus.Deleted ? <><div className="content--header">
      Status
    </div> <div className="content--text">
        {fileIndexItem.status}
        Niet gevonden
    </div></> : null}

    <div className="content--header">
      Tags
      </div>
    <div className="content--text">
      <div onBlur={handleChange}
        data-name="tags"
        ref={tagsReference}
        suppressContentEditableWarning={true}
        contentEditable={isFormEnabled}
        className={isFormEnabled ? "form-control" : "form-control disabled"}>
        {fileIndexItem.tags}
      </div>
    </div>

    <div className="content--header">
      Info &amp; Titel
      </div>
    <div className="content--text">
      <h4>Info</h4>
      <div onBlur={handleChange}
        data-name="description"
        suppressContentEditableWarning={true}
        contentEditable={isFormEnabled}
        className={isFormEnabled ? "form-control" : "form-control disabled"}>
        {fileIndexItem.description}
      </div>
      <h4>Titel</h4>
      <div onBlur={handleChange}
        data-name="title"
        suppressContentEditableWarning={true}
        contentEditable={isFormEnabled}
        className={isFormEnabled ? "form-control" : "form-control disabled"}>
        {fileIndexItem.title}
      </div>
    </div>

    <div className="content--header">
      Kleur-Classificatie
      </div>
    <div className="content--text">
      <ColorClassSelect onToggle={() => { }} filePath={fileIndexItem.filePath} currentColorClass={fileIndexItem.colorClass} isEnabled={isFormEnabled}></ColorClassSelect>
    </div>

    {fileIndexItem.latitude || fileIndexItem.longitude || isValidDate(fileIndexItem.dateTime) || fileIndexItem.make || fileIndexItem.model || fileIndexItem.aperture || fileIndexItem.focalLength ?
      <div className="content--header">
        Details
      </div> : null}

    <div className="content--text">

      {isValidDate(fileIndexItem.dateTime) ? <div className="box">
        <div className="icon icon--date"></div>
        <b>{parseDate(fileIndexItem.dateTime)}</b>
        <p>{parseTime(fileIndexItem.dateTime)}</p>
      </div> : ""}

      {isValidDate(fileIndexItem.dateTime) ?
        <div className="box">
          <div className="icon icon--last-edited"></div>
          <b>{parseRelativeDate(fileIndexItem.lastEdited)}</b>
          <p>geleden bewerkt</p>
        </div> : ""}

      {fileIndexItem.make && fileIndexItem.model && fileIndexItem.aperture && fileIndexItem.focalLength ?
        <div className="box">
          <div className="icon icon--shutter-speed"></div>
          <b>{fileIndexItem.make} {fileIndexItem.model}</b>
          <p>f/{fileIndexItem.aperture}&nbsp;&nbsp;&nbsp;{fileIndexItem.shutterSpeed} sec&nbsp;&nbsp;&nbsp;
             {fileIndexItem.focalLength.toFixed(1)} mm&nbsp;&nbsp;&nbsp;{fileIndexItem.isoSpeed !== 0 ? <>ISO {fileIndexItem.isoSpeed}</> : null}</p>
        </div> : ""}


      {fileIndexItem.latitude && fileIndexItem.longitude ?
        <a className="box" target="_blank" rel="noopener noreferrer" href={"https://www.openstreetmap.org/?mlat=" +
          fileIndexItem.latitude + "&mlon=" + fileIndexItem.longitude + "#map=16/" +
          fileIndexItem.latitude + "/" + fileIndexItem.longitude}>
          <div className="icon icon--right icon--edit"></div>
          <div className="icon icon--location"></div>
          {fileIndexItem.locationCity && fileIndexItem.locationCountry ?
            <>
              <b>{fileIndexItem.locationCity}</b>
              <p>{fileIndexItem.locationCountry}</p>
            </> : <>
              <b>Onbekende </b>
              <p>locatie</p>
            </>}

        </a> : ""}

    </div>

  </div>);
});
export default DetailViewSidebar