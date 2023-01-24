export enum AccessibilityActionType {
    TOGGLE_KEYBOARD_COLUMN_RESIZING = 'TOGGLE_KEYBOARD_COLUMN_RESIZING',
}

export type AccessibilityAction = IToggleKeyboardColumnResizing;

export interface IToggleKeyboardColumnResizing {
    type: AccessibilityActionType.TOGGLE_KEYBOARD_COLUMN_RESIZING;
    isOn: boolean;
}
