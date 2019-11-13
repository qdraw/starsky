export interface ILogin {
  Email: string;
  Password: string;
  RememberMe: boolean;
  submitted: boolean;
  loading: boolean;
  isError: boolean;
  errorMessage: string;
}

export function newILogin(): ILogin {
  return {} as ILogin;
}