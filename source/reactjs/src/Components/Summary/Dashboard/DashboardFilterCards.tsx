import * as React from 'react';
import { Stack } from '@fluentui/react/lib/Stack';
import { Text } from '@fluentui/react/lib/Text';
import { IDashboardFilters } from '../Models/ISummaryData';
import { IStackTokens } from '@fluentui/react';
import { useHistory } from 'react-router-dom';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { initiateSearch, updatePanelState } from '../../Shared/SharedComponents.actions';
import { getUserAlias } from '../../Shared/SharedComponents.persistent-selectors';
import { NavigationUtils } from '../../Shared/Utils/NavigationUtils';

interface IDashboardFilterCardsProps {
    recentSubmissionsCount: number;
    errorCount: number;
    highPriorityRequestsCount?: number; // Optional, can be undefined to show without number
    activeFilters: IDashboardFilters;
    onFilterToggle: (filterKey: keyof IDashboardFilters) => void;
    isPanelOpen?: boolean;
    isSearchResultsViewOpen?: boolean;
}

const FilterCard = styled.div<{ isActive: boolean; isMinimized?: boolean; isDisabled?: boolean }>`
    cursor: ${(props) => (props.isDisabled ? 'not-allowed' : 'pointer')};
    padding: ${(props) => (props.isMinimized ? '4px 8px' : '16px 20px')};
    border-radius: ${(props) => (props.isMinimized ? '12px' : '4px')};
    border: ${(props) => {
        if (props.isDisabled) {
            return '1px solid #e1e1e1';
        }
        if (props.isMinimized) {
            return props.isActive ? '1px solid #0078d4' : '1px solid #e1e1e1';
        }
        return props.isActive ? '2px solid #0078d4' : '1px solid #e1e1e1';
    }};
    background: ${(props) => (props.isDisabled ? '#f5f5f5' : props.isActive ? '#f3f9ff' : '#ffffff')};
    transition: all 0.2s ease;
    min-width: ${(props) => (props.isMinimized ? '60px' : '160px')};
    height: ${(props) => (props.isMinimized ? '24px' : 'auto')};
    box-shadow: ${(props) => {
        if (props.isDisabled) {
            return '0 1px 2px rgba(0, 0, 0, 0.04)';
        }
        if (props.isMinimized) {
            return props.isActive ? '0 2px 8px rgba(0, 120, 212, 0.1)' : '0 1px 2px rgba(0, 0, 0, 0.04)';
        }
        return props.isActive ? '0 4px 16px rgba(0, 120, 212, 0.15)' : '0 1px 3px rgba(0, 0, 0, 0.04)';
    }};
    opacity: ${(props) => (props.isDisabled ? '0.5' : '1')};
    position: relative;
    overflow: hidden;
    display: flex;
    align-items: center;
    justify-content: center;

    &::before {
        content: '';
        position: absolute;
        top: 0;
        left: 0;
        right: 0;
        height: ${(props) => (props.isMinimized || props.isDisabled ? '0px' : '3px')};
        background: ${(props) => (props.isActive && !props.isDisabled ? '#0078d4' : 'transparent')};
        transition: all 0.2s ease;
    }

    &:hover {
        box-shadow: ${(props) => {
            if (props.isDisabled) {
                return '0 1px 2px rgba(0, 0, 0, 0.04)';
            }
            if (props.isMinimized) {
                return props.isActive ? '0 4px 12px rgba(0, 120, 212, 0.15)' : '0 2px 8px rgba(0, 0, 0, 0.08)';
            }
            return props.isActive ? '0 6px 20px rgba(0, 120, 212, 0.2)' : '0 4px 16px rgba(0, 0, 0, 0.12)';
        }};
        transform: ${(props) => (props.isDisabled ? 'none' : props.isMinimized ? 'scale(1.02)' : 'translateY(-2px)')};
        border-color: ${(props) => (props.isDisabled ? '#e1e1e1' : props.isActive ? '#0078d4' : '#b3b3b3')};
        background: ${(props) => (props.isDisabled ? '#f5f5f5' : props.isActive ? '#f3f9ff' : '#f8f8f8')};

        &::before {
            background: ${(props) =>
                props.isDisabled ? 'transparent' : props.isActive ? '#0078d4' : 'rgba(0, 120, 212, 0.5)'};
        }
    }

    &:active {
        transform: ${(props) => (props.isDisabled ? 'none' : props.isMinimized ? 'scale(0.98)' : 'translateY(-1px)')};
        transition: all 0.1s ease;
        box-shadow: ${(props) => {
            if (props.isMinimized) {
                return props.isActive ? '0 1px 4px rgba(0, 120, 212, 0.1)' : '0 1px 4px rgba(0, 0, 0, 0.04)';
            }
            return props.isActive ? '0 2px 8px rgba(0, 120, 212, 0.15)' : '0 2px 8px rgba(0, 0, 0, 0.08)';
        }};
    }
`;

