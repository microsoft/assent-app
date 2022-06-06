import { IComponentsAppState, IFeaturesIntroductionStep } from './SharedComponents.types';
import { SharedComponentsPersistentInitialState } from './SharedComponents.persistent-reducer';

export const getUserAlias = (state: IComponentsAppState): string => {
    return state.SharedComponentsPersistentReducer?.userAlias || SharedComponentsPersistentInitialState.userAlias;
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