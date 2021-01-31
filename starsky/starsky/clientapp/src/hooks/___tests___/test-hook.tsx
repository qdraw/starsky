import { mount, shallow } from "enzyme";
import React from "react";
import { act } from "react-dom/test-utils";

type ModalPropTypes = {
  children: (hookValues: any) => any;
};

export const shallowReactHook = (hook: any, args: string[]) => {
  const Component = ({ children }: ModalPropTypes) => {
    return children(hook(...args));
  };
  const componentHook = {};
  let componentMount;

  act(() => {
    componentMount = shallow(
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

export const mountReactHook = (hook: any, args: any[]) => {
  const Component = ({ children }: ModalPropTypes) => {
    return children(hook(...args));
  };
  const componentHook = {};
  let componentMount = mount(<></>);

  act(() => {
    componentMount = mount(
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