export const DashboardFilterCards: React.FunctionComponent<IDashboardFilterCardsProps> = ({
    recentSubmissionsCount,
    errorCount,
    highPriorityRequestsCount,
    activeFilters,
    onFilterToggle,
    isPanelOpen = false,
    isSearchResultsViewOpen = false,
}) => {
    const { useSelector, dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const history = useHistory();
    const userAlias = useSelector(getUserAlias);
    const cardTokens: IStackTokens = { childrenGap: 12 };

    const handleHighPrioritySearch = React.useCallback((): void => {
        const searchQuery = 'What are my 5 highest priority requests?';

        // Use NavigationUtils for consistent navigation logic
        NavigationUtils.performSearch(history, searchQuery);

        dispatch(updatePanelState(false));
        dispatch(initiateSearch(userAlias, searchQuery));
        onFilterToggle('highPriorityRequests');
    }, [history, userAlias, dispatch, onFilterToggle]);

    return (
        <Stack horizontal wrap tokens={isPanelOpen ? { childrenGap: 4 } : cardTokens}>
            {/* Recent Submissions Card */}
            <FilterCard
                isActive={activeFilters.recentSubmissions}
                isMinimized={isPanelOpen}
                onClick={() => onFilterToggle('recentSubmissions')}
                title={isPanelOpen ? `Recent Submissions: ${recentSubmissionsCount}` : undefined}
            >
                {isPanelOpen ? (
                    <Text
                        variant="small"
                        styles={{
                            root: {
                                fontWeight: '600',
                                color: activeFilters.recentSubmissions ? '#0078d4' : '#242424',
                                lineHeight: '1',
                                fontSize: '12px',
                            },
                        }}
                    >
                        {recentSubmissionsCount}
                    </Text>
                ) : (
                    <Stack tokens={{ childrenGap: 6 }}>
                        <Text
                            variant="xLarge"
                            styles={{
                                root: {
                                    fontWeight: '600',
                                    color: activeFilters.recentSubmissions ? '#0078d4' : '#242424',
                                    lineHeight: '1.2',
                                },
                            }}
                        >
                            {recentSubmissionsCount}
                        </Text>
                        <Text
                            variant="medium"
                            styles={{
                                root: {
                                    color: activeFilters.recentSubmissions ? '#106ebe' : '#616161',
                                    fontWeight: '400',
                                    lineHeight: '1.3',
                                },
                            }}
                        >
                            Recent Submissions
                        </Text>
                    </Stack>
                )}
            </FilterCard>

            {/* Error Count Card */}
            <FilterCard
                isActive={activeFilters.hasErrors}
                isMinimized={isPanelOpen}
                onClick={() => onFilterToggle('hasErrors')}
                title={isPanelOpen ? `Failed Requests: ${errorCount}` : undefined}
            >
                {isPanelOpen ? (
                    <Text
                        variant="small"
                        styles={{
                            root: {
                                fontWeight: '600',
                                color: activeFilters.hasErrors ? '#0078d4' : '#242424',
                                lineHeight: '1',
                                fontSize: '12px',
                            },
                        }}
                    >
                        {errorCount}
                    </Text>
                ) : (
                    <Stack tokens={{ childrenGap: 6 }}>
                        <Text
                            variant="xLarge"
                            styles={{
                                root: {
                                    fontWeight: '600',
                                    color: activeFilters.hasErrors ? '#0078d4' : '#242424',
                                    lineHeight: '1.2',
                                },
                            }}
                        >
                            {errorCount}
                        </Text>
                        <Text
                            variant="medium"
                            styles={{
                                root: {
                                    color: activeFilters.hasErrors ? '#106ebe' : '#616161',
                                    fontWeight: '400',
                                    lineHeight: '1.3',
                                },
                            }}
                        >
                            Failed Requests
                        </Text>
                    </Stack>
                )}
            </FilterCard>

            {/* High Priority Requests Card */}
            <FilterCard
                isActive={activeFilters.highPriorityRequests}
                isMinimized={isPanelOpen}
                isDisabled={isSearchResultsViewOpen}
                onClick={isSearchResultsViewOpen ? undefined : handleHighPrioritySearch}
                title={
                    isPanelOpen
                        ? isSearchResultsViewOpen
                            ? 'High Priority Requests (Disabled during search)'
                            : 'High Priority Requests'
                        : undefined
                }
            >
                {isPanelOpen ? (
                    <Stack horizontal tokens={{ childrenGap: 2 }} verticalAlign="center">
                        <img
                            src="/icons/ic_fluent_search_sparkle_24_regular.svg"
                            alt="High Priority Search"
                            width={12}
                            height={12}
                            style={{
                                filter: activeFilters.highPriorityRequests
                                    ? 'brightness(0) saturate(100%) invert(42%) sepia(93%) saturate(1352%) hue-rotate(204deg) brightness(102%) contrast(97%)'
                                    : 'brightness(0) saturate(100%) invert(15%) sepia(7%) saturate(928%) hue-rotate(199deg) brightness(98%) contrast(95%)',
                            }}
                        />
                        {highPriorityRequestsCount !== undefined && (
                            <Text
                                variant="small"
                                styles={{
                                    root: {
                                        fontWeight: '600',
                                        color: activeFilters.highPriorityRequests ? '#0078d4' : '#242424',
                                        lineHeight: '1',
                                        fontSize: '12px',
                                    },
                                }}
                            >
                                {highPriorityRequestsCount}
                            </Text>
                        )}
                    </Stack>
                ) : (
                    <Stack tokens={{ childrenGap: 6 }}>
                        {highPriorityRequestsCount !== undefined ? (
                            <Stack horizontal tokens={{ childrenGap: 6 }} verticalAlign="center">
                                <img
                                    src="/icons/ic_fluent_search_sparkle_24_regular.svg"
                                    alt="High Priority Search"
                                    width={20}
                                    height={20}
                                    style={{
                                        filter: activeFilters.highPriorityRequests
                                            ? 'brightness(0) saturate(100%) invert(42%) sepia(93%) saturate(1352%) hue-rotate(204deg) brightness(102%) contrast(97%)'
                                            : 'brightness(0) saturate(100%) invert(15%) sepia(7%) saturate(928%) hue-rotate(199deg) brightness(98%) contrast(95%)',
                                    }}
                                />
                                <Text
                                    variant="xLarge"
                                    styles={{
                                        root: {
                                            fontWeight: '600',
                                            color: activeFilters.highPriorityRequests ? '#0078d4' : '#242424',
                                            lineHeight: '1.2',
                                        },
                                    }}
                                >
                                    {highPriorityRequestsCount}
                                </Text>
                            </Stack>
                        ) : (
                            <img
                                src="/icons/ic_fluent_search_sparkle_24_regular.svg"
                                alt="High Priority Search"
                                width={24}
                                height={24}
                                style={{
                                    filter: activeFilters.highPriorityRequests
                                        ? 'brightness(0) saturate(100%) invert(42%) sepia(93%) saturate(1352%) hue-rotate(204deg) brightness(102%) contrast(97%)'
                                        : 'brightness(0) saturate(100%) invert(15%) sepia(7%) saturate(928%) hue-rotate(199deg) brightness(98%) contrast(95%)',
                                }}
                            />
                        )}
                        <Text
                            variant="medium"
                            styles={{
                                root: {
                                    color: activeFilters.highPriorityRequests ? '#106ebe' : '#616161',
                                    fontWeight: '400',
                                    lineHeight: '1.3',
                                },
                            }}
                        >
                            High Priority Requests
                        </Text>
                    </Stack>
                )}
            </FilterCard>
        </Stack>
    );
};
