import * as React from 'react';
import { IPersonaProps } from '@fluentui/react/lib/Persona';
import { IBasePickerSuggestionsProps, NormalPeoplePicker } from '@fluentui/react/lib/Pickers';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import {
    sharedComponentsInitialState,
    sharedComponentsReducer,
    sharedComponentsReducerName
} from '../SharedComponents.reducer';
import { sharedComponentsSagas } from '../SharedComponents.sagas';
import { Reducer } from 'redux';
import { updatePeoplePickerHasError, updatePeoplePickerSelection } from '../SharedComponents.actions';
import { IComponentsAppState } from '../SharedComponents.types';
import { useDynamicReducer } from '@micro-frontend-react/employee-experience/lib/useDynamicReducer';

const suggestionProps: IBasePickerSuggestionsProps = {
    suggestionsHeaderText: 'Suggested People',
    noResultsFoundText: 'No results found',
    loadingText: 'Loading'
};

const APPROVER_ERROR = 'Approver alias is required';

export const PeoplePicker: React.FunctionComponent = () => {
    useDynamicReducer(sharedComponentsReducerName, sharedComponentsReducer as Reducer, [sharedComponentsSagas], false);
    const reduxContext = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const { dispatch, useSelector, httpClient } = reduxContext;

    const { peoplePickerHasError } = useSelector(
        (state: IComponentsAppState) => state.dynamic?.[sharedComponentsReducerName] || sharedComponentsInitialState
    );

    const picker = React.useRef(null);

    const onFilterChanged = (filterText: string): IPersonaProps[] | Promise<IPersonaProps[]> => {
        if (filterText) {
            return new Promise(resolve => {
                //calling graph api directly here instead of using sagas because api response value needs to be returned
                //TODO: add telemetry
                try {
                    httpClient
                        .request({
                            url: `${__GRAPH_BASE_URL__}users?$filter=startswith(userPrincipalName%2C+'${filterText}')`,
                            resource: __GRAPH_RESOURCE_URL__
                        })
                        .then((result: any) =>
                            resolve(
                                result.data.value.map((item: any) => {
                                    return {
                                        text: item.displayName,
                                        upn: item.userPrincipalName
                                    };
                                })
                            )
                        );
                } catch {
                    resolve([]);
                }
            });
        } else {
            return [];
        }
    };

    function getTextFromItem(persona: IPersonaProps): string {
        return persona.text as string;
    }

    function onChangeSelection(items: object[]): void {
        dispatch(updatePeoplePickerSelection(items));
        if (items.length > 0 && peoplePickerHasError) {
            dispatch(updatePeoplePickerHasError(false));
        }
    }

    return (
        <div>
            <label className="ms-Label custom-label">Approver Alias</label>
            <NormalPeoplePicker
                onResolveSuggestions={onFilterChanged}
                onChange={onChangeSelection}
                getTextFromItem={getTextFromItem}
                pickerSuggestionsProps={suggestionProps}
                className={'ms-PeoplePicker'}
                key={'normal'}
                removeButtonAriaLabel={'Remove'}
                inputProps={{
                    'aria-label': "Enter the approver's alias",
                    'aria-required': true,
                    required: true,
                    placeholder: 'Enter an alias'
                }}
                componentRef={picker}
                resolveDelay={300}
                styles={{ root: { maxWidth: 500, minHeight: '32px' } }}
                itemLimit={1}
            />
            {peoplePickerHasError && (
                <p role="alert" className="custom-input-error">
                    {APPROVER_ERROR}
                </p>
            )}
        </div>
    );
};
