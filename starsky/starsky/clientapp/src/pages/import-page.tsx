
import { RouteComponentProps } from '@reach/router';
import React, { FunctionComponent } from 'react';
import MenuSearch from '../components/menu-search';

const ImportPage: FunctionComponent<RouteComponentProps> = (props) => {
  return (<div>
    <MenuSearch></MenuSearch>
    <div className="content">
      <div className="content--header"><a href="/v1/import">Importeren werkt nog niet in V2</a></div>
      <div className="content--subheader"><a href="/v1/import"><u>Ga naar de oude weergave om te importeren</u></a></div>
    </div>
  </div>)
}

export default ImportPage;
