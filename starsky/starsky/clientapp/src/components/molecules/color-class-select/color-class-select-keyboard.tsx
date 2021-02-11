import React, { useEffect, useState } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useKeyboardEvent from "../../../hooks/use-keyboard/use-keyboard-event";
import { Keyboard } from "../../../shared/keyboard";
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

  useKeyboardEvent(/[0-8]/, (event: KeyboardEvent) => {
    if (new Keyboard().isInForm(event)) return;

    new ColorClassUpdateSingle(
      props.isEnabled,
      setIsLoading,
      props.filePath,
      props.collections,
      setIsError,
      settings,
      () => {
        setIsDone("Colorclass is updated");
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
          <Preloader isDetailMenu={false} isOverlay={true} />
        </Portal>
      ) : null}
    </>
  );
};

export default ColorClassSelectKeyboard;
