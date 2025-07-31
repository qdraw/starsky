import React, { useState } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import localization from "../../../localization/localization.json";
import { Language } from "../../../shared/language";
import getTextLength from "./get-text-length";
import { LimitLength } from "./limit-length";

interface IFormControlProps {
  contentEditable: boolean;
  onBlur?(event: React.ChangeEvent<HTMLDivElement>): void;
  onInput?(event: React.ChangeEvent<HTMLDivElement>): void;
  reference?: React.RefObject<HTMLDivElement>;
  name: string;
  className?: string;
  maxlength?: number;
  children?: React.ReactNode;
  warning?: boolean;
  spellcheck?: boolean;
  "data-test"?: string;
}

const FormControl: React.FunctionComponent<IFormControlProps> = ({ onBlur, ...props }) => {
  const maxlength = props.maxlength ?? 255;
  const [childLength, setChildLength] = useState(getTextLength(props.children));

  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageFieldMaxLength = language.key(
    localization.MessageFieldMaxLength,
    ["{maxlength}"],
    [maxlength.toString()]
  );

  /**
   * Limit length on paste event
   * @param element ClipboardEvent
   */
  const limitLengthPaste = function (element: React.ClipboardEvent<HTMLDivElement>) {
    if (childLength + element.clipboardData.getData("Text").length <= maxlength) return;
    element.preventDefault();
    setChildLength(childLength + element.clipboardData.getData("Text").length);
  };

  const propsClassName = props.className ?? "";

  return (
    <>
      {props.warning !== false && childLength >= maxlength ? (
        <div className="warning-box">{MessageFieldMaxLength}</div>
      ) : null}

      {/* NOSONAR(S6847) */}
      <div
        data-test={props["data-test"] ?? "form-control"}
        onBlur={new LimitLength(setChildLength, onBlur, maxlength).LimitLengthBlur}
        data-name={props.name}
        onKeyDown={new LimitLength(setChildLength, onBlur, maxlength).LimitLengthKey}
        onInput={props.onInput}
        onPaste={limitLengthPaste}
        spellCheck={props.spellcheck}
        ref={props.reference}
        suppressContentEditableWarning={true}
        contentEditable={props.contentEditable}
        className={
          props.contentEditable
            ? `form-control ${propsClassName}`
            : `form-control disabled ${propsClassName}`
        }
      >
        {props.children}
      </div>
    </>
  );
};

export default FormControl;
