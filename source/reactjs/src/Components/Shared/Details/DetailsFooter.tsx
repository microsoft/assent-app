import * as React from 'react';
import * as Styled from '../SharedLayout';
import * as DetailsStyled from './DetailsStyling';
import '../../../App.css';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { IComponentsAppState } from '../SharedComponents.types';
import {
    getTenantInfo,
    getSummary,
    getSelectedApprovalRecords,
    getBulkActionConcurrentCall,
    getPanelOpen,
    getFilterValue,
    getIsPullTenantSelected,
    getTenantAdditionalNotes,
    getTenantDataModelMapping,
    getExternalTenantInfo,
    getFilteredTenantInfo,
    getToggleDetailsScreen,
    getSelectedTenantDelegation,
} from '../SharedComponents.selectors';
import { postAction } from '../Details/Details.actions';
import { IStackTokens, Stack } from '@fluentui/react/lib/Stack';
import { Checkbox, ICheckboxProps } from '@fluentui/react/lib/Checkbox';
import { TextField } from '@fluentui/react/lib/TextField';
import BasicDropdown from '../../../Controls/BasicDropdown';
import MultilineTextField from '../../../Controls/MultilineTextField';
import BasicButton from '../../../Controls/BasicButton';
import { IContextualMenuItem, IDropdownOption } from '@fluentui/react';
import { PeoplePicker } from '../Components/PeoplePicker';
import {
    flattenObject,
    generatePullModelSummary,
    generateSummaryObjForPullTenant,
    isJustificationRequiredForBulk,
    lowerCaseFirstLetter,
    testRegexIgnoreCase,
    validateCondition,
    validateConditionClient,
} from '../../../Helpers/sharedHelpers';
import { trackException, TrackingEventId } from '../../../Helpers/telemetryHelpers';
import DropdownButton from '../../../Controls/DropdownButton';
import WarningView from './DetailsMessageBars/WarningView';
import { sharedComponentsInitialState, sharedComponentsReducerName } from '../SharedComponents.reducer';
import { IAddditionalInformation, IControlValidation, DetailsType } from './Details.types';
import {
    updatePeoplePickerHasError,
    updatePeoplePickerSelection,
    updateIsProcessingBulkApproval,
    updateFailedPullTenantRequests,
    updatePanelState,
    updateBulkStatus,
} from '../SharedComponents.actions';
import { getAreDetailsEditable, getSummaryJSON } from './Details.selectors';
import { postEditableDetails, updateAdditionalData } from './Details.actions';
import { startCase } from 'lodash';
import sanitizeHtml = require('sanitize-html');

const RECEIPTS_ERROR = 'Receipts must be reviewed before submission';
const COMPLIANCE_ERROR = 'Compliance with Anti-Corruption policies must be confirmed before submission';
const NEXT_APPROVER_ERROR = 'Next approver is required';
const PLACEMENT_ERROR = 'Approver placement is required';
const SIGNATURE_ERROR = 'Signature does not match';

const FIRST_RENDER_TIMEOUT = 1000;

const inputValidationErrors: { [code: string]: string } = {
    addApproverPlacement: PLACEMENT_ERROR,
    addNextApprover: NEXT_APPROVER_ERROR,
    signatureInput: SIGNATURE_ERROR,
};

