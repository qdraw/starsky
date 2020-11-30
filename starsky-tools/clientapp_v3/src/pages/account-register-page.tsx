import { RouteComponentProps } from '@reach/router';
import React, { FunctionComponent } from 'react';
import AccountRegister from '../containers/account-register';

const AccountRegisterPage: FunctionComponent<RouteComponentProps> = (props) => {
  return (<AccountRegister />)
}

export default AccountRegisterPage;
