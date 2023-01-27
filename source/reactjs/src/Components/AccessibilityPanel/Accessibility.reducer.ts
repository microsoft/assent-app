import { AccessibilityAction, AccessibilityActionType } from './Accessibility.action-types';
import { IAccessibilityState } from './Accessibility.types';

export const accessibilityReducerName = 'AccessibilityReducer';
export const accessibilityInitialState: IAccessibilityState = {
    isKeyboardColumnResizingOn: false,
};

export function accessibilityReducer(
    prev: IAccessibilityState = accessibilityInitialState,
    action: AccessibilityAction
): IAccessibilityState {
    switch (action.type) {
        case AccessibilityActionType.TOGGLE_KEYBOARD_COLUMN_RESIZING:
            return {
                ...prev,
                isKeyboardColumnResizingOn: action.isOn,
            };
        default:
            return prev;
    }
}
