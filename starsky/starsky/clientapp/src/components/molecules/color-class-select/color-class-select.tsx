import "core-js/modules/es.array.find";
import React, { useEffect, useState } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { Language } from "../../../shared/language";
import Notification, {
  NotificationType
} from "../../atoms/notification/notification";
import Portal from "../../atoms/portal/portal";
import Preloader from "../../atoms/preloader/preloader";
import { ColorClassUpdateSingle } from "./color-class-update-single";

export interface IColorClassSelectProps {
  currentColorClass?: number;
  isEnabled: boolean;
  filePath: string;
  clearAfter?: boolean;
  onToggle(value: number): void;
  collections: boolean;
}

/**
 * Used to update colorclasses
 */
const ColorClassSelect: React.FunctionComponent<IColorClassSelectProps> = (
  props
) => {
  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  const colorContent: Array<string> = [
    language.text("Kleurloos", "Colorless"),
    language.text("Roze", "Pink"),
    language.text("Rood", "Red"),
    language.text("Oranje", "Orange"),
    language.text("Geel", "Yellow"),
    language.text("Groen", "Green"),
    language.text("Azuur", "Azure"),
    language.text("Blauw", "Blue"),
    language.text("Grijs", "Grey")
  ];

  const [currentColorClass, setCurrentColorClass] = React.useState(
    props.currentColorClass
  );

  /** re-render when switching page */
  useEffect(() => {
    setCurrentColorClass(props.currentColorClass);
  }, [props.currentColorClass, props.filePath]);

  // for showing a notification
  const [isError, setIsError] = useState("");
  const [isLoading, setIsLoading] = useState(false);

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
      {isLoading ? (
        <Portal>
          <Preloader isWhite={false} isOverlay={true} />
        </Portal>
      ) : null}
      <div
        className={
          props.isEnabled
            ? "colorclass colorclass--select"
            : "colorclass colorclass--select colorclass--disabled"
        }
      >
        {colorContent.map((item, index) => (
          <button
            key={index}
            data-test={"color-class-select-" + index}
            onClick={() => {
              new ColorClassUpdateSingle(
                props.isEnabled,
                setIsLoading,
                props.filePath,
                props.collections,
                setIsError,
                settings,
                setCurrentColorClass,
                props.onToggle,
                props.clearAfter
              ).Update(index);
            }}
            className={
              currentColorClass === index
                ? "btn btn--default colorclass colorclass--" + index + " active"
                : "btn colorclass colorclass--" + index
            }
          >
            <label />
            <span>{item}</span>
          </button>
        ))}
      </div>
    </>
  );
};

export default ColorClassSelect;
