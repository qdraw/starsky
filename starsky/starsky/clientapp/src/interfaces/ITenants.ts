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
  /** may create and delete tenants */
  isPlatformAdmin?: boolean;
}

/** TenantState of the API */
export enum TenantState {
  Provisioning = 0,
  Active = 1,
  Disabled = 2,
  Deleting = 3,
  Deleted = 4
}

export interface ITenantOverviewItem {
  tenantId: string;
  displayName: string;
  state: TenantState;
}

export interface ITenantMember {
  userId: number;
  name?: string | null;
  /** the e-mail address the user signs in with */
  identifier?: string | null;
  roleCode: string;
}
