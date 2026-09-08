import * as React from 'react';
import { DirectionalHint, FocusTrapCallout } from '@fluentui/react/lib/Callout';
import { Stack } from '@fluentui/react/lib/Stack';
import { Text } from '@fluentui/react/lib/Text';
import { IconButton, PrimaryButton, DefaultButton } from '@fluentui/react/lib/Button';
import { Icon } from '@fluentui/react/lib/Icon';
import { Checkbox } from '@fluentui/react/lib/Checkbox';
import * as Styled from './ExportSummaryCallout.styles';

interface IExportSummaryCalloutProps {
    /** Whether the active filter is being ignored (export-all or a pull tenant), which mutes the filter pill. */
    filterIgnored: boolean;
    /** A pull-tenant filter can never be exported; hides the "export all" checkbox. */
    isFilterPullTenant: boolean;
    /** Human-readable label for the active grouping dimension (e.g. "Submitter", "Date"). */
    filterTypeLabel: string;
    /** Currently selected filter value. */
    filterValue: string;
    /** Whether the user opted to ignore the current filter and export all records. */
    exportAll: boolean;
    /** Descriptive note explaining exactly which records will be exported. */
    exportScopeNote: React.ReactNode;
    /** Called when the "export all" checkbox toggles. */
    onExportAllChange: (checked: boolean) => void;
    /** Called when the user confirms the export. */
    onExport: () => void;
    /** Called when the callout is dismissed or cancelled. */
    onDismiss: () => void;
}

/**
 * Presentational callout for the "Export pending approvals" flow. Extracted from SecondaryHeader; it owns
 * no state and simply renders the export scope UI, delegating all decisions back to the parent via props.
 * Anchored to the export button (`#exportPendingApprovals`) rendered by SecondaryHeader.
 */
function ExportSummaryCallout(props: IExportSummaryCalloutProps): React.ReactElement {
    const {
        filterIgnored,
        isFilterPullTenant,
        filterTypeLabel,
        filterValue,
        exportAll,
        exportScopeNote,
        onExportAllChange,
        onExport,
        onDismiss,
    } = props;

    return (
        <FocusTrapCallout
            target="#exportPendingApprovals"
            onDismiss={onDismiss}
            directionalHint={DirectionalHint.bottomRightEdge}
            directionalHintFixed
            isBeakVisible={false}
            setInitialFocus
            role="dialog"
            ariaLabelledBy="export-callout-title"
            focusTrapProps={{ isClickableOutsideFocusTrap: true, forceFocusInsideTrap: true }}
            styles={Styled.calloutStyles}
        >
            <Stack tokens={{ childrenGap: 0 }}>
                {/* Callout header */}
                <Stack
                    horizontal
                    horizontalAlign="space-between"
                    verticalAlign="center"
                    styles={Styled.headerStackStyles}
                >
                    <Text id="export-callout-title" variant="mediumPlus" styles={Styled.titleTextStyles}>
                        Export pending approvals
                    </Text>
                    <IconButton
                        iconProps={{ iconName: 'Cancel' }}
                        ariaLabel="Close export panel"
                        onClick={onDismiss}
                        styles={Styled.closeButtonStyles}
                    />
                </Stack>

                {/* Body */}
                <Stack tokens={{ childrenGap: 16 }} styles={Styled.bodyStackStyles}>
                    {/* Active filter shown as an informational pill/tag */}
                    <Stack.Item>
                        <Text variant="smallPlus" styles={Styled.activeFilterLabelStyles}>
                            Active Filter
                        </Text>
                        <Stack horizontal verticalAlign="center" wrap tokens={{ childrenGap: 8 }}>
                            <Stack
                                horizontal
                                verticalAlign="center"
                                tokens={{ childrenGap: 6 }}
                                styles={Styled.pillStackStyles(filterIgnored)}
                            >
                                <Icon iconName="Filter" styles={Styled.filterIconStyles(filterIgnored)} />
                                <Text variant="small" styles={Styled.pillTextStyles(filterIgnored)}>
                                    {filterTypeLabel}: {filterValue}
                                </Text>
                            </Stack>
                        </Stack>
                        {!isFilterPullTenant && (
                            <Checkbox
                                label="Export all records (ignore the current filter)"
                                checked={exportAll}
                                onChange={(_, checked) => onExportAllChange(!!checked)}
                                styles={Styled.exportAllCheckboxStyles}
                            />
                        )}
                        <Text variant="small" styles={Styled.scopeNoteTextStyles}>
                            {exportScopeNote}
                        </Text>
                    </Stack.Item>
                </Stack>

                {/* Footer */}
                <Stack horizontal horizontalAlign="end" tokens={{ childrenGap: 8 }} styles={Styled.footerStackStyles}>
                    <PrimaryButton
                        text="Export"
                        iconProps={{ iconName: 'Download' }}
                        onClick={onExport}
                        ariaLabel="Export pending approvals"
                    />
                    <DefaultButton text="Cancel" onClick={onDismiss} ariaLabel="Cancel export" />
                </Stack>
            </Stack>
        </FocusTrapCallout>
    );
}

export default ExportSummaryCallout;
