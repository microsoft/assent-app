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

function renderComboboxDropdown(
    columnOptions: IFilterOption[],
    selectedKeys: string[],
    onChange: any,
    columnCategory: any
): JSX.Element {
    const comboOptions: IComboBoxOption[] = columnOptions.map((item, index) => ({
        key: item.label,
        text: item.label.toString(),
        selected: item.checked,
        styles: { optionText: { whiteSpace: 'normal' } },
    }));
    return (
        <VirtualizedComboBox
            label={columnCategory.label}
            aria-label={columnCategory.label}
            placeholder="Select values to filter"
            options={comboOptions}
            dropdownMaxWidth={200}
            useComboBoxAsMenuWidth
            multiSelect
            autoComplete="off"
            selectedKey={selectedKeys}
            onChange={(
                event: React.FormEvent<IComboBox>,
                option?: IComboBoxOption,
                index?: number,
                value?: string
            ): void => {
                onChange(event, option.selected, columnCategory, option.key);
            }}
        />
    );
}

export function FilterColumn(props: IFilterColumnProps): React.ReactElement {
    const { columnOptions, columnCategory, onChange, onClear, showDropdown, selectedKeys } = props;
    return (
        <Stack tokens={stackTokens}>
            {!isCollapsible && !showDropdown && (
                <Stack.Item styles={{ root: { paddingLeft: '8%' } }}>
                    <Label id={'comboxbox-' + columnCategory.label?.replace(' ', '')}>{columnCategory.label}</Label>
                </Stack.Item>
            )}
            <Stack.Item>
                <div style={{ paddingLeft: '8%' }}>
                    <Stack tokens={{ childrenGap: 7 }}>
                        {!showDropdown &&
                            columnOptions &&
                            columnOptions.map((item, index) => (
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
                        {showDropdown && renderComboboxDropdown(columnOptions, selectedKeys, onChange, columnCategory)}
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
                            root: { border: 'none', padding: 0 },
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
