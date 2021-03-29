import React, { memo } from "react";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import { URLPath } from "../../../shared/url-path";
import ModalPublish from "./modal-publish";

interface IModalPublishWrapperProps {
  select: string[] | undefined;
  stateFileIndexItems: IFileIndexItem[];
  isModalPublishOpen: boolean;
  setModalPublishOpen: React.Dispatch<React.SetStateAction<boolean>>;
}

/**
 * Wrapper to hide modal in the menu
 */
const ModalPublishToggleWrapper: React.FunctionComponent<IModalPublishWrapperProps> = memo(
  ({
    select,
    stateFileIndexItems,
    isModalPublishOpen,
    setModalPublishOpen
  }) => {
    return (
      <>
        {isModalPublishOpen ? (
          <ModalPublish
            handleExit={() => setModalPublishOpen(!isModalPublishOpen)}
            select={
              select
                ? new URLPath().MergeSelectFileIndexItem(
                    select,
                    stateFileIndexItems
                  )
                : []
            }
            isOpen={isModalPublishOpen}
          />
        ) : null}
      </>
    );
  }
);

export default ModalPublishToggleWrapper;
