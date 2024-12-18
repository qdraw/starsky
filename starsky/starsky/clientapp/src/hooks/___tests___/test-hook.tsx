import { render, RenderResult } from "@testing-library/react";
import { act } from "react";

type ModalPropTypes = {
  children: (hookValues: unknown) => React.ReactNode;
};

export type MountReactHookResult = {
  componentMount: RenderResult;
  componentHook: object;
};

export const mountReactHook = (hook: (...args: unknown[]) => unknown, args: unknown[]) => {
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
  return { componentMount, componentHook } as MountReactHookResult;
};
