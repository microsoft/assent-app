/* eslint-disable react/jsx-key */
import * as React from 'react';
import { Checkbox, IComboBox, IComboBoxOption, Label, Stack, VirtualizedComboBox } from '@fluentui/react';
import BasicButton from '../../../Controls/BasicButton';

export interface IFilterOption {
    label: string;
    checked: boolean;
}

interface IFilterColumnProps {
    columnCategory: { key: string; label: string };
    columnOptions: IFilterOption[];
    selectedKeys: string[];
    onChange: (
        ev: React.FormEvent<HTMLElement>,
        checked: boolean,
        columnCategory: { key: string; label: string },
        optionLabel: string
    ) => void;
    onClear: (columnCategory: { key: string; label: string }) => void;
    showDropdown: boolean;
}

const stackTokens = { childrenGap: 10 };

const isCollapsible = false;
const showClear = true;

// Case-insensitive, natural-numeric sort so long dropdowns are alphabetized ("10 USD" < "60 USD" < "100 USD").
const compareLabels = (a: IFilterOption, b: IFilterOption): number =>
    (a.label?.toString() ?? '').localeCompare(b.label?.toString() ?? '', undefined, {
        sensitivity: 'base',
        numeric: true,
    });

function renderComboboxDropdown(
    columnOptions: IFilterOption[],
    selectedKeys: string[],
    onChange: any,
    columnCategory: any,
    onInputValueChange: (text: string) => void,
    onMenuDismissed: () => void,
    componentRef: React.RefObject<IComboBox>
): JSX.Element {
    const comboOptions: IComboBoxOption[] = columnOptions.map((item, index) => ({
        key: item.label,
        text: item.label.toString(),
        selected: item.checked,
        styles: { optionText: { whiteSpace: 'normal' } },
    }));
    return (
        <VirtualizedComboBox
            componentRef={componentRef}
            label={columnCategory.label}
            aria-label={columnCategory.label}
            placeholder="Select values to filter"
            options={comboOptions}
            dropdownMaxWidth={200}
            useComboBoxAsMenuWidth
            multiSelect
            // allowFreeform keeps the ComboBox from rewriting typed input to a prefix match.
            allowFreeform
            autoComplete="off"
            selectedKey={selectedKeys}
            onInputValueChange={onInputValueChange}
            onMenuDismissed={onMenuDismissed}
            onFocus={(ev: React.FocusEvent<IComboBox>): void => {
                // Only auto-open when focus lands on the input; the caret click already toggles the menu.
                if ((ev.target as unknown as HTMLElement).tagName === 'INPUT') {
                    componentRef.current?.focus(true);
                }
            }}
            onChange={(
                event: React.FormEvent<IComboBox>,
                option?: IComboBoxOption,
                index?: number,
                value?: string
            ): void => {
                // With allowFreeform, option is undefined for unmatched typed text — ignore.
                if (!option) return;
                onChange(event, option.selected, columnCategory, option.key);
            }}
        />
    );
}

export function FilterColumn(props: IFilterColumnProps): React.ReactElement {
    const { columnOptions, columnCategory, onChange, onClear, showDropdown, selectedKeys } = props;
    const [query, setQuery] = React.useState('');
    const comboBoxRef = React.useRef<IComboBox>(null);
    const sortedOptions = [...columnOptions].sort(compareLabels);
    // Fluent v8 ComboBox has no built-in type-to-filter; filter its options prop ourselves.
    const q = query.trim().toLowerCase();
    const visibleOptions = q
        ? sortedOptions.filter((item) => item.label?.toString().toLowerCase().includes(q))
        : sortedOptions;
    return (
        <Stack tokens={stackTokens}>
            {!isCollapsible && !showDropdown && (
                <Stack.Item styles={{ root: { paddingLeft: '5%' } }}>
                    <Label id={'comboxbox-' + columnCategory.label?.replace(' ', '')}>{columnCategory.label}</Label>
                </Stack.Item>
            )}
            <Stack.Item>
                <div style={{ paddingLeft: '5%' }}>
                    <Stack tokens={{ childrenGap: 7 }}>
                        {!showDropdown &&
                            sortedOptions &&
                            sortedOptions.map((item, index) => (
                                <Stack.Item>
                                    <Checkbox
                                        label={item.label.toString()}
                                        checked={item.checked}
                                        onChange={(ev: React.FormEvent<HTMLElement>, checked: boolean) => {
                                            onChange(ev, checked, columnCategory, item.label);
                                        }}
                                    />
                                </Stack.Item>
                            ))}
                        {showDropdown &&
                            renderComboboxDropdown(
                                visibleOptions,
                                selectedKeys,
                                onChange,
                                columnCategory,
                                (text) => {
                                    setQuery(text ?? '');
                                    // allowFreeform suppresses click-to-open on the input; reopen the menu on type.
                                    comboBoxRef.current?.focus(true);
                                },
                                () => setQuery(''),
                                comboBoxRef
                            )}
                    </Stack>
                </div>
            </Stack.Item>
            {showClear && (
                <Stack.Item>
                    <BasicButton
                        primary={false}
                        text="Clear"
                        title="Clear"
                        onClick={() => {
                            onClear(columnCategory);
                        }}
                        styles={{
                            root: { border: 'none', marginLeft: '5%' },
                            label: {
                                color: '#0064C1',
                                selectors: {
                                    '@media (min-device-width: 1023px) and (min-width: 320px) and (max-width: 639px)': {
                                        fontSize: '12px',
                                    },
                                },
                            },
                            rootHovered: { backgroundColor: '#fafafa', border: 'none' },
                            rootFocused: { border: '1px solid rgb(177, 177, 177)' },
                        }}
                    />
                </Stack.Item>
            )}
        </Stack>
    );
}
