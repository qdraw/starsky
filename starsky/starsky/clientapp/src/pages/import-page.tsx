
import { RouteComponentProps } from '@reach/router';
import React, { FunctionComponent } from 'react';
import DropArea from '../components/drop-area';
import MenuSearch from '../components/menu-search';


const ImportPage: FunctionComponent<RouteComponentProps> = (props) => {

  return (<div>
    <MenuSearch></MenuSearch>
    <div className="content">
      <div className="content--header">Importeren</div>
      <div className="content--subheader"><a href="/v1/import"><u>Test functionaliteit: Ga naar oude weergave</u></a></div>
      <DropArea><></></DropArea>
    </div>
  </div>
  )
}

export default ImportPage;
