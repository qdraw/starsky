import { Link, RouteComponentProps } from '@reach/router';
import React, { FunctionComponent } from 'react';
import MenuSearch from '../components/menu-search';

const NotFoundPage: FunctionComponent<RouteComponentProps<any>> = () => {
  return (<div>
    <MenuSearch></MenuSearch>
    <div className="content">
      <div className="content--header"><Link to="/">Oeps niet gevonden</Link></div>
      <div className="content--subheader"><Link to="/"><u>Ga naar de homepagina</u></Link></div>
    </div>
  </div>)
}

export default NotFoundPage;
