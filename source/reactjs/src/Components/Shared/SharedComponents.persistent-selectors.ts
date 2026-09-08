import { IComponentsAppState, IFeaturesIntroductionStep } from './SharedComponents.types';
import { SharedComponentsPersistentInitialState } from './SharedComponents.persistent-reducer';

export const getUserAlias = (state: IComponentsAppState): string => {
    return state.SharedComponentsPersistentReducer?.userAlias || SharedComponentsPersistentInitialState.userAlias;
};

export const getOnBehalfUserUpn = (state: IComponentsAppState): string => {
    if (state.SharedComponentsPersistentReducer) {
        return (
            state.SharedComponentsPersistentReducer.onBehalfUserUpn ||
            SharedComponentsPersistentInitialState.onBehalfUserUpn
        );
    }
};

export const getOnBehalfUserId = (state: IComponentsAppState): string => {
    if (state.SharedComponentsPersistentReducer) {
        return (
            state.SharedComponentsPersistentReducer.onBehalfUserId ||
            SharedComponentsPersistentInitialState.onBehalfUserId
        );
    }
};

export const getTeachingBubbleVisibility = (state: IComponentsAppState): boolean => {
    return (
        state.SharedComponentsPersistentReducer?.teachingBubbleVisibility ||
        SharedComponentsPersistentInitialState.teachingBubbleVisibility
    );
};

export const getTeachingBubbleStep = (state: IComponentsAppState): IFeaturesIntroductionStep => {
    return (
        state.SharedComponentsPersistentReducer?.teachingBubbleStep ||
        SharedComponentsPersistentInitialState.teachingBubbleStep
    );
};

export const getUserName = (state: IComponentsAppState): string => {
    return state.SharedComponentsPersistentReducer?.userName || SharedComponentsPersistentInitialState.userName;
};

export const getPersistedVisibleColumns = (state: IComponentsAppState, tenantType: string): string[] => {
    if (tenantType === 'pullTenant') {
        return (
            state.SharedComponentsPersistentReducer?.visibleColumnsPullTenant ||
            SharedComponentsPersistentInitialState.visibleColumnsPullTenant
        );
    }
    return (
        state.SharedComponentsPersistentReducer?.visibleColumnsDefault ||
        SharedComponentsPersistentInitialState.visibleColumnsDefault
    );
};

export const getPersistedCardViewSelected = (state: IComponentsAppState): boolean => {
    return (
        state.SharedComponentsPersistentReducer?.isCardViewSelected ??
        SharedComponentsPersistentInitialState.isCardViewSelected
    );
};
