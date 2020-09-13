import { Link } from '@reach/router';
import { mount, shallow } from 'enzyme';
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

  it("when click on Link, it should display a preloader", () => {
    var fileIndexItem = {
      fileName: 'test',
      status: IExifStatus.Ok
    } as IFileIndexItem
    var component = mount(<ListImageBox item={fileIndexItem} />)
    component.find(Link).simulate('click', {
      metaKey: false
    });

    expect(component.exists('.preloader--overlay')).toBeTruthy();
  });

  it("when click on Link, with command key it should ignore preloader", () => {
    var fileIndexItem = {
      fileName: 'test',
      status: IExifStatus.Ok
    } as IFileIndexItem
    var component = mount(<ListImageBox item={fileIndexItem} />)
    component.find(Link).simulate('click', {
      metaKey: true
    });

    expect(component.exists('.preloader--overlay')).toBeFalsy();
  });
});