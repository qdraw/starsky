import React from 'react';
import useGlobalSettings from '../../../hooks/use-global-settings';
import FetchPost from '../../../shared/fetch-post';
import { Language } from '../../../shared/language';
import { UrlQuery } from '../../../shared/url-query';
import ButtonStyled from '../../atoms/button-styled/button-styled';

export const PreferencesPassword: React.FunctionComponent<any> = (_) => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageChangePassword = language.text("Verander je wachtwoord", "Change your password");

  const MessageExamplePassword = language.text("superveilig", "supersafe");
  const MessageCurrentPassword = language.text("Geef je huidige wachtwoord op", "Enter your current password");
  const MessageChangedPassword = language.text("Geef je nieuwe wachtwoord op", "Enter your new password");
  const MessageChangedConfirmPassword = language.text("Herhaal je nieuwe wachtwoord", "And your new password again");
  const MessageNoPassword = language.text("Voer het huidige en nieuwe wachtwoord in", "Enter the current and new password");
  const MessagePasswordChanged = language.text("Je wachtwoord is succesvol veranderd", "Your password has been successfully changed");
  const MessagePasswordNoMatch = language.text("De wachtwoorden komen niet overeen", "The passwords do not match");
  const MessagePasswordModalError = language.text("Het nieuwe wachtwoord voldoet niet aan de criteria",
    "The new password does not meet the criteria");

  const [loading, setLoading] = React.useState(false);

  const useErrorHandler = (initialState: string | null) => { return initialState };
  const [error, setError] = React.useState(useErrorHandler(null));

  const [userCurrentPassword, setUserCurrentPassword] = React.useState("");
  const [userChangedPassword, setUserChangedPassword] = React.useState("");
  const [userChangedConfirmPassword, setUserChangedConfirmPassword] = React.useState("");

  function validateChangePassword(): boolean {
    if (!userCurrentPassword || !userChangedPassword || !userChangedConfirmPassword) {
      setError(MessageNoPassword)
      return false;
    }
    if (userChangedPassword !== userChangedConfirmPassword) {
      setError(MessagePasswordNoMatch)
      return false;
    }
    return true;
  }

  async function changeSecret() {
    setLoading(true);
    var bodyParams = new URLSearchParams();
    bodyParams.set("Password", userCurrentPassword);
    bodyParams.set("ChangedPassword", userChangedPassword);
    bodyParams.set("ChangedConfirmPassword", userChangedConfirmPassword);

    const response = await FetchPost(new UrlQuery().UrlAccountChangeSecret(), bodyParams.toString());
    setLoading(false);
    if (response.statusCode === 200 && response.data && response.data.success) {
      setError(MessagePasswordChanged)
      return;
    }
    if (response.statusCode === 401) {
      setError(MessageCurrentPassword)
      return;
    }
    setError(MessagePasswordModalError)
  }

  return <>
    <form
      className="preferences-password form-inline"
      onSubmit={async e => {
        e.preventDefault();
        setError(null);
        if (validateChangePassword()) {
          await changeSecret();
        }
      }}
    >
      <div className="content--subheader">{MessageChangePassword}</div>
      <div className="content--text">
        <label htmlFor="password">
          {MessageCurrentPassword}
        </label>
        <input
          className="form-control"
          type="password"
          name="password"
          maxLength={80}
          placeholder={MessageExamplePassword}
          value={userCurrentPassword}
          onChange={e => setUserCurrentPassword(e.target.value)}
        />

        <label htmlFor="changed-password">
          {MessageChangedPassword}
        </label>
        <input
          className="form-control"
          type="password"
          name="changed-password"
          maxLength={80}
          value={userChangedPassword}
          onChange={e => setUserChangedPassword(e.target.value)}
        />

        <label htmlFor="password">
          {MessageChangedConfirmPassword}
        </label>
        <input
          className="form-control"
          type="password"
          name="changed-confirm-password"
          maxLength={80}
          value={userChangedConfirmPassword}
          onChange={e => setUserChangedConfirmPassword(e.target.value)}
        />
        {error && <div className="warning-box">{error}</div>}

        <ButtonStyled className="btn btn--default" type="submit" disabled={loading} onClick={e => { }}>
          {loading ? "Loading..." : MessageChangePassword}
        </ButtonStyled>
      </div>
    </form>
  </>;
};

export default PreferencesPassword
