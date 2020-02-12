import React from "react";

export interface IFormControlProps {
  isFormEnabled: boolean;
  handleChange(event: React.ChangeEvent<HTMLDivElement>): void;
  ref?: React.RefObject<HTMLDivElement>;
  text: string;
}

const FormControl: React.FunctionComponent<IFormControlProps> = (props) => {

  return <div onBlur={props.handleChange}
    data-name="tags"
    ref={props.ref}
    suppressContentEditableWarning={true}
    contentEditable={props.isFormEnabled}
    className={props.isFormEnabled ? "form-control" : "form-control disabled"}>
    {props.text}
  </div>
};

export default FormControl;