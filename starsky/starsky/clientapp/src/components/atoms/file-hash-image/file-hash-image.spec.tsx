import { mount, shallow } from 'enzyme';
import React from 'react';
import { IConnectionDefault } from '../../../interfaces/IConnectionDefault';
import { IDetailView, PageType } from '../../../interfaces/IDetailView';
import { IExifStatus } from '../../../interfaces/IExifStatus';
import { Orientation } from '../../../interfaces/IFileIndexItem';
import * as DetectAutomaticRotation from '../../../shared/detect-automatic-rotation';
import * as FetchGet from '../../../shared/fetch-get';
import { UrlQuery } from '../../../shared/url-query';
import FileHashImage from './file-hash-image';

describe("FileHashImage", () => {
  it("renders", () => {
    shallow(<FileHashImage isError={false} fileHash={""} />);
  });

  it("Rotation API is called return 202", () => {
    console.log('-- Rotation API is called return 202 --');

    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 202, data: {
        subPath: "/test/image.jpg",
        pageType: PageType.DetailView,
        fileIndexItem: { orientation: Orientation.Rotate270Cw, fileHash: 'needed', status: IExifStatus.Ok, filePath: "/test/image.jpg", fileName: "image.jpg" }
      } as IDetailView
    } as IConnectionDefault);


    let trackEventSpy = jest.spyOn(DetectAutomaticRotation, 'default').mockImplementationOnce(() => {
      return Promise.resolve(false);
    });


    // new OrientationHelper().DetectAutomaticRotation() = jet


    var spyGet = jest.spyOn(FetchGet, 'default')
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault);

    var component = mount(<FileHashImage isError={false} fileHash="" orientation={Orientation.Horizontal} />);

    component.update();

    expect(spyGet).toBeCalled();
    expect(spyGet).toBeCalledWith(new UrlQuery().UrlThumbnailJsonApi('hash'));

    component.unmount();
    console.log('-- Rotation API is called return 202 --');

  });

  xit("Rotation API is called return 200", () => {

    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({ statusCode: 200 } as IConnectionDefault);
    var spyGet = jest.spyOn(FetchGet, 'default').mockImplementationOnce(() => mockGetIConnectionDefault);

    var detailview = mount(<TestComponent />);

    expect(spyGet).toBeCalled();
    expect(spyGet).toBeCalledWith(new UrlQuery().UrlThumbnailJsonApi('hash'));
    detailview.unmount();
  });

});