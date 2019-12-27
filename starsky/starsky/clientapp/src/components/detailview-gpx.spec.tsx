import { mount, shallow } from 'enzyme';
import React from 'react';
import { IConnectionDefault } from '../interfaces/IConnectionDefault';
import * as FetchXml from '../shared/fetch-xml';
import DetailViewGpx from './detailview-gpx';

describe("DetailViewGpx", () => {

  it("renders (without state component)", () => {
    shallow(<DetailViewGpx></DetailViewGpx>)
  });

  describe("useContext-test", () => {
    it("renders (without state component)", () => {

      var xmlDocument = "";
      const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        statusCode: 200, data: xmlDocument
      } as IConnectionDefault);
      var spyGet = jest.spyOn(FetchXml, 'default').mockImplementationOnce(() => mockGetIConnectionDefault);


      var gpx = mount(<DetailViewGpx></DetailViewGpx>);

      expect(gpx.exists("#map")).toBeTruthy();

      expect(spyGet).toBeCalled();
      console.log(gpx.html());

    });
  });
});