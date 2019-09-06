import React from 'react';
import ReactDOM from 'react-dom';
import { newIFileIndexItemArray } from '../interfaces/IFileIndexItem';
import ArchiveSidebarColorClass from './archive-sidebar-color-class';

describe('<Login /> with no props', () => {
  it('renders without crashing', () => {
    const div: HTMLDivElement = document.createElement('div');
    ReactDOM.render(<ArchiveSidebarColorClass fileIndexItems={newIFileIndexItemArray()} isReadOnly={false} />, div);
    console.log(div.innerHTML);

    ReactDOM.unmountComponentAtNode(div);
  });
});