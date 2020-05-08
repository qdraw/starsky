import { mount, shallow } from "enzyme";
import React from 'react';
import * as HealthStatusError from '../components/molecules/health-status-error/health-status-error';
import * as useFileList from '../hooks/use-filelist';
import MediaContent from './media-content';

describe("MediaContent", () => {
  it("renders", () => {
    shallow(<MediaContent />)
  });
  it("application failed", () => {
    // use this import => import * as useFileList from '../hooks/use-filelist';
    jest.spyOn(useFileList, 'default').mockImplementationOnce(() => {
      return null;
    })

    // use ==> import * as HealthStatusError from '../components/health-status-error'; 
    jest.spyOn(HealthStatusError, 'default').mockImplementationOnce(() => {
      return null;
    }).mockImplementationOnce(() => {
      return null;
    });

    var result = mount(<MediaContent />);
    expect(result.html()).toBe('<br>The application failed');
  });

});