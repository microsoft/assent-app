import { accessibilityInitialState, accessibilityReducerName } from './Accessibility.reducer';
import { IAccessibilityAppState } from './Accessibility.types';

export const getIsKeyboardColumnResizingOn = (state: IAccessibilityAppState): boolean => {
    return (
        state?.[accessibilityReducerName]?.isKeyboardColumnResizingOn ||
        accessibilityInitialState.isKeyboardColumnResizingOn
    );
};
