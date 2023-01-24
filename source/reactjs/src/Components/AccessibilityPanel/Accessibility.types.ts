import { IDefaultState } from '@micro-frontend-react/employee-experience/lib/IDefaultState';
import { accessibilityReducerName } from './Accessibility.reducer';

export interface IAccessibilityAppState extends IDefaultState {
    [accessibilityReducerName]: IAccessibilityState;
}

export interface IAccessibilityState {
    isKeyboardColumnResizingOn: boolean;
}
