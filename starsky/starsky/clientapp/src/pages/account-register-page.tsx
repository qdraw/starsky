
import { RouteComponentProps } from '@reach/router';
import React, { FunctionComponent } from 'react';
import useGlobalSettings from '../hooks/use-global-settings';
import { Language } from '../shared/language';

const AccountRegisterPage: FunctionComponent<RouteComponentProps> = (props) => {

  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  const MessageApplicationName = "Starsky";
  const MessageCreateNewAccount = language.text("Maak nieuw account", "Create new account")
  const MessageUsername = language.text("E-mailadres", "E-mail address");
  const MessageExamplePassword = language.text("superveilig", "supersafe");
  const MessageExampleUsername = "dont@mail.me";
  const MessagePassword = language.text("Geef je wachtwoord op", "Enter your password");
  const MessageConfirmPassword = language.text("Vul je wachtwoord nog een keer in", "Enter your password again");

  const [userEmail, setUserEmail] = React.useState("");

  return (<>
    <header className="header header--main header--bluegray700">
      <div className="wrapper">
        <div className="item item--first item--detective">{MessageApplicationName}
        </div>
      </div>
    </header>

    <div className="content">
      <div className="content--header">{MessageCreateNewAccount}</div>

      <form method="post" action="/account/register"
        className="content--login-form form-inline form-nav">

        <label htmlFor="email">
          {MessageUsername}
        </label>
        <input
          className="form-control"
          autoComplete="off"
          type="email"
          name="email"
          value={userEmail}
          placeholder={MessageExampleUsername}
          onChange={e => setUserEmail(e.target.value)}
        />

        <label htmlFor="email">
          {MessagePassword}
        </label>
        <input
          className="form-control"
          autoComplete="off"
          type="password"
          name="password"
          placeholder={MessageExamplePassword}
        // value={userEmail}
        // onChange={e => setUserEmail(e.target.value)}
        />

        <label htmlFor="email">
          {MessageConfirmPassword}
        </label>
        <input
          className="form-control"
          autoComplete="off"
          type="password"
          name="password"
        // value={userEmail}
        // onChange={e => setUserEmail(e.target.value)}
        />


      </form>
    </div>
  </>)
}

export default AccountRegisterPage;
