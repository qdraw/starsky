import { shallow } from 'enzyme';
import React from 'react';
import ModalDetailviewRenameFile from './modal-detailview-rename-file';

describe("ModalDetailviewRenameFile", () => {

  it("renders", () => {
    shallow(<ModalDetailviewRenameFile
      isOpen={true}
      handleExit={() => { }}>
      test
    </ModalDetailviewRenameFile>)
  });


});