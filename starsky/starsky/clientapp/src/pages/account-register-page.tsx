
import { RouteComponentProps } from '@reach/router';
import React, { FunctionComponent } from 'react';
import Button from '../components/Button';
import useGlobalSettings from '../hooks/use-global-settings';
import useLocation from '../hooks/use-location';
import FetchPost from '../shared/fetch-post';
import { Language } from '../shared/language';
import { UrlQuery } from '../shared/url-query';
import { validateLoginForm } from '../shared/validate-login-form';

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
  const MessageNoUsernamePassword = language.text("Voer een emailadres en een wachtwoord in", "Enter an email address and password");
  const MessageWrongFormatEmailAddress = language.text("Controleer je email adres", "Check your email address");
  const MessagePasswordToShort = language.text("Gebruik minimaal 8 tekens voor je wachtwoord", "Use at least 8 characters for your password");
  const MessagePasswordNoMatch = language.text("Deze wachtwoorden komen niet overeen. Probeer het opnieuw", "These passwords do not match. Please try again");
  const MessageConnection = language.text("Er is geen verbinding mogelijk, probeer het later opnieuw", "No connection is possible, please try again later");
  const MessageRejectedBadRequest = language.text("Dit verzoek is afgewezen aangezien er niet voldaan is aan de beveiligingseisen (Error 400)", "This request was rejected because the security requirements were not met  (Error 400)");

  const MessageLegalCreateAccountHtml = language.text(`Door het creëren van een account gaat u akkoord met de
   <a href="/legal/toc.nl.html">Algemene Voorwaarden</a> van Starsky. Raadpleeg en bekijk hier onze 
   <a href="/legal/privacy-policy.nl.html">Privacykennisgeving</a> en onze <a href="/legal/privacy-policy.nl.html#cookie">Cookieverklaring</a>.`,
    `By creating an account you agree to <a href="/legal/toc.en.html">Starsky's Conditions of Use</a>. 
   Please see our  <a href="/legal/privacy-policy.en.html">Privacy</a> Notice and our <a href="/legal/privacy-policy.en.html#cookie">Cookies Notice </a>   `)

  var history = useLocation();

  const [userEmail, setUserEmail] = React.useState("");
  const [userPassword, setUserPassword] = React.useState("");
  const [userConfirmPassword, setUserConfirmPassword] = React.useState("");

  const useErrorHandler = (initialState: string | null) => { return initialState };
  const [error, setError] = React.useState(useErrorHandler(null));

  const [loading, setLoading] = React.useState(false);

  const setUpAccountHandler = async () => {
    setLoading(true);

    const response = await FetchPost(new UrlQuery().UrlAccountRegister(),
      `Email=${userEmail}&Password=${userPassword}&ConfirmPassword=${userConfirmPassword}`);
    if (response.statusCode === 400) {
      setError(MessageRejectedBadRequest);
      setLoading(false);
      return;
    }

    if (!response || !response.data) {
      setError(MessageConnection);
      setLoading(false);
      return;
    }

    history.navigate(new UrlQuery().UrlLogin(), { replace: true });
  };

  return (<>
    <header className="header header--main header--bluegray700">
      <div className="wrapper">
        <div className="item item--first item--detective">{MessageApplicationName}
        </div>
      </div>
    </header>

    <div className="content">
      <div className="content--header">{MessageCreateNewAccount}</div>

      <form method="post"
        className="content--login-form form-inline form-nav"
        onSubmit={e => {
          e.preventDefault();
          setError(null);
          var loginValidation = validateLoginForm(userEmail, userPassword);

          if (!loginValidation) {
            setError(loginValidation === null ? MessageWrongFormatEmailAddress : MessageNoUsernamePassword)
            return;
          }
          if (userPassword.length <= 7) {
            setError(MessagePasswordToShort);
            return;
          }
          if (userPassword !== userConfirmPassword) {
            setError(MessagePasswordNoMatch);
            return;
          }
          setUpAccountHandler();
        }}>

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
          value={userPassword}
          onChange={e => setUserPassword(e.target.value)}
        />

        <label htmlFor="email">
          {MessageConfirmPassword}
        </label>
        <input
          className="form-control"
          autoComplete="off"
          type="password"
          name="password"
          value={userConfirmPassword}
          onChange={e => setUserConfirmPassword(e.target.value)}
        />
        <div className="legal-text-row" dangerouslySetInnerHTML={{ __html: MessageLegalCreateAccountHtml }}></div>

        {error && <div className="content--error-true">{error}</div>}

        <Button className="btn btn--default" type="submit" disabled={loading} onClick={e => { }}>
          {loading ? "Loading..." : MessageCreateNewAccount}
        </Button>
      </form>
    </div>
  </>)
}

export default AccountRegisterPage;
