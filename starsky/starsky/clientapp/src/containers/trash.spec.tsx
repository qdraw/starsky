import { shallow } from "enzyme";
import React from 'react';
import { newIArchive } from '../interfaces/IArchive';
import Trash from './trash';

describe("Trash", () => {
  it("renders", () => {
    shallow(<Trash {...newIArchive()} />)
  });
});