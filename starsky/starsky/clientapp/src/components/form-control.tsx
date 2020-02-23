import React, { useState } from "react";
import useGlobalSettings from '../hooks/use-global-settings';
import { Language } from '../shared/language';

export interface IFormControlProps {
  contentEditable: boolean;
  onBlur?(event: React.ChangeEvent<HTMLDivElement>): void;
  onInput?(event: React.ChangeEvent<HTMLDivElement>): void;
  reference?: React.RefObject<HTMLDivElement>;
  name: string;
  className?: string;
  maxlength?: number;
  children: React.ReactNode;
}

const FormControl: React.FunctionComponent<IFormControlProps> = (props) => {

  var maxlength = props.maxlength ? props.maxlength : 255;

  const [childLength, setChildLength] = useState(props.children?.toString().length ? props.children?.toString().length : 0);

  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageFieldMaxLength = language.token(language.text("Het onderstaande veld mag maximaal {maxlength} tekens hebben",
    "The field below can have a maximum of {maxlength} characters"), ["{maxlength}"], [maxlength.toString()]);

  /**
   * Limit on keydown
   * @param element KeydownEvent
   */
  var limitLengthKey = function (element: React.KeyboardEvent<HTMLDivElement>) {
    if (!element.currentTarget.textContent) {
      setChildLength(0);
      return;
    }

    var elementLength = element.currentTarget.textContent.trim().length
    setChildLength(elementLength);

    if (elementLength < maxlength || (element.key === "x" && element.ctrlKey) ||
      (element.key === "x" && element.metaKey) || !element.key.match(/^.{0,1}$/)) return;

    element.preventDefault();
  }

  /**
   * Limit length on paste event
   * @param element ClipboardEvent
   */
  var limitLengthPaste = function (element: React.ClipboardEvent<HTMLDivElement>) {

    // DEBUG
    console.log(childLength + element.clipboardData.getData('Text').length <= childLength,
      childLength, element.clipboardData.getData('Text').length, maxlength);

    if (childLength + element.clipboardData.getData('Text').length <= maxlength) return;
    element.preventDefault();
    setChildLength(childLength + element.clipboardData.getData('Text').length);
  }

  /**
   * Limit length before sending to onBlurEvent
   * @param element Focus event
   */
  var limitLengthBlur = function (element: React.FocusEvent<HTMLDivElement>) {
    if (element.currentTarget.innerHTML.length > maxlength) {
      setChildLength(element.currentTarget.innerHTML.length);
      return;
    }
    if (!props.onBlur) return;
    props.onBlur(element)
  }

  return <>
    {childLength >= maxlength ? <div className="warning-box">{MessageFieldMaxLength}</div> : null}
    <div onBlur={limitLengthBlur}
      data-name={props.name}
      onKeyDown={limitLengthKey}
      onInput={props.onInput}
      onPaste={limitLengthPaste}
      ref={props.reference}
      suppressContentEditableWarning={true}
      contentEditable={props.contentEditable}
      className={props.contentEditable ? `form-control ${props.className ? props.className : ""}` :
        `form-control disabled ${props.className ? props.className : ""}`}>
      {props.children}
    </div></>
};

export default FormControl;