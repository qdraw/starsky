import React, { memo, useEffect, useRef } from "react";
import useFetch from '../hooks/use-fetch';
import useKeyboardEvent from '../hooks/use-keyboard-event';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { CastToInterface } from '../shared/cast-to-interface';
import { isValidDate, parseDate, parseRelativeDate, parseTime } from '../shared/date';
import { Keyboard } from '../shared/keyboard';
import { Query } from '../shared/query';
import ColorClassSelect from './color-class-select';
interface IDetailViewSidebarProps {
  fileIndexItem: IFileIndexItem,
  filePath: string,
}

const DetailViewSidebar: React.FunctionComponent<IDetailViewSidebarProps> = memo((props) => {

  var isEnabled = true;
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

  function handleChange(event: React.ChangeEvent<HTMLDivElement>) {
    let value = event.currentTarget.innerText;
    let name = event.currentTarget.dataset["name"];

    if (!name) return;
    if (!value) return;

    // compare
    var fileIndexObject: any = fileIndexItem
    if (!fileIndexObject[name]) return;

    var currentString: string = fileIndexObject[name];
    if (value === currentString) return;

    // Sorry no empty strings are supported
    if (event.currentTarget.innerText.length === 1) {
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
    <div className="content--header">
      Tags
      </div>
    <div className="content--text">
      <div onBlur={handleChange}
        data-name="tags"
        ref={tagsReference}
        suppressContentEditableWarning={true}
        contentEditable={isEnabled}
        className={isEnabled ? "form-control" : "form-control disabled"}>
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
        contentEditable={isEnabled}
        className={isEnabled ? "form-control" : "form-control disabled"}>
        {fileIndexItem.description}
      </div>
      <h4>Titel</h4>
      <div onBlur={handleChange}
        data-name="title"
        suppressContentEditableWarning={true}
        contentEditable={isEnabled}
        className={isEnabled ? "form-control" : "form-control disabled"}>
        {fileIndexItem.title}
      </div>
    </div>

    <div className="content--header">
      Kleur-Classificatie
      </div>
    <div className="content--text">
      <ColorClassSelect onToggle={() => { }} filePath={fileIndexItem.filePath} currentColorClass={fileIndexItem.colorClass} isEnabled={isEnabled}></ColorClassSelect>
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