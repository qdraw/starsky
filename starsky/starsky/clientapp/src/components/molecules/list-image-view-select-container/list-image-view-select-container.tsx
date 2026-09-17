import React, { memo, useEffect } from "react";
import useLocation from "../../../hooks/use-location/use-location";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import { DRAG_MOVE_MIME, IDragMoveData } from "../../../shared/move-file-helper";
import { URLPath } from "../../../shared/url/url-path";
import { UrlQuery } from "../../../shared/url/url-query";
import Link from "../../atoms/link/link";
import Preloader from "../../atoms/preloader/preloader";

interface IListImageBox {
  item: IFileIndexItem;
  className?: string;
  children?: React.ReactNode;

  /**
   * When selecting and pressing shift
   * @param filePath the entire path (subPath style)
   */
  onSelectionCallback?(filePath: string): void;

  /** Returns the currently selected file/folder paths for drag-start encoding */
  onDragStart?(): IDragMoveData;

  /** Called when files are dropped onto this item (only fired for directory items) */
  onDropFiles?(targetFolderPath: string): void;
}

const ListImageViewSelectContainer: React.FunctionComponent<IListImageBox> = memo(
  ({ item, className: propsClassName, onSelectionCallback, onDragStart, onDropFiles, children }) => {
    item.isDirectory ??= false;

    const className = propsClassName ?? "list-image-box";
    const history = useLocation();

    // Check if select exist or Length 0 or more
    const [select, setSelect] = React.useState(
      new URLPath().StringToIUrl(history.location.search).select
    );
    useEffect(() => {
      setSelect(new URLPath().StringToIUrl(history.location.search).select);
    }, [history.location.search]);

    // reset preloader state when a new filePath is loaded
    useEffect(() => {
      setPreloaderState(false);
    }, [item.filePath]);

    function toggleSelection(fileName: string): void {
      const urlObject = new URLPath().toggleSelection(fileName, history.location.search);
      history.navigate(new URLPath().IUrlToString(urlObject), {
        replace: true
      });
      setSelect(urlObject.select);
    }

    const preloader = <Preloader isOverlay={true} isWhite={false} />;
    const [preloaderState, setPreloaderState] = React.useState(false);
    const [isDragOver, setIsDragOver] = React.useState(false);

    function preloaderStateOnClick(event: React.MouseEvent) {
      // Command (mac) or ctrl click means open new window
      // event.button = is only trigged in safari
      if (event.metaKey || event.ctrlKey || event.button === 1) return;
      setPreloaderState(true);
    }

    function handleDragStart(event: React.DragEvent) {
      if (!onDragStart) return;
      const data = onDragStart();
      event.dataTransfer.setData(DRAG_MOVE_MIME, JSON.stringify(data));
      event.dataTransfer.effectAllowed = "move";
    }

    function handleDragOver(event: React.DragEvent) {
      if (!event.dataTransfer.types.includes(DRAG_MOVE_MIME)) return;
      event.preventDefault();
      event.dataTransfer.dropEffect = "move";
      setIsDragOver(true);
    }

    function handleDragLeave() {
      setIsDragOver(false);
    }

    function handleDrop(event: React.DragEvent) {
      event.preventDefault();
      setIsDragOver(false);
      if (!onDropFiles) return;
      onDropFiles(item.filePath);
    }

    const isDropTarget = item.isDirectory && !!onDropFiles;

    // selected state
    if (select) {
      return (
        <div
          className={`${className} ${className}--select`}
          data-filepath={item.filePath}
          data-test="list-image-view-select-container"
          draggable={!!onDragStart}
          onDragStart={handleDragStart}
          onDragOver={isDropTarget ? handleDragOver : undefined}
          onDragLeave={isDropTarget ? handleDragLeave : undefined}
          onDrop={isDropTarget ? handleDrop : undefined}
        >
          <button
            type="button"
            onClick={(event) => {
              // multiple select using the shift key
              if (!event.shiftKey) {
                toggleSelection(item.fileName);
              } else if (event.shiftKey && onSelectionCallback) {
                onSelectionCallback(item.filePath);
              }
            }}
            className={
              (select.includes(item.fileName)
                ? "box-content box-content--selected colorclass--" +
                  item.colorClass +
                  " isDirectory-" +
                  item.isDirectory
                : "box-content colorclass--" + item.colorClass + " isDirectory-" + item.isDirectory) +
              (isDragOver ? " box-content--drag-over" : "")
            }
          >
            {children}
          </button>
        </div>
      );
    }

    // default state
    // data-filepath is needed to scroll to
    return (
      <div
        data-test="list-image-view-select-container"
        className={`${className} box--view`}
        data-filepath={item.filePath}
      >
        {/* for slow connections show preloader icon */}
        {preloaderState ? preloader : null}
        {/* the <a href to the child page */}
        <Link
          onClick={preloaderStateOnClick}
          title={item.fileName}
          to={new UrlQuery().updateFilePathHash(history.location.search, item.filePath)}
          className={
            "box-content colorclass--" + item.colorClass + " isDirectory-" + item.isDirectory
          }
        >
          {children}
        </Link>
      </div>
    );
  }
);

export default ListImageViewSelectContainer;
