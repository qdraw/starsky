import { mount, shallow } from 'enzyme';
import React from 'react';
import { act } from 'react-dom/test-utils';
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

  it("Rotation API is called return 202", async () => {
    console.log('-- Rotation API is called return 202 --');

    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 202, data: {
        subPath: "/test/image.jpg",
        pageType: PageType.DetailView,
        fileIndexItem: { orientation: Orientation.Rotate270Cw, fileHash: 'needed', status: IExifStatus.Ok, filePath: "/test/image.jpg", fileName: "image.jpg" }
      } as IDetailView
    } as IConnectionDefault);

    let detectRotationSpy = jest.spyOn(DetectAutomaticRotation, 'default').mockImplementationOnce(() => {
      return Promise.resolve(false);
    });

    var spyGet = jest.spyOn(FetchGet, 'default')
      .mockImplementationOnce(() => mockGetIConnectionDefault)

    // need to await here
    var component = mount(<></>);
    await act(async () => {
      component = await mount(<FileHashImage isError={false} fileHash="hash" orientation={Orientation.Horizontal} />);
    })

    await expect(detectRotationSpy).toBeCalled();

    expect(spyGet).toBeCalledWith(new UrlQuery().UrlThumbnailJsonApi('hash'));

    component.unmount();
  });

  it("Rotation API is called return 200", async () => {
    console.log('-- Rotation API is called return 200 --');

    const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 200, data: {
        subPath: "/test/image.jpg",
        pageType: PageType.DetailView,
        fileIndexItem: { orientation: Orientation.Rotate270Cw, fileHash: 'needed', status: IExifStatus.Ok, filePath: "/test/image.jpg", fileName: "image.jpg" }
      } as IDetailView
    } as IConnectionDefault);

    let detectRotationSpy = jest.spyOn(DetectAutomaticRotation, 'default').mockImplementationOnce(() => {
      return Promise.resolve(false);
    });

    var spyGet = jest.spyOn(FetchGet, 'default')
      .mockImplementationOnce(() => mockGetIConnectionDefault)

    // need to await here
    var component = mount(<></>);
    await act(async () => {
      component = await mount(<FileHashImage isError={false} fileHash="hash" orientation={Orientation.Horizontal} />);
    })

    await expect(detectRotationSpy).toBeCalled();

    expect(spyGet).toBeCalledWith(new UrlQuery().UrlThumbnailJsonApi('hash'));

    component.unmount();
  });

});