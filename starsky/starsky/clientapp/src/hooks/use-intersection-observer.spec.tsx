import { mount, shallow } from 'enzyme';
import React, { useRef } from 'react';
import useIntersection, { newIntersectionObserver } from './use-intersection-observer';

describe("useIntersection", () => {

  const IntersectionComponentTest = () => {
    const target = useRef<HTMLDivElement>(null);
    shallow(<div ref={target}></div>)
    useIntersection(target);
    return null;
  };

  it("call api", () => {
    const focus = jest.fn();
    const useRefSpy = jest.spyOn(React, 'useRef').mockReturnValueOnce({ current: { focus } });

    mount(<IntersectionComponentTest />)
    expect(useRefSpy).toHaveBeenCalledTimes(1)
  });


  const NewIntersectionComponentTest = () => {
    const target = useRef<HTMLDivElement>(null);
    mount(<div ref={target}></div>)
    const tagRef = { current: { scrollHeight: 100, clientHeight: 200 } };
    newIntersectionObserver(target, jest.fn(), true, tagRef);
    return null;
  };

  it("newIntersectionObserver is not failing", () => {
    mount(<NewIntersectionComponentTest />);
    // there is no assert/check
  });


});

