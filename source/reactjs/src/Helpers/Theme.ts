/* eslint-disable no-var */
import { createTheme, FontSizes } from '@fluentui/react';
import { DefaultPalette, Depths } from '@fluentui/theme';
var p = DefaultPalette;
var extendedSemanticColors = {
    headerText: p.neutralPrimary,
    navBackground: p.neutralQuaternaryAlt,
    navItemBackgroundHovered: p.neutralQuaternary,
    navItemBackgroundPressed: p.neutralTertiaryAlt,
    stepCompleted: p.themeDark,
    stepCurrent: p.themePrimary,
    stepNotStarted: p.neutralDark,
    stepModifierBorder: p.neutralDark,
    stepHover: p.themeDark,
    stepPressed: p.themeDarker,
    stepError: p.red,
    allStepsComplete: p.themePrimary,
    panelBackground: p.neutralLighter,
    bodyStandoutBackground: p.neutralLighter,
    bodyTextChecked: p.neutralLighter,
    disabledBodyText: p.neutralTertiaryAlt,
    variantBorder: p.neutralSecondary,
    variantBorderHovered: p.neutralSecondary,
    defaultStateBackground: p.white,
    link: p.themeDark,
    linkHovered: p.themeDarker,
    buttonBackgroundChecked: p.neutralLight,
    buttonBackgroundCheckedHovered: p.neutralQuaternary,
    buttonBorder: p.neutralSecondary,
    buttonTextHovered: p.neutralPrimary,
    buttonTextChecked: p.neutralPrimary,
    buttonTextCheckedHovered: p.neutralPrimary,
    buttonTextPressed: p.neutralPrimary,
    buttonTextDisabled: p.neutralTertiaryAlt,
    primaryButtonTextDisabled: p.neutralTertiaryAlt,
    accentButtonBackground: p.themePrimary,
    inputBorder: p.neutralSecondary,
    inputFocusBorderAlt: p.themeDarkAlt,
    inputTextHovered: p.neutralPrimary,
    disabledText: p.neutralTertiaryAlt,
    disabledSubtext: p.neutralTertiaryAlt,
    listItemBackgroundCheckedHovered: p.neutralQuaternary,
    menuDivider: p.neutralQuaternaryAlt,
    menuIcon: p.neutralPrimary,
    menuHeader: p.neutralPrimary,
    menuItemTextHovered: p.neutralPrimary,
};
export var CoherenceTheme = createTheme({
    palette: DefaultPalette,
    semanticColors: extendedSemanticColors,
    effects: {
        roundedCorner2: '2px',
        elevation4: Depths.depth4,
        elevation8: Depths.depth8,
        elevation16: Depths.depth16,
        elevation64: Depths.depth64,
    },
    fonts: {
        medium: {
            fontSize: FontSizes.medium,
        },
        mediumPlus: {
            fontSize: FontSizes.mediumPlus,
        },
    },
});
export default CoherenceTheme;
