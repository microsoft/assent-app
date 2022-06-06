import { SimpleEffect, getContext, put, all, call, takeLatest } from 'redux-saga/effects';
import { IHttpClient } from '@micro-frontend-react/employee-experience/lib/IHttpClient';
import { HelpPanelActionType } from './HelpPanel.action-types';
import { receiveAboutInfo } from './HelpPanel.actions';
import { setHeader } from '../Shared/Components/SagasHelper';

function* fetchAboutInfo(): IterableIterator<SimpleEffect<{}, {}>> {
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const aboutResponse: any = yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/about`,
            resource: __RESOURCE_URL__,
            headers: setHeader(null),
        });
        // get rid of the "mailto:" in front of the email address
        yield put(receiveAboutInfo(aboutResponse.data.supportEmailId.substring(7)));
    } catch (error) {
        // TODO: add failure action
        console.log(error);
    }
}

export function* helpPanelSagas(): IterableIterator<{}> {
    yield all([takeLatest(HelpPanelActionType.REQUEST_ABOUT_INFO, fetchAboutInfo)]);
}
