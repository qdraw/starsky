import React, { memo, useState } from "react";
import useLocation from "../../../hooks/use-location/use-location";
import { DRAG_MOVE_MIME, IDragMoveData, moveDragAndDropFiles } from "../../../shared/move-file-helper";
import { URLPath } from "../../../shared/url/url-path";
import { UrlQuery } from "../../../shared/url/url-query";
import Link from "../../atoms/link/link";

/**
 * subPath is child folder
 * Breadcrumb variable should only contain parent Folders
 */
interface IBreadcrumbProps {
  subPath: string;
  breadcrumb: Array<string>;
}

const Breadcrumbs: React.FunctionComponent<IBreadcrumbProps> = memo((props) => {
  // used for reading current location
  const history = useLocation();
  const [dragOverItem, setDragOverItem] = useState<string | null>(null);

  function handleDragOver(event: React.DragEvent, targetPath: string) {
    if (!event.dataTransfer.types.includes(DRAG_MOVE_MIME)) return;
    event.preventDefault();
    event.dataTransfer.dropEffect = "move";
    setDragOverItem(targetPath);
  }

  function handleDragLeave() {
    setDragOverItem(null);
  }

  async function handleDrop(event: React.DragEvent, targetPath: string) {
    event.preventDefault();
    setDragOverItem(null);
    const raw = event.dataTransfer.getData(DRAG_MOVE_MIME);
    if (!raw) return;
    let data: IDragMoveData;
    try {
      data = JSON.parse(raw) as IDragMoveData;
    } catch {
      return;
    }
    const ok = await moveDragAndDropFiles(data.filePaths, data.folderPaths, targetPath);
    if (ok) {
      history.navigate(new UrlQuery().updateFilePathHash(history.location.search, targetPath));
    }
  }

  if (!props.subPath || !props.breadcrumb) return <div className="breadcrumb" />;
  return (
    <div className={props.subPath.length >= 28 ? "breadcrumb breadcrumb--long" : "breadcrumb"}>
      {props.breadcrumb.map((item, index) => {
        let name = item.split("/")[item.split("/").length - 1];

        // instead of nothing
        if (index === 0) {
          name = "Home";
        }

        const isDragOver = dragOverItem === item;

        // For the home page
        if (item === props.subPath) {
          return (
            <span
              key={item}
              data-test={"breadcrumb-span"}
              className={isDragOver ? "breadcrumb__item--drag-over" : undefined}
              onDragOver={(e) => handleDragOver(e, item)}
              onDragLeave={handleDragLeave}
              onDrop={(e) => handleDrop(e, item)}
            >
              <Link to={new UrlQuery().updateFilePathHash(history.location.search, item)}>
                {name}
              </Link>
            </span>
          );
        }

        return (
          <span
            key={item}
            data-test={"breadcrumb-span"}
            className={isDragOver ? "breadcrumb__item--drag-over" : undefined}
            onDragOver={(e) => handleDragOver(e, item)}
            onDragLeave={handleDragLeave}
            onDrop={(e) => handleDrop(e, item)}
          >
            <Link to={new UrlQuery().updateFilePathHash(history.location.search, item)}>
              {name}
            </Link>{" "}
            <span> »</span>{" "}
          </span>
        );
      })}
      <span className="current" data-test={"breadcrumb-span-current"}>
        {new URLPath().FileNameBreadcrumb(props.subPath)}
      </span>
    </div>
  );
});

export default Breadcrumbs;
