import React, { useState } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { Language } from "../../../shared/language";
import { LimitLength } from "./limit-length";

export interface IFormControlProps {
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
}

const FormControl: React.FunctionComponent<IFormControlProps> = ({
  onBlur,
  ...props
}) => {
  const maxlength = props.maxlength ? props.maxlength : 255;

  const [childLength, setChildLength] = useState(
    props.children?.toString().length ? props.children?.toString().length : 0
  );

  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageFieldMaxLength = language.token(
    language.text(
      "Het onderstaande veld mag maximaal {maxlength} tekens hebben",
      "The field below can have a maximum of {maxlength} characters"
    ),
    ["{maxlength}"],
    [maxlength.toString()]
  );

  /**
   * Limit length on paste event
   * @param element ClipboardEvent
   */
  const limitLengthPaste = function (
    element: React.ClipboardEvent<HTMLDivElement>
  ) {
    if (childLength + element.clipboardData.getData("Text").length <= maxlength)
      return;
    element.preventDefault();
    setChildLength(childLength + element.clipboardData.getData("Text").length);
  };

  const propsClassName = props.className ? props.className : "";

  return (
    <>
      {props.warning !== false && childLength >= maxlength ? (
        <div className="warning-box">{MessageFieldMaxLength}</div>
      ) : null}

      <div
        data-test="form-control"
        onBlur={
          new LimitLength(setChildLength, onBlur, maxlength).LimitLengthBlur
        }
        data-name={props.name}
        onKeyDown={
          new LimitLength(setChildLength, onBlur, maxlength).LimitLengthKey
        }
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
