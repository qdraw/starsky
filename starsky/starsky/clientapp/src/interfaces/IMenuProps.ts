import { IDetailView } from './IDetailView';

export interface IMenuProps {
  isDetailMenu: boolean;
  parent?: string;
  detailView?: IDetailView; // used to toggle the delete button
}