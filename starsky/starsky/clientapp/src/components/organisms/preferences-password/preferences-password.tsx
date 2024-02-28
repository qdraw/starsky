import { useState } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import localization from "../../../localization/localization.json";
import FetchPost from "../../../shared/fetch/fetch-post";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url/url-query";
import ButtonStyled from "../../atoms/button-styled/button-styled";

const PreferencesPassword: React.FunctionComponent = () => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageChangePassword = language.key(localization.MessageChangedPassword);
  const MessageExamplePassword = language.key(localization.MessageExamplePassword);
  const MessageCurrentPassword = language.key(localization.MessageCurrentPassword);
  const MessageChangedPassword = language.key(localization.MessageChangedPassword);
  const MessageChangedConfirmPassword = language.key(localization.MessageChangedConfirmPassword);
  const MessageNoPassword = language.key(localization.MessageNoPassword);
  const MessagePasswordChanged = language.key(localization.MessagePasswordChanged);
  const MessagePasswordNoMatch = language.key(localization.MessagePasswordNoMatch);
  const MessagePasswordModalError = language.key(localization.MessagePasswordModalError);

  const [loading, setLoading] = useState(false);

  const useErrorHandler = (initialState: string | null) => {
    return initialState;
  };
  const [error, setError] = useState(useErrorHandler(null));

  const [userCurrentPassword, setUserCurrentPassword] = useState("");
  const [userChangedPassword, setUserChangedPassword] = useState("");
  const [userChangedConfirmPassword, setUserChangedConfirmPassword] = useState("");

  function validateChangePassword(): boolean {
    if (!userCurrentPassword || !userChangedPassword || !userChangedConfirmPassword) {
      setError(MessageNoPassword);
      return false;
    }
    if (userChangedPassword !== userChangedConfirmPassword) {
      setError(MessagePasswordNoMatch);
      return false;
    }
    return true;
  }

  async function changeSecret() {
    setLoading(true);
    const bodyParams = new URLSearchParams();
    bodyParams.set("Password", userCurrentPassword);
    bodyParams.set("ChangedPassword", userChangedPassword);
    bodyParams.set("ChangedConfirmPassword", userChangedConfirmPassword);

    const response = await FetchPost(
      new UrlQuery().UrlAccountChangeSecret(),
      bodyParams.toString()
    );
    setLoading(false);
    if (response.statusCode === 200 && response.data?.success) {
      setError(MessagePasswordChanged);
      return;
    }
    if (response.statusCode === 401) {
      setError(MessageCurrentPassword);
      return;
    }
    setError(MessagePasswordModalError);
  }

  return (
    <form
      className="preferences preferences--password form-inline"
      onSubmit={async (e) => {
        e.preventDefault();
        setError(null);
        if (validateChangePassword()) {
          await changeSecret();
        }
      }}
    >
      <div className="content--subheader">{MessageChangePassword}</div>
      <div className="content--text">
        <label htmlFor="password">{MessageCurrentPassword}</label>
        <input
          className="form-control"
          type="password"
          name="password"
          data-test="preferences-password-input"
          maxLength={80}
          placeholder={MessageExamplePassword}
          value={userCurrentPassword}
          onChange={(e) => setUserCurrentPassword(e.target.value)}
        />

        <label htmlFor="changed-password">{MessageChangedPassword}</label>
        <input
          className="form-control"
          type="password"
          name="changed-password"
          data-test="preferences-password-changed-input"
          maxLength={80}
          value={userChangedPassword}
          onChange={(e) => setUserChangedPassword(e.target.value)}
        />

        <label htmlFor="password">{MessageChangedConfirmPassword}</label>
        <input
          className="form-control"
          type="password"
          name="changed-confirm-password"
          data-test="preferences-password-changed-confirm-input"
          maxLength={80}
          value={userChangedConfirmPassword}
          onChange={(e) => setUserChangedConfirmPassword(e.target.value)}
        />
        {error && (
          <div data-test="preferences-password-warning" className="warning-box">
            {error}
          </div>
        )}

        <ButtonStyled
          className="btn btn--default"
          type="submit"
          data-test="preferences-password-submit"
          disabled={loading}
        >
          {loading ? "Loading..." : MessageChangePassword}
        </ButtonStyled>
      </div>
    </form>
  );
};

export default PreferencesPassword;
