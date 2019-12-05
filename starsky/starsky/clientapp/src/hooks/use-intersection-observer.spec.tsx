import { mount, shallow } from 'enzyme';
import React, { useRef } from 'react';
import useIntersection from './use-intersection-observer';


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


});

