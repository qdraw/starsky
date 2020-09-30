import { shallow } from 'enzyme';
import React from 'react';
import { newIArchive } from '../../../interfaces/IArchive';
import { IArchiveProps } from '../../../interfaces/IArchiveProps';
import { newIFileIndexItemArray } from '../../../interfaces/IFileIndexItem';
import MenuOptionMoveToTrash from './menu-option-move-to-trash';

describe("MenuOptionMoveToTrash", () => {
  it("renders", () => {
    var test = { ...newIArchive(), fileIndexItems: newIFileIndexItemArray() } as IArchiveProps
    shallow(<MenuOptionMoveToTrash setSelect={jest.fn()} select={["test.jpg"]} isReadOnly={true} state={test} dispatch={jest.fn()} />)
  });

});
