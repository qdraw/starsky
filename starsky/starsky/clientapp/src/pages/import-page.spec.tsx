import { mount } from 'enzyme';
import React from 'react';
import * as DropArea from '../components/drop-area';
import * as MenuSearch from '../components/menu-search';
import ImportPage from './import-page';

describe("ImportPage", () => {
  it("default check if MenuSearch is called", () => {
    var menuSearchSpy = jest.spyOn(MenuSearch, 'default').mockImplementationOnce(() => { return <></> });
    var dropAreaSpy = jest.spyOn(DropArea, 'default').mockImplementationOnce(() => { return <></> });
    mount(<ImportPage></ImportPage>);
    expect(menuSearchSpy).toBeCalled();
    expect(dropAreaSpy).toBeCalled()
  });
});