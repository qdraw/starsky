import { mount } from 'enzyme';
import React from 'react';
import * as MenuSearch from '../components/menu-search';
import * as ArchiveContextWrapper from '../contexts-wrappers/archive-wrapper';
import SearchPage from './search-page';

describe("SearchPage", () => {
  it("default check if MenuSearch + context is called", () => {
    // used as ==> import * as MenuSearch from '../components/menu-search';
    var menuSearchSpy = jest.spyOn(MenuSearch, 'default').mockImplementationOnce(() => { return <></> });
    var contextSpy = jest.spyOn(ArchiveContextWrapper, 'default').mockImplementationOnce(() => { return <></> });

    mount(<SearchPage></SearchPage>);

    expect(menuSearchSpy).toBeCalled();
    expect(contextSpy).toBeCalled();
  });
});