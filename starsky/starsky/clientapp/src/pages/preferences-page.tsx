import { RouteComponentProps } from '@reach/router';
import React, { FunctionComponent } from 'react';
import Preferences from '../containers/preferences';

const PreferencesPage: FunctionComponent<RouteComponentProps> = (props) => {
  return (<Preferences />)
}

export default PreferencesPage;
