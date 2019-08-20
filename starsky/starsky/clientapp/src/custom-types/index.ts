export type UserAuth = {
  login: boolean;
  username: string;
  connectionError?: boolean;
};

export type ToDoItemType = { id: string; toDo?: string; complete?: boolean };

export enum ActionType {
  add = "ADD",
  delete = "DELETE",
  updateStatus = "UPDATE"
}
