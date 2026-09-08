import { IStackStyles } from '@fluentui/react/lib/Stack';
import { ITextStyles } from '@fluentui/react/lib/Text';
import { IButtonStyles } from '@fluentui/react/lib/Button';
import { ICheckboxStyles } from '@fluentui/react/lib/Checkbox';
import { IIconStyles } from '@fluentui/react/lib/Icon';
import { ICalloutContentStyles } from '@fluentui/react/lib/Callout';

// Styling for the "Export pending approvals" callout. Extracted verbatim from SecondaryHeader so the
// callout markup and its (previously inline) styles live alongside the component that renders them.

export const calloutStyles: Partial<ICalloutContentStyles> = {
    root: { width: 380, boxShadow: '0 8px 24px rgba(0,0,0,0.14)' },
};

export const headerStackStyles: IStackStyles = {
    root: { padding: '14px 16px 12px', borderBottom: '1px solid #EDEBE9' },
};

export const titleTextStyles: ITextStyles = {
    root: { fontWeight: 600, color: '#323130' },
};

export const closeButtonStyles: IButtonStyles = {
    root: { color: '#605E5C', height: 28, width: 28 },
};

export const bodyStackStyles: IStackStyles = {
    root: { padding: '16px 16px 8px' },
};

export const activeFilterLabelStyles: ITextStyles = {
    root: {
        fontWeight: 600,
        color: '#605E5C',
        textTransform: 'uppercase',
        letterSpacing: '0.5px',
        display: 'block',
        marginBottom: 8,
    },
};

// When the active filter is ignored (export-all or a pull tenant) the pill renders muted and struck through.
export const pillStackStyles = (filterIgnored: boolean): IStackStyles => ({
    root: {
        padding: '4px 10px',
        backgroundColor: filterIgnored ? '#F3F2F1' : '#EFF6FC',
        border: `1px solid ${filterIgnored ? '#E1DFDD' : '#C7E0F4'}`,
        borderRadius: 16,
    },
});

export const filterIconStyles = (filterIgnored: boolean): IIconStyles => ({
    root: { fontSize: 12, color: filterIgnored ? '#605E5C' : '#005A9E' },
});

export const pillTextStyles = (filterIgnored: boolean): ITextStyles => ({
    root: {
        color: filterIgnored ? '#605E5C' : '#005A9E',
        fontWeight: 600,
        textDecoration: filterIgnored ? 'line-through' : 'none',
    },
});

export const exportAllCheckboxStyles: ICheckboxStyles = {
    root: { marginTop: 12 },
};

export const scopeNoteTextStyles: ITextStyles = {
    root: { color: '#605E5C', display: 'block', marginTop: 10, minHeight: 32 },
};

export const footerStackStyles: IStackStyles = {
    root: { padding: '12px 16px 16px', borderTop: '1px solid #EDEBE9' },
};
