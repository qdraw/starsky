import { shallow } from 'enzyme';
import React from 'react';
import { IExifStatus } from '../../../interfaces/IExifStatus';
import { IFileIndexItem } from '../../../interfaces/IFileIndexItem';
import ListImageBox from './list-image-box';

describe("ListImageTest", () => {
  it("renders", () => {
    var fileIndexItem = {
      fileName: 'test',
      status: IExifStatus.Ok
    } as IFileIndexItem
    shallow(<ListImageBox item={fileIndexItem} />)
  });
});