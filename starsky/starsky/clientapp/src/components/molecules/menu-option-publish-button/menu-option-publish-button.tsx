import React, { memo, useState } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import { Language } from "../../../shared/language";
import { URLPath } from "../../../shared/url-path";
import ModalPublish from "../../organisms/modal-publish/modal-publish";

interface MoreMenuPublishButtonProps {
  select: string[];
  stateFileIndexItems: IFileIndexItem[];
}

const MenuOptionPublishButton: React.FunctionComponent<MoreMenuPublishButtonProps> = memo(
  ({ select, stateFileIndexItems }) => {
    const [isModalPublishOpen, setModalPublishOpen] = useState(false);

    const settings = useGlobalSettings();
    const language = new Language(settings.language);
    const MessagePublish = language.text("Publiceren", "Publish");

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

        <li
          data-test="publish"
          className="menu-option"
          onClick={() => setModalPublishOpen(!isModalPublishOpen)}
        >
          {MessagePublish}
        </li>
      </>
    );
  }
);

export default MenuOptionPublishButton;
