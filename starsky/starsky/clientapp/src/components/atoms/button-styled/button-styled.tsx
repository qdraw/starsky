import React, { memo } from "react";

export interface IButtonProps {
  type?: "button" | "submit" | "reset";
  disabled?: boolean;
  className?: string;
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
    >
      {props.children}
    </button>
  );
});

export default ButtonStyled;
