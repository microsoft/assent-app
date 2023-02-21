import { AccessibilityActionType, IToggleKeyboardColumnResizing } from './Accessibility.action-types';

export function toggleKeyboardColumnResizing(isOn: boolean): IToggleKeyboardColumnResizing {
    return {
        type: AccessibilityActionType.TOGGLE_KEYBOARD_COLUMN_RESIZING,
        isOn,
    };
}
