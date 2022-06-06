import * as React from 'react';
import { connect } from 'react-redux';
import { SummaryCard } from './SummaryCard';
import { CardContainer } from './SummaryCard.styled';
import { ISummaryCardsModel } from './Models/ISummaryCardsModel';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { detailsReducerName, detailsReducer, detailsInitialState } from '../../Shared/Details/Details.reducer';
import { detailsSagas } from '../../Shared/Details/Details.sagas';
import { IComponentsAppState } from '../../Shared/SharedComponents.types';
import { IDetailsAppState } from '../../Shared/Details/Details.types';
import { sharedComponentsReducerName } from '../../Shared/SharedComponents.reducer';

interface ISummaryCardsState {
    selectedApprovalRecords: any;
    readRequests: string[];
    displayDocumentNumber?: any;
}
interface ISummaryCardsProps extends ISummaryCardsState {
    summary: any;
    bulkSelectedRecordsLength: number;
}

class SummaryCards extends React.Component<ISummaryCardsProps, ISummaryCardsState> {
    public static contextType: React.Context<IEmployeeExperienceContext> = Context as IEmployeeExperienceContext;
    public context!: React.ContextType<typeof Context>;
    constructor(props: ISummaryCardsProps) {
        super(props);
    }
    componentDidMount(): void {
        const { reducerRegistry, runSaga } = this.context;
        if (!reducerRegistry.exists(detailsReducerName)) {
            reducerRegistry.registerDynamic(detailsReducerName, detailsReducer, false, false);
            runSaga(detailsSagas);
        }
    }

    public render(): JSX.Element {
        const summaryList = this._mapSummarytoCard(this.props.summary);
        const allBulkSelected = this.props.bulkSelectedRecordsLength > 0 ? true : false;
        var bulkApprovalMaxSelected = this._getMaxBulkRecordsLength(summaryList);
        return (
            <CardContainer selectedApprovalRecords={this.props.selectedApprovalRecords}>
                {summaryList &&
                    summaryList
                        .filter((s) => s.DisplayDocumentNumber)
                        .map((item, index) => (
                            <SummaryCard
                                key={index}
                                selectedDocmentNumber={this.props.displayDocumentNumber}
                                cardRef={item.DisplayDocumentNumber + '_' + item.TenantId}
                                cardInfo={item}
                                showApprovalButton={false}
                                showViewDetailsButton={false}
                                allBulkCheckSelected={allBulkSelected}
                                selectForBulkApproval={
                                    this._isCardAvailableForBulk(item) &&
                                    !this._isCardSelected(item) &&
                                    bulkApprovalMaxSelected-- > 0
                                        ? true
                                        : this._isCardSelected(item)
                                }
                            />
                        ))}
            </CardContainer>
        );
    }

    private _isCardAvailableForBulk(item: ISummaryCardsModel): boolean {
        const { readRequests } = this.props;
        return item.isRead || readRequests.includes(item.DocumentNumber) || !item.IsControlsAndComplianceRequired;
    }

    private _isCardSelected(item: ISummaryCardsModel): boolean {
        const { selectedApprovalRecords } = this.props;
        const record = selectedApprovalRecords.find(
            (rec: { DocumentNumber: string }) => rec.DocumentNumber == item.DocumentNumber
        );
        return record ? true : false;
    }

    private _getMaxBulkRecordsLength(summaryList: ISummaryCardsModel[]): number {
        const { selectedApprovalRecords, bulkSelectedRecordsLength } = this.props;
        let recordsLength = selectedApprovalRecords.length;
        let selectedRecords = 0;
        for (let i = 0; i < bulkSelectedRecordsLength && i < summaryList.length; i++) {
            const item = summaryList[i];
            if (this._isCardAvailableForBulk(item) && this._isCardSelected(item)) {
                recordsLength--;
                selectedRecords++;
            }
        }
        return bulkSelectedRecordsLength - recordsLength - selectedRecords;
    }

    private _mapSummarytoCard(summary: any): ISummaryCardsModel[] {
        const summaryList = summary.map(
            (val: any) =>
                ({
                    cardRef: val.ApprovalIdentifier?.DocumentNumber, // this is using for id of the div
                    AppName: val.AppName,
                    Title: val.Title,
                    SubmittedDate: val.SubmittedDate,
                    CompanyCode: val.CompanyCode,
                    UnitValue: val.UnitValue,
                    UnitofMeasure: val.UnitOfMeasure,
                    DisplayDocumentNumber: val.ApprovalIdentifier?.DisplayDocumentNumber,
                    DocumentNumber: val.ApprovalIdentifier?.DocumentNumber,
                    FiscalYear: val.ApprovalIdentifier?.FiscalYear,
                    Submitter: val.Submitter?.Name,
                    SubmitterAlias: val.Submitter?.Alias,
                    TenantId: val.TenantId,
                    isRead: val.IsRead,
                    lastFailed: val.LastFailed,
                    CustomAttributeName: val.CustomAttribute?.CustomAttributeName,
                    CustomAttributeValue: val.CustomAttribute?.CustomAttributeValue,
                    IsControlsAndComplianceRequired: val.IsControlsAndComplianceRequired,
                } as ISummaryCardsModel)
        );
        return summaryList;
    }
}

const mapStateToProps = (state: IComponentsAppState): ISummaryCardsState => ({
    selectedApprovalRecords: state.dynamic?.[sharedComponentsReducerName]?.selectedApprovalRecords || [],
    readRequests: state.dynamic?.[detailsReducerName]?.readRequests || detailsInitialState.readRequests,
});

const connected = connect(mapStateToProps)(React.memo(SummaryCards));
export { connected as SummaryCards };
