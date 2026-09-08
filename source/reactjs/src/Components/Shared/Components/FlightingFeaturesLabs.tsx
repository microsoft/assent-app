import * as React from 'react';
import { Reducer } from 'redux';
import { IconButton, Icon, Spinner, SpinnerSize, Stack, Text, Toggle } from '@fluentui/react';
import { useDynamicReducer } from '@micro-frontend-react/employee-experience/lib/useDynamicReducer';
import { Context, withContext } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import {
    sharedComponentsReducer,
    sharedComponentsReducerName,
    sharedComponentsInitialState,
} from '../SharedComponents.reducer';
import { sharedComponentsSagas } from '../SharedComponents.sagas';
import {
    requestFlightingData,
    subscribeFlightingFeatures,
    unsubscribeFlightingFeatures,
    submitFlightingFeatureFeedback,
} from '../SharedComponents.actions';
import { IComponentsAppState } from '../SharedComponents.types';

const previewTagStyle: React.CSSProperties = {
    backgroundColor: '#f3f2f1',
    color: '#605e5c',
    fontSize: 11,
    fontWeight: 600,
    padding: '2px 8px',
    borderRadius: 10,
    textTransform: 'uppercase',
    letterSpacing: 0.4,
};

const generalTagStyle: React.CSSProperties = {
    ...previewTagStyle,
    backgroundColor: '#dff6dd',
    color: '#0b6a0b',
};

interface ILabsFeatureRow {
    FeatureName: string;
    FeatureDescription: string;
    IsSubscribed: boolean;
    IsPermanent: boolean;
    IconName: string;
}

