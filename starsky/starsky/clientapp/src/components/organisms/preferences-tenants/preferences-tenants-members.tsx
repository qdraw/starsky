import React, { useCallback, useEffect, useState } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { ITenantMember } from "../../../interfaces/ITenants";
import localization from "../../../localization/localization.json";
import FetchGet from "../../../shared/fetch/fetch-get";
import FetchPost from "../../../shared/fetch/fetch-post";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url/url-query";
import ButtonStyled from "../../atoms/button-styled/button-styled";

const Roles = ["User", "Administrator"];

const JsonHeaders = { "Content-Type": "application/json" };

/**
 * Members of the current tenant: only rendered for administrators of that tenant
 */
const PreferencesTenantsMembers: React.FunctionComponent = () => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageTenantMembers = language.key(localization.MessageTenantMembers);
  const MessageTenantAddMember = language.key(localization.MessageTenantAddMember);
  const MessageTenantMemberEmail = language.key(localization.MessageTenantMemberEmail);
  const MessageTenantRemoveMember = language.key(localization.MessageTenantRemoveMember);
  const MessageTenantMemberNotFound = language.key(localization.MessageTenantMemberNotFound);
  const MessageTenantMemberConflict = language.key(localization.MessageTenantMemberConflict);
  const MessageTenantMemberError = language.key(localization.MessageTenantMemberError);
  const MessageRole = language.key(localization.MessageRole);

  const [members, setMembers] = useState<ITenantMember[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [identifier, setIdentifier] = useState("");
  const [role, setRole] = useState(Roles[0]);
  const [loading, setLoading] = useState(false);

  const load = useCallback(async () => {
    const response = await FetchGet(new UrlQuery().UrlTenantsMembers());
    if (response.statusCode === 200 && Array.isArray(response.data)) {
      setMembers(response.data as ITenantMember[]);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  const showResult = (statusCode: number): boolean => {
    if (statusCode === 200) {
      setError(null);
      return true;
    }
    if (statusCode === 404) {
      setError(MessageTenantMemberNotFound);
    } else if (statusCode === 409) {
      setError(MessageTenantMemberConflict);
    } else {
      setError(MessageTenantMemberError);
    }
    return false;
  };

  const run = async (call: () => Promise<{ statusCode: number }>) => {
    setLoading(true);
    const response = await call();
    setLoading(false);
    const success = showResult(response.statusCode);
    await load();
    return success;
  };

  const addMember = async () => {
    const success = await run(() =>
      FetchPost(
        new UrlQuery().UrlTenantsMembers(),
        JSON.stringify({ identifier: identifier.trim(), role }),
        "post",
        JsonHeaders
      )
    );
    if (success) setIdentifier("");
  };

  const changeRole = (member: ITenantMember, newRole: string) =>
    run(() =>
      FetchPost(
        new UrlQuery().UrlTenantsMember(member.userId),
        JSON.stringify({ role: newRole }),
        "put",
        JsonHeaders
      )
    );

  const removeMember = (member: ITenantMember) =>
    run(() => FetchPost(new UrlQuery().UrlTenantsMember(member.userId), "", "delete"));

  return (
    <div className="preferences preferences--tenant-members" data-test="preferences-tenant-members">
      <div className="content--subheader">{MessageTenantMembers}</div>
      <table className="preferences-tenant-members__table">
        <tbody>
          {members.map((member) => (
            <tr key={member.userId} data-test={`preferences-tenant-member-${member.userId}`}>
              <td className="content--text">{member.identifier ?? member.name}</td>
              <td>
                <select
                  className="form-control"
                  aria-label={MessageRole}
                  data-test={`preferences-tenant-member-role-${member.userId}`}
                  value={member.roleCode}
                  disabled={loading}
                  onChange={(e) => void changeRole(member, e.target.value)}
                >
                  {Roles.map((roleCode) => (
                    <option key={roleCode} value={roleCode}>
                      {roleCode}
                    </option>
                  ))}
                </select>
              </td>
              <td>
                <button
                  className="btn btn--default"
                  type="button"
                  data-test={`preferences-tenant-member-remove-${member.userId}`}
                  disabled={loading}
                  onClick={() => void removeMember(member)}
                >
                  {MessageTenantRemoveMember}
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <form
        className="form-inline"
        onSubmit={(e) => {
          e.preventDefault();
          if (identifier.trim()) void addMember();
        }}
      >
        <div className="content--subheader">{MessageTenantAddMember}</div>
        <div className="content--text">
          <label htmlFor="tenant-member-email">{MessageTenantMemberEmail}</label>
          <input
            className="form-control"
            id="tenant-member-email"
            type="email"
            maxLength={80}
            spellCheck={false}
            data-test="preferences-tenant-member-email"
            value={identifier}
            onChange={(e) => setIdentifier(e.target.value)}
          />
          <select
            className="form-control"
            aria-label={MessageRole}
            data-test="preferences-tenant-member-new-role"
            value={role}
            onChange={(e) => setRole(e.target.value)}
          >
            {Roles.map((roleCode) => (
              <option key={roleCode} value={roleCode}>
                {roleCode}
              </option>
            ))}
          </select>
          {error && (
            <div data-test="preferences-tenant-member-warning" className="warning-box">
              {error}
            </div>
          )}
          <ButtonStyled
            className="btn btn--default"
            type="submit"
            data-test="preferences-tenant-member-add"
            disabled={loading || !identifier.trim()}
          >
            {MessageTenantAddMember}
          </ButtonStyled>
        </div>
      </form>
    </div>
  );
};

export default PreferencesTenantsMembers;
