import React, { useEffect, useState } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useKeyboardEvent from "../../../hooks/use-keyboard/use-keyboard-event";
import { Keyboard } from "../../../shared/keyboard";
import { Language } from "../../../shared/language";
import Notification, {
  NotificationType
} from "../../atoms/notification/notification";
import Portal from "../../atoms/portal/portal";
import Preloader from "../../atoms/preloader/preloader";
import { IColorClassSelectProps } from "../color-class-select/color-class-select";
import { ColorClassUpdateSingle } from "./color-class-update-single";

const ColorClassSelectKeyboard: React.FunctionComponent<IColorClassSelectProps> = (
  props
) => {
  // for showing a notification
  const [isError, setIsError] = useState("");
  const [isDone, setIsDone] = useState("");

  const [isLoading, setIsLoading] = useState(false);
  const settings = useGlobalSettings();

  useEffect(() => {
    setIsDone("");
  }, [props.filePath]);

  const MessageColorClassIsUpdated = new Language(settings.language).text(
    "Colorclass is bijgewerkt",
    "Colorclass is updated"
  );

  useKeyboardEvent(/[0-8]/, (event: KeyboardEvent) => {
    // cmd + 0 or ctrl are zoom functions
    if (new Keyboard().isInForm(event) || event.ctrlKey || event.metaKey)
      return;

    new ColorClassUpdateSingle(
      props.isEnabled,
      setIsLoading,
      props.filePath,
      props.collections,
      setIsError,
      settings,
      () => {
        setIsDone(MessageColorClassIsUpdated);
      },
      props.onToggle,
      props.clearAfter
    ).Update(Number(event.key));
  });

  return (
    <>
      {isError ? (
        <Notification
          callback={() => setIsError("")}
          type={NotificationType.danger}
        >
          {isError}
        </Notification>
      ) : null}

      {isDone ? (
        <Notification
          callback={() => setIsDone("")}
          type={NotificationType.default}
        >
          {isDone}
        </Notification>
      ) : null}
      {isLoading ? (
        <Portal>
          <Preloader isWhite={false} isOverlay={true} />
        </Portal>
      ) : null}
    </>
  );
};

export default ColorClassSelectKeyboard;