function FlightingFeaturesLabs(): React.ReactElement {
    useDynamicReducer(sharedComponentsReducerName, sharedComponentsReducer as Reducer, [sharedComponentsSagas], false);

    const { useSelector, dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);

    const { myFlightingData, allFlightingData, isLoadingFlightingData, flightingFeatureFeedback } = useSelector(
        (state: IComponentsAppState) =>
            state.dynamic?.[sharedComponentsReducerName] || sharedComponentsInitialState
    );

    // Optimistic per-row state so toggling one feature doesn't flicker while data refreshes.
    const [pendingFeatures, setPendingFeatures] = React.useState<Record<string, boolean>>({});

    React.useEffect(() => {
        const hasData = Array.isArray(allFlightingData) && Array.isArray(myFlightingData);
        if (!hasData && !isLoadingFlightingData) {
            dispatch(requestFlightingData());
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [dispatch]);

    React.useEffect(() => {
        if (!Array.isArray(myFlightingData)) return;
        const names = myFlightingData.map((f: any) => f.FeatureName || f.featureName);
        setPendingFeatures(prev => {
            const next: Record<string, boolean> = {};
            let changed = false;
            for (const k of Object.keys(prev)) {
                if (names.includes(k) === prev[k]) {
                    changed = true;
                } else {
                    next[k] = prev[k];
                }
            }
            return changed ? next : prev;
        });
    }, [myFlightingData]);

    const rows = React.useMemo((): ILabsFeatureRow[] => {
        if (!allFlightingData || !Array.isArray(allFlightingData)) {
            return [];
        }
        const myFeatureNames: string[] = Array.isArray(myFlightingData)
            ? myFlightingData.map((f: any) => f.FeatureName || f.featureName)
            : [];
        return allFlightingData
            .map((f: any): ILabsFeatureRow | null => {
                // Rows without an IconName aren't user-facing Labs features (e.g. coachmark/pulse rows).
                const iconName: string = f.IconName || f.iconName || '';
                if (!iconName) {
                    return null;
                }
                const flightingRing = f.FlightingRing ?? f.flightingRing;
                if (flightingRing === null || flightingRing === undefined || flightingRing === '') {
                    return null;
                }
                const status: string = f.FlightingStatus || f.flightingStatus || '';
                const isFlighting = status === 'Flighting';
                const isPermanent = status === 'Enable All';
                if (!isFlighting && !isPermanent) {
                    return null;
                }
                const name: string = f.FeatureName || f.featureName;
                return {
                    FeatureName: name,
                    FeatureDescription: f.FeatureDescription || f.featureDescription || '',
                    IsSubscribed: isPermanent ? true : myFeatureNames.includes(name),
                    IsPermanent: isPermanent,
                    IconName: iconName,
                };
            })
            .filter((r): r is ILabsFeatureRow => r !== null)
            .sort((a, b) => Number(a.IsPermanent) - Number(b.IsPermanent));
    }, [allFlightingData, myFlightingData]);

    const onToggle = (row: ILabsFeatureRow, checked: boolean): void => {
        if (row.IsPermanent) {
            return;
        }
        if (checked && !row.IsSubscribed) {
            setPendingFeatures(prev => ({ ...prev, [row.FeatureName]: true }));
            dispatch(subscribeFlightingFeatures([row.FeatureName]));
        } else if (!checked && row.IsSubscribed) {
            setPendingFeatures(prev => ({ ...prev, [row.FeatureName]: false }));
            dispatch(unsubscribeFlightingFeatures([row.FeatureName]));
        }
    };

    const onVote = (row: ILabsFeatureRow, nextVote: 'Like' | 'Dislike'): void => {
        const current = (flightingFeatureFeedback || {})[row.FeatureName];
        // Click the same vote again to clear it; otherwise set/switch.
        const vote = current === nextVote ? 'None' : nextVote;
        dispatch(submitFlightingFeatureFeedback(row.FeatureName, vote));
    };

    if (isLoadingFlightingData && rows.length === 0) {
        return (
            <Stack horizontalAlign="center" styles={{ root: { padding: 24 } }}>
                <Spinner size={SpinnerSize.medium} label="Loading features..." />
            </Stack>
        );
    }

    return (
        <Stack >
            <Stack.Item>
                <Text variant="medium">
                    Try out experimental features. Toggle a feature on to opt in; turn it off to opt out.
                    Preview features may change or be removed at any time.
                </Text>
            </Stack.Item>

            {rows.length === 0 ? (
                <Stack.Item>
                    <Text variant="medium">No experimental features are currently available.</Text>
                </Stack.Item>
            ) : (
                rows.map(row => (
                    <Stack
                        key={row.FeatureName}
                        horizontal
                        verticalAlign="start"
                        tokens={{ childrenGap: 12 }}
                        styles={{
                            root: { padding: '12px 8px 12px 8px',
                                borderBottom: '1px solid #edebe9',
                            },
                        }}
                    >
                        <Stack.Item>
                            {/\.(svg|png)$/i.test(row.IconName) ? (
                                <img
                                    src={`/icons/${row.IconName}`}
                                    alt=""
                                    aria-hidden
                                    width={20}
                                    height={20}
                                    style={{ marginTop: 2 }}
                                />
                            ) : (
                                <Icon iconName={row.IconName} styles={{ root: { fontSize: 20, marginTop: 2 } }} />
                            )}
                        </Stack.Item>
                        <Stack.Item grow={1} styles={{ root: { minWidth: 0, flexBasis: 0 } }}>
                            <Stack tokens={{ childrenGap: 4 }}>
                                <Stack
                                    horizontal
                                    wrap
                                    verticalAlign="center"
                                    tokens={{ childrenGap: 8 }}
                                    styles={{ root: { minWidth: 0 } }}
                                >
                                    <Text
                                        variant="medium"
                                        styles={{ root: { fontWeight: 600, overflowWrap: 'anywhere', wordBreak: 'break-word', minWidth: 0 } }}
                                    >
                                        {row.FeatureName}
                                    </Text>
                                    {row.IsPermanent ? (
                                        <span style={generalTagStyle}>Enabled</span>
                                    ) : (
                                        <span style={previewTagStyle}>Preview</span>
                                    )}
                                </Stack>
                                {row.FeatureDescription && (
                                    <Text variant="small" styles={{ root: { color: '#605e5c' } }}>
                                        {row.FeatureDescription}
                                    </Text>
                                )}
                            </Stack>
                        </Stack.Item>
                        <Stack.Item align="center" styles={{ root: { flexShrink: 0 } }}>
                            <Stack horizontal tokens={{ childrenGap: 4 }} verticalAlign="center">
                                <IconButton
                                    ariaLabel={`Like ${row.FeatureName}`}
                                    title="Like"
                                    iconProps={{
                                        iconName:
                                            (flightingFeatureFeedback || {})[row.FeatureName] === 'Like'
                                                ? 'LikeSolid'
                                                : 'Like',
                                    }}
                                    styles={{
                                        root: { color: (flightingFeatureFeedback || {})[row.FeatureName] === 'Like' ? '#0078d4' : '#605e5c' },
                                    }}
                                    onClick={(): void => onVote(row, 'Like')}
                                />
                                <IconButton
                                    ariaLabel={`Dislike ${row.FeatureName}`}
                                    title="Dislike"
                                    iconProps={{
                                        iconName:
                                            (flightingFeatureFeedback || {})[row.FeatureName] === 'Dislike'
                                                ? 'DislikeSolid'
                                                : 'Dislike',
                                    }}
                                    styles={{
                                        root: { color: (flightingFeatureFeedback || {})[row.FeatureName] === 'Dislike' ? '#a4262c' : '#605e5c' },
                                    }}
                                    onClick={(): void => onVote(row, 'Dislike')}
                                />
                            </Stack>
                        </Stack.Item>
                        <Stack.Item align="center" styles={{ root: { flexShrink: 0 } }}>
                            <Toggle
                                ariaLabel={`Toggle ${row.FeatureName}`}
                                checked={
                                    row.FeatureName in pendingFeatures
                                        ? pendingFeatures[row.FeatureName]
                                        : row.IsSubscribed
                                }
                                disabled={row.IsPermanent}
                                onChange={(_ev, checked): void => onToggle(row, !!checked)}
                                styles={{ root: { marginBottom: 0 } }}
                            />
                        </Stack.Item>
                    </Stack>
                ))
            )}
        </Stack>
    );
}

const connected = withContext(FlightingFeaturesLabs);
export { connected as FlightingFeaturesLabs };
