import * as React from 'react';
import { Checkbox, ISearchBoxStyles, Label, SearchBox, Stack } from '@fluentui/react';
import { Panel } from '@fluentui/react/lib/Panel';
import { FilterColumn, IFilterOption } from './FilterColumn';
import * as Styled from '../SharedLayout';

interface IFilterCategory {
    key: string;
    label: string;
    filterOptions: IFilterOption[];
}

interface IFilterPanelProps {
    filterCategories: IFilterCategory[];
    onChange: (
        ev: React.FormEvent<HTMLElement>,
        checked: boolean,
        columnCategory: { key: string; label: string },
        optionLabel: string
    ) => void;
    onClear: (columnCategory: { key: string; label: string }) => void;
    isMobile?: boolean;
    onClosePanel?: () => void;
}

const stackTokens = { childrenGap: 15 };

const showSearch = true;

export function FilterPanel(props: IFilterPanelProps): React.ReactElement {
    const { filterCategories, onChange, onClear, onClosePanel, isMobile } = props;

    let searchRef: any = null;

    React.useEffect(() => {
        if (searchRef) {
            searchRef.focus();
        }
    }, [searchRef]);

    const [searchValue, setSearchValue] = React.useState<string>('');
    const filterMenus = filterCategories.map((item, index) => {
        let uniqueValues = item.filterOptions;
        uniqueValues = uniqueValues.filter(item => item.label.toLowerCase().includes(searchValue));
        return (
            uniqueValues.length > 0 &&
            uniqueValues[0].label && (
                <Stack.Item styles={Styled.StackStylesBottomBorder}>
                    <FilterColumn
                        columnCategory={{ key: item.key, label: item.label }}
                        columnOptions={uniqueValues}
                        onChange={(
                            ev: React.FormEvent<HTMLElement>,
                            checked: boolean,
                            columnCategory: { key: string; label: string },
                            optionLabel: string
                        ): void => {
                            onChange(ev, checked, columnCategory, optionLabel);
                        }}
                        onClear={(columnCategory: { key: string; label: string }): void => {
                            onClear(columnCategory);
                        }}
                        showDropdown={!searchValue}
                    />
                </Stack.Item>
            )
        );
    });

    const renderFilterContent = (): JSX.Element => {
        return (
            <Stack tokens={stackTokens}>
                {showSearch && (
                    <Stack.Item styles={{ root: { paddingLeft: '5%' } }}>
                        <SearchBox
                            placeholder="Keyword search"
                            onClear={ev => {
                                setSearchValue('');
                            }}
                            onChange={(_, newValue): void => setSearchValue(newValue?.toLowerCase())}
                            componentRef={(input: any): void => (searchRef = input)}
                        />
                    </Stack.Item>
                )}
                {filterMenus}
            </Stack>
        );
    };
    return isMobile ? (
        <Panel
            isOpen={true}
            onDismiss={onClosePanel}
            isLightDismiss={true}
            hasCloseButton={true}
            titleText={'Filter'}
            styles={{ header: { marginBottom: '10px', marginTop: 0 } }}
        >
            {renderFilterContent()}
        </Panel>
    ) : (
        <Styled.FilterContainer>{renderFilterContent()}</Styled.FilterContainer>
    );
}
