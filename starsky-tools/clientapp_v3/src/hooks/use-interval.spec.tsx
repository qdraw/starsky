import { mount } from 'enzyme';
import React, { memo } from 'react';
import useInterval from './use-interval';

describe("useInterval", () => {

  interface UseIntervalComponentTestProps {
    callback: Function,
    timer: number
  }

  const UseIntervalComponentTest: React.FunctionComponent<UseIntervalComponentTestProps> = memo((props) => {
    useInterval(props.callback, props.timer);
    console.log('-h');
    
    return <></>;
  });

  it("check if is called once", (done) => {
    function callback() {
      done();
    }
    mount(<UseIntervalComponentTest timer={0} callback={callback}>t</UseIntervalComponentTest>);
  });

  it("check if setInterval is called", () => {
    var clearIntervalSpy = jest.spyOn(globalThis, 'setInterval')
      .mockImplementationOnce(() => { return {} as any })

    var component = mount(<UseIntervalComponentTest timer={10} callback={jest.fn()}/>);
    component.unmount();
   
    expect(clearIntervalSpy).toBeCalled();
  });
});