import { shallow } from 'enzyme';
import React from 'react';
import { newIFileIndexItem } from '../interfaces/IFileIndexItem';
import ListImageBox from './list-image-box';

describe("ListImageTest", () => {
  it("renders", () => {
    shallow(<ListImageBox item={newIFileIndexItem()} />)
  });
});