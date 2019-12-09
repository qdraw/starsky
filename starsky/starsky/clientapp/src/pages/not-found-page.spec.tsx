import { shallow } from 'enzyme';
import React from 'react';
import NotFoundPage from './not-found-page';


describe("NotFoundPage", () => {
  it("has MenuDefault child Component", () => {

    var notFoundComponent = shallow(<NotFoundPage></NotFoundPage>);

    var headerText = notFoundComponent.find('.content--header').text()


    expect(headerText).toBe('Oeps niet gevonden')
  });
});