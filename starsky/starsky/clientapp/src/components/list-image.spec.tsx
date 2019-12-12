import { mount, shallow } from 'enzyme';
import React from 'react';
import useIntersection from '../hooks/use-intersection-observer';
import ListImage from './list-image';

jest.mock('../hooks/use-intersection-observer');

describe("ListImageTest", () => {

  it("renders", () => {
    shallow(<ListImage alt={'alt'} src={'src'}/>)
  });

  it('useIntersection = true', () => {
    (useIntersection as jest.Mock<any>).mockImplementation(() => (true));
    var element = mount(<ListImage src={'test.jpg'} />);
    element.find('img').simulate("load");

    expect(element.find('img').length).toBe(1);
    expect(element.find('img').filterWhere((item) => {
      return item.prop('src') === 'test.jpg';
    })).toHaveLength(1);
  });

  it('img-box--error null', () => {
    var element = shallow(<ListImage src={'null.jpg?issingleitem=true'} />);

    expect(element.filterWhere((item) => {
      return item.prop('className') === 'img-box--error';
    })).toHaveLength(1);
  });

});
