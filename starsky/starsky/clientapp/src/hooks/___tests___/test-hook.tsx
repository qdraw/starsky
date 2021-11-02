import { render } from "@testing-library/react";
import React from "react";
import { act } from "react-dom/test-utils";

type ModalPropTypes = {
  children: (hookValues: any) => any;
};

export const mountReactHook = (hook: any, args: any[]) => {
  const Component = ({ children }: ModalPropTypes) => {
    return children(hook(...args));
  };
  const componentHook = {};
  let componentMount = render(<></>);

  act(() => {
    componentMount = render(
      <Component>
        {(hookValues) => {
          Object.assign(componentHook, hookValues);
          return null;
        }}
      </Component>
    );
  });
  return { componentMount, componentHook };
};
