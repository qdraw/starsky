import { mount, shallow } from 'enzyme';
import React from 'react';
import useIntersection from '../hooks/use-intersection-observer';
import { ImageFormat } from '../interfaces/IFileIndexItem';
import { UrlQuery } from '../shared/url-query';
import ListImage from './list-image';

jest.mock('../hooks/use-intersection-observer');

describe("ListImageTest", () => {

  it("renders", () => {
    shallow(<ListImage alt={'alt'} fileHash={'src'} imageFormat={ImageFormat.jpg} />)
  });

  it('useIntersection = true', () => {
    (useIntersection as jest.Mock<any>).mockImplementation(() => (true));
    var element = mount(<ListImage fileHash={'test.jpg'} imageFormat={ImageFormat.jpg} />);
    element.find('img').simulate("load");

    expect(element.find('img').length).toBe(1);
    expect(element.find('img').filterWhere((item) => {
      return item.prop('src') === new UrlQuery().UrlThumbnailImage('test.jpg', true);
    })).toHaveLength(1);
  });

  it('img-box--error null', () => {
    var element = shallow(<ListImage imageFormat={ImageFormat.jpg} fileHash={'null'} />);

    expect(element.filterWhere((item) => {
      return item.prop('className') === 'img-box--error';
    })).toHaveLength(1);
  });

  it('img-box--error null', () => {
    var element = shallow(<ListImage imageFormat={ImageFormat.jpg} fileHash={'null'} />);

    expect(element.filterWhere((item) => {
      return item.prop('className') === 'img-box--error';
    })).toHaveLength(1);
  });

});