const DetailsFooter = React.forwardRef((props: any, ref): JSX.Element => {
    const [comments, setComments] = React.useState('');
    const [notesErrorMessage, setNotesErrorMessage] = React.useState('');
    const [isCommentMandatory, setIsCommentMandatory] = React.useState(false);
    const [actionDetailsComments, setActionDetailsComments] = React.useState(null);
    const [code, setCode] = React.useState('');
    const [reasonCode, setReasonCode] = React.useState('');
    const [reasonText, setReasonText] = React.useState('');
    const [justificationsList, setJustificationsList] = React.useState(null);
    const [charLimit, setCharLimit] = React.useState(null);
    const [displayNotesInput, setDisplayNotesInput] = React.useState(null);
    const [actionAdditionalInformation, setActionAdditionalInformation] = React.useState(null);
    const [remainingChars, setRemainingChars] = React.useState(null);
    const [receiptsCheck, setReceiptsCheck] = React.useState(false);
    const [corruptionCheck, setCorruptionCheck] = React.useState(false);
    const [displayCorruptionError, setDisplayCorruptionError] = React.useState(false);
    const [displayReceiptError, setDisplayReceiptError] = React.useState(false);
    const [displayReceiptCheckbox, setDisplayReceiptCheckbox] = React.useState(false);
    const [displayCorruptionCheckbox, setDisplayCorruptionCheckbox] = React.useState(false);
    const [actionName, setActionName] = React.useState('');
    const [isJustificationApplicableForAction, setIsJustificationApplicableForAction] = React.useState(true);

    const { useSelector, dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const summary = useSelector(getSummary);
    const selectedApprovalRecords = useSelector(getSelectedApprovalRecords);
    const bulkActionConcurrentCall = useSelector(getBulkActionConcurrentCall);
    const tenantInfo = useSelector(getTenantInfo);
    const isPanelOpen = useSelector(getPanelOpen);
    const filterValue = useSelector(getFilterValue);
    const summaryJSON: any = useSelector(getSummaryJSON);
    const selectedTenantDelegation = useSelector(getSelectedTenantDelegation);
    const [sequenceID, setSequenceID] = React.useState('');
    const [nextApprover, setNextApprover] = React.useState('');
    const [actionConfirmationMessage, setActionConfirmationMessage] = React.useState('');
    const [summaryObjectValues, setSummaryObjectValues] = React.useState(null);
    const [additionalControlValidation, setAdditionalControlValidation] = React.useState<IControlValidation[]>([]);
    const [commentsWithKeys, setCommentsWithKeys] = React.useState([]);
    const [justificationsWithKeys, setJustificationsWithKeys] = React.useState([]);
    const [digitalSignature, setDigitalSignature] = React.useState('');
    const [isCommentExceeded, setIsCommentExceeded] = React.useState(false);
    const [isFirstNotesRender, setIsFirstNotesRender] = React.useState(false);
    const stackTokens: IStackTokens = { childrenGap: 12 };

    let primaryActionRef: any = null;
    let justificationRef: any = null;
    const {
        headerDetailsJSON,
        detailsJSON,
        isMicrofrontendOpen,
        isControlsAndComplianceRequired,
        isRequestFullyScrolled,
        executeMicrofrontendActionRef,
        detailsComponentType,
        isBulkApproval,
    } = props;

    const { peoplePickerSelections, profile, peoplePickerHasError } = useSelector(
        (state: IComponentsAppState) => state.dynamic?.[sharedComponentsReducerName] || sharedComponentsInitialState
    );

    const areDetailsEditable = useSelector(getAreDetailsEditable);
    const isPullTenantSelected = useSelector(getIsPullTenantSelected);
    const tenantAdditionalNotes = useSelector(getTenantAdditionalNotes);
    const tenantDataModel = useSelector(getTenantDataModelMapping);
    const externalTenantInfo = useSelector(getExternalTenantInfo);
    const filteredTenantInfo = useSelector(getFilteredTenantInfo);
    const toggleDetailsScreen = useSelector(getToggleDetailsScreen);

    const isExternalTenantActionDetailsForBulk = filteredTenantInfo?.isExternalTenantActionDetails;
    const isExternalTenantActionDetailsForSingle = props.isExternalTenantActionDetails && isPullTenantSelected;

    const actionDetails = isBulkApproval
        ? isExternalTenantActionDetailsForBulk
            ? externalTenantInfo?.actionDetails
            : filteredTenantInfo?.actionDetails
        : isExternalTenantActionDetailsForSingle
        ? externalTenantInfo?.actionDetails
        : props.actionDetails;

    React.useImperativeHandle(ref, () => {
        return {
            clearNotesPopupData: clearNotesPopupData,
        };
    });

    React.useEffect(() => {
        if (justificationRef && displayNotesInput && !actionAdditionalInformation) {
            justificationRef.focus();
            setIsFirstNotesRender(true);
            setTimeout(() => {
                setIsFirstNotesRender(false);
            }, FIRST_RENDER_TIMEOUT);
        }
    }, [justificationRef, displayNotesInput, actionAdditionalInformation]);


    const isJustificationApplicableinSummary = (isBulk: boolean): boolean => {
        if (isBulk) {
            if (selectedApprovalRecords && selectedApprovalRecords.length > 0) {
                return isJustificationRequiredForBulk(selectedApprovalRecords);
            }
            return false;
        } else {
            return headerDetailsJSON?.isJustificationApplicable;
        }
    };

    const resetValidation = (index: number, newValue: string): void => {
        let newValidationArray = [...additionalControlValidation];
        const expectedValue = newValidationArray[index].expectedValue;
        if (expectedValue && expectedValue !== '') {
            if (newValue && newValue === expectedValue && !newValidationArray[index].isValid) {
                newValidationArray[index].isValid = true;
                newValidationArray[index].errorMessage = '';
            } else if (newValue !== expectedValue && newValidationArray[index].isValid) {
                newValidationArray[index].isValid = false;
                newValidationArray[index].errorMessage = inputValidationErrors[newValidationArray[index].controlCode];
            }
        } else if (
            (!expectedValue || expectedValue === '') &&
            !newValidationArray[index].isValid &&
            newValue &&
            newValue !== ''
        ) {
            newValidationArray[index].isValid = true;
            newValidationArray[index].errorMessage = '';
        }
        setAdditionalControlValidation(newValidationArray);
    };

    const handleSequenceIDSelect = (e: object, selection: IDropdownOption): void => {
        const newValue = selection.key as string;
        setSequenceID(newValue);
        resetValidation(parseInt((selection as any).id), newValue);
    };

    const handleNextApproverSelect = (e: object, selection: IDropdownOption): void => {
        const newValue = selection.key as string;
        setNextApprover(newValue);
        resetValidation(parseInt((selection as any).id), newValue);
    };

    const handleSignatureField = (event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, id: number): void => {
        const { value } = (event as any).target;
        setDigitalSignature(value);
        resetValidation(id, value);
    };

    const getControlValues = (control: IAddditionalInformation): object[] | null => {
        try {
            let controlValues = control.Values;
            if (control.IsValueFromSummaryObject && control.Values.length > 0) {
                controlValues = headerDetailsJSON[control.Values[0].Code];
            }
            return controlValues;
        } catch {
            return null;
        }
    };

    const getSummaryObjectValues = (additionalInformation: IAddditionalInformation[]) => {
        if (additionalInformation !== null) {
            const res = additionalInformation.map((control: IAddditionalInformation) => {
                if (control.IsValueFromSummaryObject) {
                    const values = getControlValues(control);
                    return values.map((value) => {
                        return JSON.stringify(value);
                    });
                }
                return [];
            });
            return res;
        } else {
            return [];
        }
    };

    const getValidationInfo = (additionalInformation: IAddditionalInformation[]): IControlValidation[] => {
        if (additionalInformation !== null) {
            const res = additionalInformation.map((control: IAddditionalInformation) => {
                const values = getControlValues(control);
                const hasValues = values && Array.isArray(values) && values.length > 0;
                const isMandatory = control.Type === 'select' ? hasValues && control.IsMandatory : control.IsMandatory;
                const expectedValue =
                    control.Code === 'signatureInput' ? profile?.givenName + ' ' + profile?.surname : null;
                const isValid = !expectedValue;
                const errorMessage = isValid ? '' : inputValidationErrors[control.Code];
                return {
                    isMandatory: isMandatory,
                    isValid: isValid,
                    errorMessage: errorMessage,
                    controlCode: control.Code,
                    expectedValue: expectedValue,
                };
            });
            return res;
        } else {
            return [];
        }
    };

    const renderAdditionalInformation = (
        additionalInformation: IAddditionalInformation[],
        isBulk: boolean
    ): JSX.Element[] => {
        const controls = additionalInformation.map((control: any, controlIndex) => {
            let resultElement = null;
            if (control.Type == 'label') {
                resultElement = (
                    <WarningView
                        warningMessages={[control.Text]}
                        isCollapsible={false}
                        messageBarStyles={DetailsStyled.actionWarningStyle(isBulk, toggleDetailsScreen)}
                    />
                );
            } else if (control.Type === 'select') {
                const controlValues: any = getControlValues(control);
                if (controlValues && controlValues.length > 0) {
                    const options: IDropdownOption[] = controlValues.map((value: any, valueIndex: number) => {
                        const dropdownText = control.IsValueFromSummaryObject ? (value as any).Name : value.Text;
                        const dropdownKey = control.IsValueFromSummaryObject
                            ? summaryObjectValues[controlIndex][valueIndex]
                            : value.Code;
                        return { key: dropdownKey, text: dropdownText, id: controlIndex.toString() };
                    });
                    const defaultSelectedValueIndex = controlValues.findIndex((value: any) => {
                        return value._default || value.Default;
                    });
                    const defaultSelectedValueKey =
                        defaultSelectedValueIndex > 0
                            ? control.IsValueFromSummaryObject
                                ? summaryObjectValues[controlIndex][defaultSelectedValueIndex]
                                : controlValues[defaultSelectedValueIndex].Code
                            : null;
                    const selectionValue = control.IsValueFromSummaryObject ? nextApprover : sequenceID;
                    const selectedKey = selectionValue === '' ? defaultSelectedValueKey : selectionValue;
                    //initializing default values
                    if (selectionValue === '' && defaultSelectedValueKey && defaultSelectedValueKey !== '') {
                        if (control.IsValueFromSummaryObject) {
                            setNextApprover(selectedKey);
                        } else {
                            setSequenceID(selectedKey);
                        }
                    }
                    const handleSelect = control.IsValueFromSummaryObject
                        ? handleNextApproverSelect
                        : handleSequenceIDSelect;
                    return (
                        <BasicDropdown
                            options={options}
                            label={control.Text}
                            onChange={handleSelect}
                            styles={Styled.SmallDropdownStyles}
                            selectedKey={selectedKey}
                            required={true}
                            errorMessage={
                                additionalControlValidation[controlIndex].isValid
                                    ? null
                                    : additionalControlValidation[controlIndex].errorMessage
                            }
                        />
                    );
                }
                return null; //if no values defined for dropdown
            } else if (control.Type == 'peoplePicker') {
                resultElement = <PeoplePicker />;
            } else if (control.Type == 'textfield') {
                const expectedValue = additionalControlValidation[controlIndex].expectedValue;
                const instructionsValue = control.Values.find((value: any) => {
                    return value.Code === 'instructions';
                });
                const errorMessage = additionalControlValidation[controlIndex].errorMessage;
                return (
                    <Stack>
                        <Stack.Item>
                            <label className="ms-Label custom-label">{control.Text}</label>
                        </Stack.Item>
                        {instructionsValue && <Stack.Item>{instructionsValue.Text}</Stack.Item>}
                        {expectedValue && (
                            <Stack.Item tokens={{ padding: '10px 0px 0px 0px' }}>
                                <label htmlFor="txtPlacements" className="ms-Label">
                                    <strong>{expectedValue}</strong>
                                </label>
                            </Stack.Item>
                        )}
                        <Stack.Item>
                            <TextField
                                id="txtPlacements"
                                errorMessage={!additionalControlValidation[controlIndex].isValid && errorMessage}
                                onChange={(e) => handleSignatureField(e, controlIndex)}
                            />
                        </Stack.Item>
                    </Stack>
                );
            } else if (control.Type == 'checkbox') {
                //TODO: evaluate conditions outside of render
                if (
                    control.Values.length > 0 &&
                    (control.Values[0].Condition == null ||
                        control.Values[0].Condition == '' ||
                        validateCondition(getConditionPropertyObject(), control.Values[0].Condition))
                ) {
                    if (control.Code === 'receiptCheck' && !displayReceiptCheckbox) {
                        setDisplayReceiptCheckbox(true);
                    } else if (control.Code === 'corruptionCheck' && !displayCorruptionCheckbox) {
                        setDisplayCorruptionCheckbox(true);
                    }
                    const inputProps: ICheckboxProps['inputProps'] = {
                        'aria-required': true,
                    };
                    const onCheckboxChange = (ev: React.FormEvent<HTMLElement>, checked: boolean): void => {
                        if (control.Code === 'receiptCheck') {
                            setReceiptsCheck(checked);
                            if (displayReceiptError && checked) {
                                setDisplayReceiptError(false);
                            }
                        } else if (control.Code === 'corruptionCheck') {
                            setCorruptionCheck(checked);
                            if (displayCorruptionError && checked) {
                                setDisplayCorruptionError(false);
                            }
                        }
                    };
                    const checkboxErrorMessage =
                        control.Code === 'receiptCheck'
                            ? RECEIPTS_ERROR
                            : control.Code === 'corruptionCheck'
                            ? COMPLIANCE_ERROR
                            : '';
                    const showErrorMessage =
                        (displayReceiptError && control.Code === 'receiptCheck') ||
                        (displayCorruptionError && control.Code === 'corruptionCheck');
                    const checkboxStyles = showErrorMessage ? { checkbox: { borderColor: 'red' } } : null;
                    return (
                        <>
                            <label className="ms-Label custom-label">{control.Text}</label>
                            <Checkbox
                                label={control.Values[0].Text}
                                inputProps={inputProps}
                                onChange={onCheckboxChange}
                                styles={checkboxStyles}
                            />
                            {showErrorMessage && (
                                <p role="alert" className="custom-input-error">
                                    {checkboxErrorMessage}
                                </p>
                            )}
                        </>
                    );
                }
            }
            return resultElement;
        });
        return controls.map((item: JSX.Element, index: number) => <Stack.Item key={index}>{item}</Stack.Item>);
    };

    const handleComments = (e: object) => {
        const { value } = (e as any).target;
        setIsCommentExceeded(false);
        setNotesErrorMessage('');
        const _remainingChars = typeof charLimit === 'number' ? charLimit - value.length : null;
        setRemainingChars(_remainingChars);

        if (charLimit - value.length < 0) {
            const exceededCharacters = value.length - Number(charLimit);
            setComments(value);
            setNotesErrorMessage(`You have exceeded allowed characters by ${exceededCharacters} characters`);
            setRemainingChars(0);
            setIsCommentExceeded(true);
            return;
        }
        const isCommentWhitespace = !value.replace(/\s/g, '');

        if (notesErrorMessage && isCommentMandatory && !isCommentWhitespace) {
            setNotesErrorMessage('');
            setComments(value);
        } else {
            setComments(value);
        }
    };

    const handleJustificationSelect = (e: object, selection: IDropdownOption) => {
        setReasonCode((selection as any).key);
        setReasonText((selection as any).text);
    };

    const renderDropDown = (defaultConfirmationMessage: string) => {
        const options: IDropdownOption[] = justificationsList.map((item: object) => {
            return { key: getValueFromProperty(item, 'Code'), text: getValueFromProperty(item, 'Name') };
        });
        if (options.length <= 0) {
            return null;
        }
        return (
            <BasicDropdown
                options={options}
                selectedKey={reasonCode}
                ariaLabel={
                    isFirstNotesRender
                        ? 'new dialog ' + defaultConfirmationMessage + ' select justification'
                        : 'Select justification'
                }
                label="Justification"
                onChange={handleJustificationSelect}
                required={true}
                styles={DetailsStyled.SmallDropdownStyles}
                componentRef={(input: any): void => (justificationRef = input)}
            />
        );
    };

    const getConditionPropertyObject = (): object => {
        let conditionPropertyObject = {};
        if (headerDetailsJSON) {
            conditionPropertyObject = Object.assign(conditionPropertyObject, headerDetailsJSON, {
                isMicrofrontendOpen: isMicrofrontendOpen,
            });
        }
        const flatConditionPropertyObject = flattenObject(conditionPropertyObject);
        return flatConditionPropertyObject;
    };

    const splitByKey = (justifications: any, isBulk: boolean): [object[], boolean] => {
        const includedKeys: any = [];
        const res = [];
        let isJustificationRequired = false;
        for (let i = 0; i < justifications.length; i++) {
            const curKey = getValueFromProperty(justifications[i], 'key');
            if (!includedKeys.includes(curKey)) {
                const curItem = {
                    key: curKey,
                    values: [justifications[i]],
                    isApplicable: isJustificationApplicableinSummary(isBulk) || testRegexIgnoreCase('^reject', curKey),
                };
                res.push(curItem);
                includedKeys.push(curKey);
                if (curItem.isApplicable) {
                    isJustificationRequired = true;
                }
            } else {
                const ind = includedKeys.indexOf(curKey);
                const valuesWithKey = res[ind].values;
                valuesWithKey.push(justifications[i]);
            }
        }
        return [res, isJustificationRequired];
    };

    const mapCommentsByKey = (comments: any, justifications: object[]): [object[], boolean] => {
        const res = [];
        let areCommentsRequired = false;
        for (let i = 0; i < comments.length; i++) {
            const curMappingKey = getValueFromProperty(comments[i], 'mappingKey');
            const mappedJustificationInd = justifications.findIndex((item: any) => curMappingKey.includes(item.key));
            const mappedJustification: any =
                typeof mappedJustificationInd === 'number' && justifications[mappedJustificationInd];
            const curItem: any = comments[i];
            curItem.remainingChars = comments[i]['length'];
            curItem.inputField = '';
            curItem.notesError = '';
            if (mappedJustification) {
                curItem.isApplicable = mappedJustification.isApplicable;
                curItem.key = mappedJustification.key;
                curItem.justificationIndex = mappedJustificationInd;
            } else {
                curItem.isApplicable = true;
            }
            if (curItem.isApplicable) {
                areCommentsRequired = true;
            }
            res.push(curItem);
        }
        return [res, areCommentsRequired];
    };

    const handleEnterNotesAndJustifications = (actionItem: any, isBulk: boolean, isPrimaryAction?: boolean): void => {
        const actionDetailComments = getValueFromProperty(actionItem, 'Comments');
        const isJustificationApplicableInItem = getValueFromProperty(actionItem, 'isJustificationApplicable');
        const isCommentMandatory =
            actionDetailComments?.length > 0
                ? getValueFromProperty(actionDetailComments[0], 'isMandatory')
                : getValueFromProperty(actionItem, 'IsCommentMandatory');
        const commentLength =
            actionDetailComments?.length > 0
                ? getValueFromProperty(actionDetailComments[0], 'Length')
                : getValueFromProperty(actionItem, 'CommentLength');

        const code = getValueFromProperty(actionItem, 'Code');
        const justifications = getValueFromProperty(actionItem, 'Justifications');
        const actionAdditionalInformation = getValueFromProperty(actionItem, 'AdditionalInformation');
        const actionConfirmationMessage = getValueFromProperty(actionItem, 'ActionConfirmationMessage');
        const nameInItem = getValueFromProperty(actionItem, 'Name');
        if (props.executeMicrofrontendActionRef) {
            props.executeMicrofrontendActionRef.actionName = code;
        }
        if (props.executeMicrofrontendActionRef?.actionHandler) {
            try {
                props.executeMicrofrontendActionRef.actionHandler();
            } catch (ex: any) {
                const { stateCommonProperties, componentContext } = props;
                const { authClient, telemetryClient } = componentContext;
                const exception = ex.message ? new Error(ex.message) : ex;
                trackException(
                    authClient,
                    telemetryClient,
                    'Microfrontend Action Event - Failure',
                    'MSApprovals.MicrofrontendActionEvent.Failure',
                    TrackingEventId.MicrofrontendActionEventFailure,
                    stateCommonProperties,
                    exception
                );
            }
        }
        let isJustificationByKeyRequired = false;
        let areCommentsByKeyRequired = false;
        setActionName(nameInItem);
        if (justifications) {
            let defaultOption = justifications.filter((item: object) => (item as any)._default)[0];
            if (!defaultOption) {
                defaultOption = justifications[0];
            }
            const _charLimit = commentLength || null;
            setCode(code);
            setJustificationsList(justifications);
            setReasonCode(getValueFromProperty(defaultOption, 'Code'));
            setReasonText(getValueFromProperty(defaultOption, 'Name'));
            setDisplayNotesInput(true);
            setIsCommentMandatory(isCommentMandatory);
            setActionDetailsComments(actionDetailComments);
            setActionAdditionalInformation(actionAdditionalInformation);
            setSummaryObjectValues(getSummaryObjectValues(actionAdditionalInformation));
            setAdditionalControlValidation(getValidationInfo(actionAdditionalInformation));
            setCharLimit(_charLimit);
            setActionConfirmationMessage(actionConfirmationMessage);
            if (typeof isJustificationApplicableInItem === 'boolean') {
                setIsJustificationApplicableForAction(isJustificationApplicableInItem);
            }

            const _remainingChars = typeof _charLimit === 'number' ? _charLimit - comments.length : null;
            setRemainingChars(_remainingChars);
            if (isPullTenantSelected) {
                const [splitJustifications, isJustificationByKeyApplicable] = splitByKey(justifications, isBulk);
                const [mappedComments, areCommentsByKeyApplicable] = mapCommentsByKey(
                    actionDetailComments,
                    splitJustifications
                );
                isJustificationByKeyRequired = isJustificationByKeyApplicable;
                areCommentsByKeyRequired = areCommentsByKeyApplicable;
                setJustificationsWithKeys(splitJustifications);
                setCommentsWithKeys(mappedComments);
            }
        } else {
            const _charLimit = commentLength || null;

            setCode(code);
            setDisplayNotesInput(true);
            setIsCommentMandatory(isCommentMandatory);
            setActionDetailsComments(actionDetailComments);
            setActionAdditionalInformation(actionAdditionalInformation);
            setSummaryObjectValues(getSummaryObjectValues(actionAdditionalInformation));
            setAdditionalControlValidation(getValidationInfo(actionAdditionalInformation));
            setCharLimit(_charLimit);
            setActionConfirmationMessage(actionConfirmationMessage);

            const _remainingChars = typeof _charLimit === 'number' ? _charLimit - comments.length : null;
            setRemainingChars(_remainingChars);
        }
        //FOR ONE CLICK APPROVAL
        const isJustificationRequired = isPullTenantSelected
            ? isJustificationByKeyRequired
            : justifications && justifications.length > 0;
        const areCommentsRequired = isPullTenantSelected
            ? areCommentsByKeyRequired
            : actionDetailComments || actionAdditionalInformation;
        if (isPrimaryAction && !isJustificationRequired && !areCommentsRequired) {
            const oneClickSubmission = {
                code,
            };
            if (isBulk) {
                handleApprovals(oneClickSubmission);
            } else {
                handleSubmitButton(oneClickSubmission);
            }
        }
    };

    const clearNotesPopupData = () => {
        setCode('');
        setReasonCode('');
        setReasonText('');
        setJustificationsList(null);
        setComments('');
        setIsCommentMandatory(false);
        setNotesErrorMessage('');
        setCharLimit(null);
        setDisplayNotesInput(false);
        setReceiptsCheck(false);
        setCorruptionCheck(false);
        setDisplayReceiptError(false);
        setDisplayCorruptionError(false);
        setDisplayReceiptCheckbox(false);
        setDisplayCorruptionCheckbox(false);
        setSequenceID('');
        setNextApprover('');
        setSummaryObjectValues(null);
        setActionConfirmationMessage('');
        setAdditionalControlValidation([]);
        setDigitalSignature('');
        const _remainingChars = typeof charLimit === 'number' ? charLimit - comments.length : null;
        setRemainingChars(_remainingChars);
    };

    const clearAllActionData = (): void => {
        clearNotesPopupData();
        dispatch(updatePeoplePickerSelection([]));
        dispatch(updatePeoplePickerHasError(false));
    };

    const validateAdditionalControls = (): boolean => {
        let result = true;
        if (actionAdditionalInformation && actionAdditionalInformation.length > 0) {
            let newValidationArray = [...additionalControlValidation];
            for (let i = 0; i < actionAdditionalInformation.length; i++) {
                if (additionalControlValidation[i].isMandatory) {
                    const control: IAddditionalInformation = actionAdditionalInformation[i];
                    if (control.Type === 'peoplePicker') {
                        if (peoplePickerSelections.length <= 0) {
                            newValidationArray[i].isValid = false;
                            if (!peoplePickerHasError) {
                                dispatch(updatePeoplePickerHasError(true));
                            }
                            result = false;
                        } else {
                            //valid
                            newValidationArray[i].isValid = true;
                            if (peoplePickerHasError) {
                                dispatch(updatePeoplePickerHasError(false));
                            }
                        }
                    } else if (control.Type === 'select' || control.Type === 'textfield') {
                        const valueToValidate =
                            control.Code === 'addApproverPlacement'
                                ? sequenceID
                                : control.Code === 'addNextApprover'
                                ? nextApprover
                                : digitalSignature;
                        const errorMessage = inputValidationErrors[control.Code];
                        let IsInvalid = false;
                        if (
                            additionalControlValidation[i].expectedValue &&
                            additionalControlValidation[i].expectedValue !== ''
                        ) {
                            IsInvalid = valueToValidate !== additionalControlValidation[i].expectedValue;
                        } else {
                            IsInvalid = !valueToValidate || valueToValidate === '';
                        }
                        if (IsInvalid) {
                            newValidationArray[i].isValid = false;
                            newValidationArray[i].errorMessage = errorMessage;
                            result = false;
                        } else {
                            newValidationArray[i].isValid = true;
                            newValidationArray[i].errorMessage = '';
                        }
                    }
                }
            }
            setAdditionalControlValidation(newValidationArray);
        }
        return result;
    };

    const validateNotesandJustification = () => {
        let res = true;
        if (isPullTenantSelected) {
            if (justificationsWithKeys) {
                const newJustifications = [...justificationsWithKeys];
                for (let i = 0; i < justificationsWithKeys.length; i++) {
                    const justificationItem = justificationsWithKeys[i];
                    const newItem = newJustifications[i];
                    if (justificationItem.isApplicable) {
                        const selection = justificationItem.selectedIndex;
                        if (!(typeof selection === 'number' && selection >= 0)) {
                            res = false;
                            newItem.dropdownError = 'No ' + startCase(justificationItem.key) + ' reason selected';
                        }
                    }
                }
                setJustificationsWithKeys(newJustifications);
            }
            if (commentsWithKeys) {
                const newComments = [...commentsWithKeys];
                for (let i = 0; i < commentsWithKeys.length; i++) {
                    const commentItem = commentsWithKeys[i];
                    const newItem = newComments[i];
                    if (commentItem.isApplicable) {
                        const commentValue = commentItem.inputField;
                        const isCommentWhitespace = !commentValue.replace(/\s/g, '');
                        const mappedItem = justificationsWithKeys?.[commentItem.justificationIndex];
                        const justificationIndex = mappedItem.selectedIndex;
                        const selectedJustificationObj =
                            typeof justificationIndex === 'number' && mappedItem?.values?.[mappedItem.selectedIndex];
                        const isRequired = selectedJustificationObj
                            ? validateConditionClient(selectedJustificationObj, commentItem.mandatoryCondition)
                            : false;
                        if (isRequired && isCommentWhitespace) {
                            res = false;
                            newItem.notesError = startCase(commentItem.key) + ' notes are required';
                        }
                    }
                }
                setCommentsWithKeys(newComments);
            }
        } else {
            const isCommentWhitespace = !comments.replace(/\s/g, '');
            if (isCommentMandatory && isCommentWhitespace) {
                setNotesErrorMessage('Notes cannot be blank');
                res = false;
            }
            if (isCommentExceeded) {
                res = false;
            }
        }
        return res;
    };

    const validateRequiredFields = () => {
        let requiredFieldsProvided = true;
        const areNotesandJustificationValid = validateNotesandJustification();
        if (!areNotesandJustificationValid) {
            requiredFieldsProvided = false;
        }
        if (actionAdditionalInformation) {
            if (displayReceiptCheckbox && !receiptsCheck) {
                requiredFieldsProvided = false;
                setDisplayReceiptError(true);
            }
            if (displayCorruptionCheckbox && !corruptionCheck) {
                requiredFieldsProvided = false;
                setDisplayCorruptionError(true);
            }
        }
        const areAdditionalControlsValid = validateAdditionalControls();
        if (!areAdditionalControlsValid) {
            requiredFieldsProvided = false;
        }
        return requiredFieldsProvided;
    };

    const handleAdditionalActionDetails = () => {
        let res: any = {};
        for (let i = 0; i < justificationsWithKeys.length; i++) {
            const curItem = justificationsWithKeys[i];
            if (curItem.isApplicable) {
                const ind = curItem.selectedIndex;
                const key = curItem.key;
                const lowerKey = lowerCaseFirstLetter(key);
                if (typeof ind === 'number' && ind >= 0) {
                    const justificiationValue = curItem.values[ind];
                    const codeKey = lowerKey + 'ReasonCode';
                    const textKey = lowerKey + 'ReasonText';
                    res[codeKey] = justificiationValue.code;
                    res[textKey] = key;
                    res['reasonText'] = key;
                    res['reasonCode'] = justificiationValue.code;
                }
            }
        }
        for (let i = 0; i < commentsWithKeys.length; i++) {
            const curItem = commentsWithKeys[i];
            if (curItem.isApplicable) {
                const key = lowerCaseFirstLetter(curItem.key);
                const inputVal = curItem.inputField;
                const commentKey = key + 'Comment';
                res[commentKey] = inputVal;
                res['comment'] = inputVal;
            }
        }
        return res;
    };

    const handleSubmitButton = (oneClickSubmission?: { code: string }): void => {
        // microfrontend event call
        //checking if the tenant has a custom UI and needs additional submission validation
        let isSubmissionValidated =
            !isMicrofrontendOpen && !areDetailsEditable && detailsComponentType !== DetailsType.Microfrontend;
        if (!isSubmissionValidated && executeMicrofrontendActionRef.submitHandler) {
            try {
                isSubmissionValidated = executeMicrofrontendActionRef.submitHandler();
                if (isSubmissionValidated && areDetailsEditable && code.toLowerCase() === 'msapprovalsedit') {
                    dispatch(postEditableDetails(props.tenantId, props.userAlias));
                    dispatch(updateAdditionalData(null));
                    clearAllActionData();
                    return;
                }
            } catch (ex: any) {
                const { stateCommonProperties, componentContext } = props;
                const { authClient, telemetryClient } = componentContext;
                const exception = ex.message ? new Error(ex.message) : ex;
                trackException(
                    authClient,
                    telemetryClient,
                    'Microfrontend Submit Event - Failure',
                    'MSApprovals.MicrofrontendSubmitEvent.Failure',
                    TrackingEventId.MicrofrontendSubmitEventFailure,
                    stateCommonProperties,
                    exception
                );
            }
        }
        const additionalActionDetails = isPullTenantSelected ? handleAdditionalActionDetails() : null;
        if (isSubmissionValidated && validateRequiredFields()) {
            const {
                tenantId,
                documentNumber,
                displayDocumentNumber,
                fiscalYear,
                dispatchPostAction,
                documentTypeId,
                businessProcessName,
                userAlias,
                setActionComplete,
            } = props;

            const submission = {
                tenantId,
                documentNumber,
                displayDocumentNumber,
                fiscalYear,
                code,
                reasonCode,
                reasonText,
                comments,
                documentTypeId,
                businessProcessName,
                receiptsCheck,
                corruptionCheck,
                sequenceID,
                nextApprover,
                peoplePickerSelections,
                additionalActionDetails,
                digitalSignature,
            };
            if (oneClickSubmission && oneClickSubmission.code) {
                submission.code = oneClickSubmission.code;
            }
            if (isPullTenantSelected) {
                const loggedInUpn = profile?.userPrincipalName ?? '';
                const loggedInAlias = loggedInUpn.substring(0, loggedInUpn.indexOf('@'));
                const originalApprover = selectedTenantDelegation
                    ? selectedTenantDelegation.alias
                    : userAlias
                    ? userAlias
                    : loggedInAlias;
                const summaryObj = generatePullModelSummary(tenantDataModel, summaryJSON);
                const summaryJSONObj = generateSummaryObjForPullTenant(summaryObj, originalApprover, summaryJSON);
                const additionalData = {
                    etag: summaryJSON?.eTag,
                    submittedFor: summaryJSON?.submittedFor,
                    partner: summaryJSON?.partner,
                    isLateApproval: summaryJSON?.isLateApproval,
                    summaryJSON: summaryJSON ? JSON.stringify(summaryJSONObj) : '',
                };
                dispatch(updateAdditionalData(additionalData));
            }
            dispatchPostAction(submission, userAlias, false, isPullTenantSelected);
            setActionComplete(false);
            clearNotesPopupData();
            dispatch(updatePeoplePickerSelection([]));
            dispatch(updatePeoplePickerHasError(false));
        }
    };

    const handleCancelButton = () => {
        // microfrontend event call
        if (props.executeMicrofrontendActionRef?.cancelHandler) {
            try {
                props.executeMicrofrontendActionRef.cancelHandler();
            } catch (ex: any) {
                const { stateCommonProperties, componentContext } = props;
                const { authClient, telemetryClient } = componentContext;
                const exception = ex.message ? new Error(ex.message) : ex;
                trackException(
                    authClient,
                    telemetryClient,
                    'Microfrontend Cancel Event - Failure',
                    'MSApprovals.MicrofrontendCancelEvent.Failure',
                    TrackingEventId.MicrofrontendCancelEventFailure,
                    stateCommonProperties,
                    exception
                );
            }
        }
        clearNotesPopupData();
        dispatch(updatePeoplePickerSelection([]));
        dispatch(updatePeoplePickerHasError(false));
        if (primaryActionRef) {
            primaryActionRef.focus();
        }
    };

    const getValueFromProperty: any = (item: any, key: string) => {
        return item?.[Object.keys(item).find((k) => k.toLowerCase() === key.toLowerCase())];
    };

    const renderPrimaryButtons = (primaryActionData: object[], isBulk: boolean) => {
        try {
            const conditionPropertyObject = getConditionPropertyObject();
            //defining because 'this' property cannot be referenced inside of reduce
            const filteredButtonItems: object[] = primaryActionData.reduce(function (filtered: any, item, index) {
                const condition = getValueFromProperty(item, 'Condition');
                if (
                    (condition == null || condition == '' || validateCondition(conditionPropertyObject, condition)) &&
                    validateBulkActionButton(item, isBulk)
                ) {
                    const actionButtonItem = {
                        key: getValueFromProperty(item, 'Code'),
                        text: getValueFromProperty(item, 'Name'),
                        title: getValueFromProperty(item, 'Name'),
                        enableActionButtons: getValueFromProperty(item, 'IsEnabled'),
                        onClick: (): void => {
                            handleEnterNotesAndJustifications(item, isBulk, true);
                        },
                        aiEventName: getValueFromProperty(item, 'Name'),
                        primary: index === 0,
                    };
                    filtered.push(actionButtonItem);
                }
                return filtered;
            }, []);
            const generateFilteredButtons: object[] = (filteredButtonItems as any).map(
                (item: object, index: number) => {
                    return (
                        <BasicButton
                            className="primarybuttonsClass"
                            text={(item as any).text}
                            title={(item as any).title}
                            onClick={(item as any).onClick}
                            primary={(item as any).primary}
                            aiEventName={(item as any).aiEventName}
                            disabled={
                                !(item as any).enableActionButtons ||
                                (isBulk
                                    ? selectedApprovalRecords.length > bulkActionConcurrentCall
                                    : isControlsAndComplianceRequired && !isRequestFullyScrolled)
                            }
                            componentRef={(input: any) => {
                                if (index === 0 && input) {
                                    primaryActionRef = input;
                                }
                            }}
                        />
                    );
                }
            );
            return generateFilteredButtons.map((item: any) => <Stack.Item>{item}</Stack.Item>);
        } catch (ex: any) {
            const { stateCommonProperties, componentContext } = props;
            const { authClient, telemetryClient } = componentContext;
            const exception = ex.message ? new Error(ex.message) : ex;
            trackException(
                authClient,
                telemetryClient,
                'Render primary action buttons - Failure',
                'MSApprovals.RenderPrimaryActionButtons.Failure',
                TrackingEventId.RenderPrimaryActionButtonsFailure,
                stateCommonProperties,
                exception
            );
            return null;
        }
    };

    if (areDetailsEditable && detailsJSON) {
        actionDetails?.Secondary?.push({
            Condition: '',
            Code: 'MSApprovalsEdit',
            Name: 'Edit',
            Text: '',
            Justifications: null,
            IsCommentMandatory: false,
            IsJustificationApplicable: true,
            IsInterimStateRequired: true,
            CommentLength: 255,
            TargetPage: [
                {
                    Condition: '',
                    PageType: 'SUMMARY',
                    DelayTime: 0,
                },
            ],
            Placements: null,
            AdditionalInformation: null,
            IsEnabled: true,
            IsBulkAction: true,
            ActionConfirmationMessage: null,
            Comments: null,
        });
        let empty: any = [];

        actionDetails.Secondary = actionDetails?.Secondary?.filter(function (o: any) {
            if (empty.indexOf(o['Name']) !== -1) return false;
            empty.push(o['Name']);
            return true;
        });
    }

    const validateBulkActionButton = (item: any, isBulk: boolean) => {
        if (!isBulk) {
            return true;
        }
        return getValueFromProperty(item, 'isBulkAction') && isBulk;
    };

    const renderSecondaryButtons = (secondaryActionData: object[], isBulk: boolean) => {
        try {
            const conditionPropertyObject = getConditionPropertyObject();
            //defining because 'this' property cannot be referenced inside of reduce
            const filteredMenuItems: IContextualMenuItem[] = secondaryActionData.reduce(function (filtered: any, item) {
                if (
                    (getValueFromProperty(item, 'Condition') == null ||
                        getValueFromProperty(item, 'Condition') == '' ||
                        validateCondition(conditionPropertyObject, getValueFromProperty(item, 'Condition'))) &&
                    validateBulkActionButton(item, isBulk)
                ) {
                    const menuItem = {
                        key: getValueFromProperty(item, 'Code'),
                        text: getValueFromProperty(item, 'Name') + ' ' + getValueFromProperty(item, 'Text'),
                        title: getValueFromProperty(item, 'Name'),
                        disabled:
                            !getValueFromProperty(item, 'IsEnabled') ||
                            (isControlsAndComplianceRequired && !isRequestFullyScrolled),
                        onClick: (): void => handleEnterNotesAndJustifications(item, isBulk, false),
                        aiEventName: getValueFromProperty(item, 'Name'),
                    };
                    filtered.push(menuItem);
                }
                return filtered;
            }, []);
            return (
                filteredMenuItems.length > 0 && (
                    <DropdownButton
                        text="Other Actions"
                        title="Other Actions"
                        className="footer-action-button"
                        menuItems={filteredMenuItems}
                    />
                )
            );
        } catch (ex: any) {
            const { stateCommonProperties, componentContext } = props;
            const { authClient, telemetryClient } = componentContext;
            const exception = ex.message ? new Error(ex.message) : ex;
            trackException(
                authClient,
                telemetryClient,
                'Render secondary action buttons - Failure',
                'MSApprovals.RenderSecondaryActionButtons.Failure',
                TrackingEventId.RenderSecondaryActionButtonsFailure,
                stateCommonProperties,
                exception
            );
            return null;
        }
    };

    const renderNotes = (isBulk: boolean) => {
        return (
            <Stack.Item>
                <MultilineTextField
                    label="Notes"
                    ariaRequired={true}
                    ariaLabelledby={`Notes edit multiline ${remainingChars} characters remaining`}
                    ariaLabel={`Notes edit multiline ${remainingChars} characters remaining`}
                    value={comments}
                    onChange={handleComments}
                    rows={2}
                    required={isCommentMandatory}
                    inputErrorMessage={notesErrorMessage}
                    componentRef={(input: any) => {
                        input && !justificationsList && !actionAdditionalInformation && input.focus();
                    }}
                    inputStyle={{ fontSize: '2px' }}
                    styles={DetailsStyled.BulkSmallTextFieldStyles(isBulk)}
                />
                {(remainingChars || remainingChars === 0) && remainingChars === 1 ? (
                    <div
                        aria-label={`${remainingChars} character remaining`}
                    >{`${remainingChars} character remaining`}</div>
                ) : (
                    <div aria-label={`${remainingChars} characters remaining`}>
                        {`${remainingChars} characters remaining`}
                    </div>
                )}
            </Stack.Item>
        );
    };

    const renderBulkApprovalMessage = () => {
        return (
            <Stack.Item styles={DetailsStyled.bulkMessageStyle}>
                <p>
                    You have selected {selectedApprovalRecords.length} out of max allowed {bulkActionConcurrentCall}{' '}
                    record(s) to take bulk action.
                </p>
            </Stack.Item>
        );
    };

    const renderAdditionalNotes = (isBulk: boolean) => {
        const cleanNotes = sanitizeHtml(tenantAdditionalNotes, {
            allowedTags: ['a'],
            allowedAttributes: {
                a: ['href', 'target'],
            },
        });
        return (
            <Stack.Item styles={DetailsStyled.additionalNotesStyle(isBulk)}>
                <div dangerouslySetInnerHTML={{ __html: cleanNotes }} />
            </Stack.Item>
        );
    };

    const handleCommentsWithKey = (e: object, commentIndex: number, isMandatory: boolean): void => {
        const commentItem = commentsWithKeys[commentIndex];
        const commentCharLimit = commentItem['length'];
        const { value } = (e as any).target;
        if (commentCharLimit - value.length < 0) {
            return;
        }
        const notesError = commentItem.notesError;
        const newComments = [...commentsWithKeys];
        newComments[commentIndex].inputField = value;
        const isCommentWhitespace = !value.replace(/\s/g, '');
        if (notesError && isMandatory && !isCommentWhitespace) {
            newComments[commentIndex].notesError = '';
        }
        const _remainingChars = typeof commentCharLimit === 'number' ? commentCharLimit - value.length : null;
        newComments[commentIndex].remainingChars = _remainingChars;
        setCommentsWithKeys(newComments);
    };

    const handleJustificationSelectWithKey = (selection: IDropdownOption, justificationIndex: number): void => {
        const selectedIndex = selection.key;
        const newJustifications = [...justificationsWithKeys];
        newJustifications[justificationIndex].selectedIndex = selectedIndex;
        newJustifications[justificationIndex].dropdownError = '';
        setJustificationsWithKeys(newJustifications);
    };

    const renderDropdownWithKey = (justificationItem: any, itemIndex: number) => {
        const vals = justificationItem?.values;
        const chosenIndex = justificationsWithKeys?.[itemIndex]?.selectedIndex;
        const options: IDropdownOption[] = vals.map((item: object, index: number) => {
            return { key: index, text: getValueFromProperty(item, 'Name') };
        });
        return (
            <Stack>
                <BasicDropdown
                    options={options}
                    selectedKey={chosenIndex ?? undefined}
                    label={startCase(justificationItem.key) + ' Reason'}
                    onChange={(e: object, selection: IDropdownOption) =>
                        handleJustificationSelectWithKey(selection, itemIndex)
                    }
                    required={true}
                    styles={DetailsStyled.SmallDropdownStyles}
                    errorMessage={justificationItem.dropdownError}
                />
                {typeof chosenIndex === 'number' && (
                    <label className="ms-Label">{vals?.[chosenIndex]?.['description']}</label>
                )}
            </Stack>
        );
    };

    const renderNotesWithKey = (
        commentItem: any,
        itemIndex: number,
        justificationItem: any,
        isBulk: boolean
    ): JSX.Element => {
        const remainingCharsforItem = commentItem.remainingChars;
        const justificationIndex = justificationItem.selectedIndex;
        const selectedJustificationObj = justificationItem.values[justificationIndex];
        const isRequired = selectedJustificationObj
            ? validateConditionClient(selectedJustificationObj, commentItem.mandatoryCondition)
            : false;
        const isMandatory = commentItem.isMandatory || isRequired;
        return (
            <Stack.Item>
                <MultilineTextField
                    label={commentItem.key ? startCase(commentItem.key) + ' Notes' : 'Notes'}
                    ariaRequired={true}
                    value={commentItem.inputField}
                    onChange={(e: any) => handleCommentsWithKey(e, itemIndex, isMandatory)}
                    rows={2}
                    required={isMandatory}
                    inputErrorMessage={commentItem.notesError}
                    inputStyle={{ fontSize: '2px' }}
                    styles={DetailsStyled.BulkSmallTextFieldStyles(isBulk)}
                />
                {(remainingCharsforItem || remainingCharsforItem === 0) && remainingCharsforItem === 1 ? (
                    <div
                        aria-label={`${remainingCharsforItem} character remaining`}
                    >{`${remainingCharsforItem} character remaining`}</div>
                ) : (
                    <div aria-label={`${remainingCharsforItem} characters remaining`}>
                        {`${remainingCharsforItem} characters remaining`}
                    </div>
                )}
            </Stack.Item>
        );
    };

    const renderJustificationsandNotesWithKeys = (isBulk: boolean): JSX.Element[] => {
        const inputs = justificationsWithKeys.map((item, index) => {
            if (!item.isApplicable) {
                return null;
            }
            const key = item.key;
            const commentsIndex = commentsWithKeys.findIndex((item) => item.key === key);
            return (
                <Stack key={index}>
                    {renderDropdownWithKey(item, index)}
                    {renderNotesWithKey(commentsWithKeys[commentsIndex], commentsIndex, item, isBulk)}
                </Stack>
            );
        });
        return inputs;
    };

    const renderSubmitAndCacelPanel = (isBulk: boolean) => {
        const isButtonDisabled = isBulk ? selectedApprovalRecords.length > bulkActionConcurrentCall : false;
        const confirmationEnding = isBulk ? 'request(s)' : 'request';
        const confirmationCode = actionName && actionName.length > 0 ? actionName.toLowerCase() : 'submit';
        const defaultConfirmationMessage =
            'Are you sure you would like to ' + confirmationCode + ' the following ' + confirmationEnding + '?';

        return (
            <Stack tokens={stackTokens}>
                <Stack tokens={stackTokens} className="approve-note-input">
                    <>
                        <Stack.Item styles={DetailsStyled.bulkMessageStyle}>
                            <h3 className="h3-width" tabIndex={0}>
                                {actionConfirmationMessage && actionConfirmationMessage != ''
                                    ? actionConfirmationMessage
                                    : defaultConfirmationMessage}
                            </h3>
                            {isBulk && <div style={{ height: '2px' }} />}

                            {isBulk && renderBulkApprovalMessage()}

                            {actionAdditionalInformation &&
                                renderAdditionalInformation(actionAdditionalInformation, isBulk)}
                            {!isPullTenantSelected && justificationsList && renderDropDown(defaultConfirmationMessage)}
                            {!isPullTenantSelected && actionDetailsComments && renderNotes(isBulk)}
                            {isPullTenantSelected && renderJustificationsandNotesWithKeys(isBulk)}
                        </Stack.Item>
                    </>
                </Stack>
                <Stack horizontal wrap tokens={stackTokens}>
                    <Stack.Item>
                        <BasicButton
                            primary={true}
                            text="Submit"
                            title="Submit"
                            disabled={isButtonDisabled}
                            onClick={isBulk ? handleApprovals : handleSubmitButton}
                        />
                    </Stack.Item>
                    <Stack.Item>
                        <BasicButton text="Cancel" title="Cancel" onClick={handleCancelButton} />
                    </Stack.Item>
                </Stack>
            </Stack>
        );
    };

    const renderFooter = (isBulk: boolean) => {
        return (
            <Stack tokens={stackTokens} styles={DetailsStyled.submitAndCancelFormStyle}>
                {!displayNotesInput ? (
                    <>
                        {isBulk && renderBulkApprovalMessage()}
                        <Stack horizontal tokens={stackTokens} styles={DetailsStyled.buttonZoomStyle}>
                            {getValueFromProperty(actionDetails, 'Primary') &&
                                renderPrimaryButtons(getValueFromProperty(actionDetails, 'Primary'), isBulk)}
                            {getValueFromProperty(actionDetails, 'Secondary') && (
                                <Stack.Item>
                                    {renderSecondaryButtons(getValueFromProperty(actionDetails, 'Secondary'), isBulk)}
                                </Stack.Item>
                            )}
                        </Stack>
                        {tenantAdditionalNotes && renderAdditionalNotes(isBulk)}
                    </>
                ) : (
                    <Stack tokens={stackTokens}>{renderSubmitAndCacelPanel(isBulk)}</Stack>
                )}
            </Stack>
        );
    };

    const handleApprovals = (oneClickSubmission?: { code: string }): void => {
        if (validateRequiredFields()) {
            const { userAlias } = props;
            const tenantId = getTenantID();
            const additionalActionDetails = isPullTenantSelected ? handleAdditionalActionDetails() : null;
            const submission = {
                tenantId,
                code,
                reasonCode,
                reasonText,
                comments,
                receiptsCheck,
                corruptionCheck,
                sequenceID,
                nextApprover,
                peoplePickerSelections,
                additionalActionDetails,
            };
            if (oneClickSubmission && oneClickSubmission.code) {
                submission.code = oneClickSubmission.code;
            }
            dispatch(updateFailedPullTenantRequests([]));
            dispatch(postAction(submission, userAlias, true, isPullTenantSelected));
            dispatch(updateIsProcessingBulkApproval(true));
            dispatch(updateBulkStatus(false));
            dispatch(updatePanelState(false));
            clearNotesPopupData();
        }
    };

    const getTenantID: any = () => {
        const selectedTenant = tenantInfo.find((tenant: any) => tenant.appName === filterValue);
        return selectedTenant?.tenantId;
    };

    return actionDetails ? (
        <div>
            {props.setFooterRef ? (
                <DetailsStyled.Footer
                    id="detailCardFooter"
                    ref={(element: any) => props.setFooterRef(element)}
                    windowHeight={props.windowHeight}
                    windowWidth={props.windowWidth}
                >
                    <DetailsStyled.SmallSpace />
                    {renderFooter(false)}
                    <DetailsStyled.SmallSpace className="ms-hiddenSm" />
                </DetailsStyled.Footer>
            ) : (
                <DetailsStyled.primaryStyle
                    id="detailCardFooter"
                    isPanel={isPanelOpen}
                    isMaximized={toggleDetailsScreen}
                    ref={(element: any) => props.setBulkFooterRef(element)}
                    windowHeight={props.windowHeight}
                    windowWidth={props.windowWidth}
                >
                    {renderFooter(true)}
                </DetailsStyled.primaryStyle>
            )}
        </div>
    ) : null;
});

export default DetailsFooter;
