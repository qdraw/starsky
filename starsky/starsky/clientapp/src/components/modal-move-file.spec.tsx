import { mount, shallow } from 'enzyme';
import React from 'react';
import ModalMoveFile from './modal-move-file';

describe("ModalMoveFile", () => {

  it("renders", () => {
    shallow(<ModalMoveFile parentDirectory="/" selectedSubPath="/test.jpg" isOpen={true} handleExit={() => { }}></ModalMoveFile>)
  });

  it("renders", () => {
    var result = mount(<ModalMoveFile parentDirectory="/" selectedSubPath="/test.jpg" isOpen={true} handleExit={() => { }}></ModalMoveFile>)

    console.log(result.html());

  });

});