import { Link, RouteComponentProps } from '@reach/router';
import React, { FunctionComponent } from 'react';
import MenuDefault from '../components/menu-default';

const NotFoundPage: FunctionComponent<RouteComponentProps<any>> = () => {
  return (<div>
    <MenuDefault isEnabled={true}></MenuDefault>
    <div className="content">
      <div className="content--header"><Link to="/">Oeps niet gevonden</Link></div>
      <div className="content--subheader"><Link to="/"><u>Ga naar de homepagina</u></Link></div>
    </div>
  </div>)
}

export default NotFoundPage;
