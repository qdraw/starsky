export interface ITenantListItem {
  tenantId: string;
  displayName: string;
  roleCode: string;
}

export interface ITenantsResponse {
  /** tenant of the current url */
  current: string;
  /** the tenants the user is a member of */
  tenants: ITenantListItem[];
  lastUsed?: string | null;
  /** where a login without target goes, null: the user chooses (or has no access) */
  landing?: string | null;
}
