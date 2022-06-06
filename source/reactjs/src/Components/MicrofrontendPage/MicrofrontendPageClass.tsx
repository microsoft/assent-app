import * as React from 'react';
import { Context, withContext } from '@micro-frontend-react/employee-experience/lib/Context';
import {
    sharedComponentsReducerName,
    sharedComponentsReducer,
    sharedComponentsInitialState
} from '../Shared/SharedComponents.reducer';
import { IComponentsAppState, ISharedComponentsState } from '../Shared/SharedComponents.types';
import { sharedComponentsSagas } from '../Shared/SharedComponents.sagas';
import { Dispatch } from 'redux';
import { requestMyProfile } from '../Shared/SharedComponents.actions';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { connect } from 'react-redux';
import { ComponentProvider } from '@micro-frontend-react/employee-experience/lib/ComponentProvider';
import { Stack } from '@fluentui/react/lib/Stack';
import { DefaultButton } from '@fluentui/react';
import { Container } from '../Shared/SharedLayout';

//DEMO PAGE FOR CDN COMPONENT

/*
interface IMicrofrontendPageClassDispatch {
    dispatchRequestMyProfile(): void;
}

interface IMicrofrontendPageClassProps extends ISharedComponentsState, IMicrofrontendPageClassDispatch {}

class MicrofrontendPageClass extends React.Component<IMicrofrontendPageClassProps> {
    public static contextType: React.Context<IReduxContext> = ReduxContext;
    public context!: React.ContextType<typeof ReduxContext>;

    executeMicrofrontendActionRef: any = {}

    public componentDidMount(): void {
        const { reducerRegistry, runSaga } = this.context;

        if (!reducerRegistry.exists(sharedComponentsReducerName)) {
            reducerRegistry.registerDynamic(sharedComponentsReducerName, sharedComponentsReducer);
            runSaga(sharedComponentsSagas);
        }

        const { dispatchRequestMyProfile } = this.props;
        dispatchRequestMyProfile();
    }

    public render(): React.ReactElement {
        return (
            <Container>
                <Stack>
                    <Stack.Item>
                        <ComponentProvider
                            config={{
                                script: __MICROFRONTEND_CDN_URL__,
                                name: 'MicrofrontendInputs'
                            }}
                            data={{ executeMicrofrontendActionRef: this.executeMicrofrontendActionRef }}
                        />
                    </Stack.Item>
                    <Stack.Item>
                        <DefaultButton
                            text="Submit"
                            title="Submit"
                            onClick={(): void => this.executeMicrofrontendActionRef.submitHandler()}
                        />
                    </Stack.Item>
                </Stack>
            </Container>
        );
    }
}

const mapStateToProps = (state: IComponentsAppState): ISharedComponentsState =>
    state.dynamic?.[sharedComponentsReducerName] || sharedComponentsInitialState;

const mapDispatchToProps = (dispatch: Dispatch): IMicrofrontendPageClassDispatch => ({
    dispatchRequestMyProfile: (): void => {
        dispatch(requestMyProfile());
    }
});

const connected = withContext(connect(mapStateToProps, mapDispatchToProps)(MicrofrontendPageClass) as any);
export { connected as MicrofrontendPageClass };

*/
