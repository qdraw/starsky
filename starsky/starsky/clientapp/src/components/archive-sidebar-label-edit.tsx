import React, { memo, useEffect } from "react";
import useLocation from '../hooks/use-location';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import FetchPost from '../shared/fetchpost';
import { URLPath } from '../shared/url-path';

interface IDetailViewSidebarProps {
  fileIndexItems: Array<IFileIndexItem>,
  colorClassUsage: Array<number>,
}

interface ISidebarUpdate {
  tags: string,
  description: string,
  title: string
}

const ArchiveSidebarLabelEdit: React.FunctionComponent<IDetailViewSidebarProps> = memo((archive) => {

  var history = useLocation();

  // show select info
  const [select, setSelect] = React.useState(new URLPath().getSelect(history.location.search));
  useEffect(() => {
    setSelect(new URLPath().getSelect(history.location.search));
  }, [history.location.search]);


  const [update, setUpdate] = React.useState({} as ISidebarUpdate)

  const [isEnabled, setEnabled] = React.useState(false)

  console.log(update);

  function handleChange(event: React.ChangeEvent<HTMLDivElement> | React.KeyboardEvent<HTMLDivElement>) {
    let value = event.currentTarget.innerText;
    let name = event.currentTarget.dataset["name"];

    if (!name) return;
    if (!value) return;

    value = value.replace(/\n/g, "");
    switch (name) {
      case "tags":
        update.tags = value;
        break;
      case "description":
        update.description = value;
        break;
      case "title":
        update.title = value;
        break;
    }

    setUpdate(update);
    setEnabled(updateToString(update).length !== 0);
  }

  function updateToString(toUpdate: ISidebarUpdate): string {
    var bodyParams = new URLSearchParams();
    for (let key of Object.entries(toUpdate)) {
      if (key[1] && key[1].length >= 1) {
        bodyParams.set(key[0], key[1]);
      }
    }
    console.log(bodyParams.toString().length);

    return bodyParams.toString();
  }

  function pushUpdate() {

    var bodyParams = updateToString(update);
    if (bodyParams.length === 0) return;

    var selectParams = new URLSearchParams();
    for (let key of Object.entries(select)) {
      if (key[1] && key[1].length >= 1) {
        selectParams.set(key[0], key[1]);
      }
    }
    console.log(selectParams.toString());

    FetchPost('post', bodyParams.toString());


  }

  return (
    <div className="content--text">
      <h4>Tags:</h4>
      <div data-name="tags"
        onInput={handleChange}
        suppressContentEditableWarning={true}
        contentEditable={select.length !== 0}
        className={select.length !== 0 ? "form-control" : "form-control disabled"}>
      </div>
      <h4>Info</h4>
      <div
        onInput={handleChange}
        data-name="description"
        suppressContentEditableWarning={true}
        contentEditable={select.length !== 0}
        className={select.length !== 0 ? "form-control" : "form-control disabled"}>
      </div>
      <h4>Titel</h4>
      <div data-name="title"
        onInput={handleChange}
        suppressContentEditableWarning={true}
        contentEditable={select.length !== 0}
        className={select.length !== 0 ? "form-control" : "form-control disabled"}>
      </div>
      {isEnabled && select.length !== 0 ? <a className="btn btn--default" onClick={() => pushUpdate()}>Push me</a> : <a className="btn btn--default disabled" >disabled</a>}

    </div >);
});
export default ArchiveSidebarLabelEdit