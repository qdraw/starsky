import { mount } from 'enzyme';
import React from 'react';
import * as ArchiveContextWrapper from '../contexts-wrappers/archive-wrapper';
import SearchPage from './search-page';

describe("SearchPage", () => {
  it("default check if MenuSearch + context is called", () => {
    var contextSpy = jest.spyOn(ArchiveContextWrapper, 'default').mockImplementationOnce(() => { return <></> });
    mount(<SearchPage></SearchPage>);
    expect(contextSpy).toBeCalled();
  });
});