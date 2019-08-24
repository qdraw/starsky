import React, { memo, useEffect } from "react";
import useLocation from '../hooks/use-location';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { URLPath } from '../shared/url-path';
import ArchiveSidebarSelectionList from './archive-sidebar-selection-list';

interface IDetailViewSidebarProps {
  fileIndexItems: Array<IFileIndexItem>,
  colorClassUsage: Array<number>,
}

// enum ISidebarUpdateTypes {
//   tags,
//   description,
//   title,
// }

interface ISidebarUpdate {
  tags: string,
  description: string,
  title: string,
}



const ArchiveSidebar: React.FunctionComponent<IDetailViewSidebarProps> = memo((archive) => {

  var history = useLocation();

  // show select info
  const [select, setSelect] = React.useState(new URLPath().getSelect(history.location.search));
  useEffect(() => {
    setSelect(new URLPath().getSelect(history.location.search));
  }, [history.location.search]);


  const [update, setUpdate] = React.useState({} as ISidebarUpdate)

  function handleChange(event: React.ChangeEvent<HTMLDivElement>) {
    let value = event.currentTarget.innerText;
    let name = event.currentTarget.dataset["name"];

    if (!name) return;
    if (!value) return;

    value = value.replace(/\n/g, "");

    var toUpdate = update;
    switch (name) {
      case "tags":
        toUpdate.tags = value;
        break;
      case "description":
        toUpdate.description = value;
        break;
      case "title":
        toUpdate.title = value;
        break;
    }
    setUpdate(toUpdate);
  }

  function pushUpdate() {
    var params = new URLSearchParams();
    for (let key of Object.entries(update)) {
      if (key[1] && key[1].length >= 1) {
        params.set(key[0], key[1]);
      }
    }

    console.log(params.toString());

  }

  return (<div className="sidebar">
    <div className="content--header">
      Selectie
    </div>
    <ArchiveSidebarSelectionList {...archive}></ArchiveSidebarSelectionList>

    <div className="content--header">
      Labels wijzigingen
    </div>
    <div className="content--text">
      <h4>Tags:</h4>
      <div data-name="tags"
        onBlur={handleChange}
        suppressContentEditableWarning={true}
        contentEditable={select.length !== 0}
        className={select.length !== 0 ? "form-control" : "form-control disabled"}>
      </div>
      <h4>Info</h4>
      <div
        onBlur={handleChange}
        data-name="description"
        suppressContentEditableWarning={true}
        contentEditable={select.length !== 0}
        className={select.length !== 0 ? "form-control" : "form-control disabled"}>
      </div>
      <h4>Titel</h4>
      <div data-name="title"
        onBlur={handleChange}
        suppressContentEditableWarning={true}
        contentEditable={select.length !== 0}
        className={select.length !== 0 ? "form-control" : "form-control disabled"}>
      </div>
      <a className="btn btn--default" onClick={() => pushUpdate()}>Push me</a>
    </div>

    <div className="content--header">
      Kleur-Classificatie
      </div>
    <div className="content--text">
      {/* <ColorClassSelect filePath={fileIndexItem.filePath} currentColorClass={fileIndexItem.colorClass} isEnabled={isEnabled}></ColorClassSelect> */}
    </div>


  </div>);
});
export default ArchiveSidebar