import { ISharedComponentsPersistentState } from './SharedComponents.types';
import { SharedComponentsAction, SharedComponentsActionType } from './SharedComponents.action-types';
import { teachingSteps, initialTeachingStep } from './Components/FeaturesIntroductionSteps';
import { DEFAULT_VISIBLE_COLUMNS, CARD_VIEW, DEFAULT_VIEW_TYPE, TABLE_COLUMNS_DEFAULT, TABLE_COLUMNS_PULLTENANT, parseColumnPreference } from './SharedConstants';

export const sharedComponentsPersistentReducerName = 'SharedComponentsPersistentReducer';

export const SharedComponentsPersistentInitialState: ISharedComponentsPersistentState = {
    userName: '',
    userAlias: '',
    onBehalfUserUpn: '',
    teachingBubbleVisibility: false,
    teachingBubbleStep: teachingSteps[initialTeachingStep],
    onBehalfUserId: '',
    visibleColumnsDefault: DEFAULT_VISIBLE_COLUMNS.Default,
    visibleColumnsPullTenant: DEFAULT_VISIBLE_COLUMNS.PullTenant,
    isCardViewSelected: true,
    isViewTypeInitialized: false,
    isColumnsInitialized: false,
};

export function sharedComponentsPersistentReducer(
    prev: ISharedComponentsPersistentState = SharedComponentsPersistentInitialState,
    action: SharedComponentsAction
): ISharedComponentsPersistentState {
    switch (action.type) {
        case SharedComponentsActionType.UPDATE_USER_ALIAS:
            return {
                ...prev,
                userAlias: action.userAlias,
                userName: action.userName,
                onBehalfUserUpn: action.onBehalfUserUpn,
                onBehalfUserId: action.onBehalfUserId,
            };
        case SharedComponentsActionType.TOGGLE_TEACHING_BUBBLE_VISIBILITY:
            return {
                ...prev,
                teachingBubbleVisibility: !prev.teachingBubbleVisibility,
                teachingBubbleStep: teachingSteps[initialTeachingStep],
            };
        case SharedComponentsActionType.UPDATE_TEACHING_STEP:
            return {
                ...prev,
                teachingBubbleStep: action.newStep,
            };
        case SharedComponentsActionType.UPDATE_VISIBLE_COLUMNS: {
            const isPullTenant = action.tenantType === 'pullTenant';
            return {
                ...prev,
                visibleColumnsDefault: isPullTenant ? prev.visibleColumnsDefault : action.columns,
                visibleColumnsPullTenant: isPullTenant ? action.columns : prev.visibleColumnsPullTenant,
                isColumnsInitialized: true,
            };
        }
        case SharedComponentsActionType.UPDATE_CARD_VIEW_TYPE:
            return {
                ...prev,
                isCardViewSelected: action.isCardViewSelected,
                isViewTypeInitialized: true,
            };
        case SharedComponentsActionType.RECEIVE_USER_PREFERENCES: {
            // Apply API defaults only once per session for each category.
            // Subsequent RECEIVE_USER_PREFERENCES (e.g. after saves) must not
            // override the user's current session choices.
            if (prev.isViewTypeInitialized && prev.isColumnsInitialized) {
                return prev;
            }
            if (!action.data || action.data.length === 0) {
                return prev;
            }
            const updates: Partial<ISharedComponentsPersistentState> = {};

            if (!prev.isViewTypeInitialized) {
                const viewType = action.data.find((u: any) => u.UserPreferenceText === DEFAULT_VIEW_TYPE);
                if (viewType) {
                    updates.isCardViewSelected = viewType.UserPreferenceStatus === CARD_VIEW;
                }
                updates.isViewTypeInitialized = true;
            }

            if (!prev.isColumnsInitialized) {
                const colsDefault = action.data.find((u: any) => u.UserPreferenceText === TABLE_COLUMNS_DEFAULT);
                if (colsDefault) {
                    const parsed = parseColumnPreference(colsDefault.UserPreferenceStatus);
                    if (parsed) {
                        updates.visibleColumnsDefault = parsed;
                    }
                }
                const colsPull = action.data.find((u: any) => u.UserPreferenceText === TABLE_COLUMNS_PULLTENANT);
                if (colsPull) {
                    const parsed = parseColumnPreference(colsPull.UserPreferenceStatus);
                    if (parsed) {
                        updates.visibleColumnsPullTenant = parsed;
                    }
                }
                updates.isColumnsInitialized = true;
            }

            return { ...prev, ...updates };
        }
        default:
            return prev;
    }
}
