import React, { memo } from "react";

export interface IButtonProps {
  children?: React.ReactNode;
  type?: "button" | "submit" | "reset";
  disabled?: boolean;
  className?: string;
  "data-test"?: string;
  onClick?: <T>(
    event: React.MouseEvent<HTMLButtonElement>
  ) => void | Promise<T>;
}

const ButtonStyled: React.FunctionComponent<IButtonProps> = memo((props) => {
  return (
    <button
      type={props.type || "button"}
      disabled={props.disabled}
      className={props.className}
      data-test={props["data-test"]}
    >
      {props.children}
    </button>
  );
});

export default ButtonStyled;
