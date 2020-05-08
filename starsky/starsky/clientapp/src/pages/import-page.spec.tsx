import { mount } from 'enzyme';
import React from 'react';
import * as DropArea from '../components/atoms/drop-area/drop-area';
import * as MenuDefault from '../components/organisms/menu-default/menu-default';
import ImportPage from './import-page';

describe("ImportPage", () => {
  it("default check if MenuDefault is called", () => {
    var menuDefaultSpy = jest.spyOn(MenuDefault, 'default').mockImplementationOnce(() => { return <></> });
    var dropAreaSpy = jest.spyOn(DropArea, 'default').mockImplementationOnce(() => { return <></> });
    mount(<ImportPage></ImportPage>);
    expect(menuDefaultSpy).toBeCalled();
    expect(dropAreaSpy).toBeCalled()
  });
});