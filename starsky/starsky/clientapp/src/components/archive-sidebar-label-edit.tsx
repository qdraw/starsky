import React, { memo, useEffect } from "react";
import { ArchiveContext } from '../contexts/archive-context';
import useLocation from '../hooks/use-location';
import FetchPost from '../shared/fetchpost';
import { URLPath } from '../shared/url-path';

interface ISidebarUpdate {
  tags: string,
  description: string,
  title: string,
  append: boolean,
  collections: boolean,
}

interface IDetailViewSidebarLabelEditProps {
  subPath: string;
}

const ArchiveSidebarLabelEdit: React.FunctionComponent<IDetailViewSidebarLabelEditProps> = memo((archive) => {

  var history = useLocation();
  let { state, dispatch } = React.useContext(ArchiveContext);

  // show select info
  const [select, setSelect] = React.useState(new URLPath().getSelect(history.location.search));
  useEffect(() => {
    setSelect(new URLPath().getSelect(history.location.search));
  }, [history.location.search]);


  const [update, setUpdate] = React.useState({
    append: true,
    collections: true,
  } as ISidebarUpdate)

  const [isEnabled, setEnabled] = React.useState(false)

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
    setEnabled(updateToUrlSearchParams(update).toString().length !== 0);
  }

  function updateToUrlSearchParams(toUpdate: ISidebarUpdate): URLSearchParams {
    var bodyParams = new URLSearchParams();
    for (let key of Object.entries(toUpdate)) {
      if (key[1] && key[1].length >= 1) {
        bodyParams.set(key[0], key[1]);
      }
      if (key[1] === true || key[1] === false) {
        bodyParams.set(key[0], key[1]);
      }
    }
    return bodyParams;
  }

  function pushUpdate() {

    var bodyParams = updateToUrlSearchParams(update);
    if (bodyParams.toString().length === 0) return;

    var selectParams = "";
    for (let index = 0; index < select.length; index++) {
      const element = select[index];
      selectParams += archive.subPath + "/" + element;
      if (index !== select.length - 1) {
        selectParams += ";";
      }
    }

    if (selectParams.length === 0) return;
    bodyParams.append("f", selectParams)

    FetchPost("/api/update", bodyParams.toString());

    dispatch({ type: 'update', ...update, select });
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