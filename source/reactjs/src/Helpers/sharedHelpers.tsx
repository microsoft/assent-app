import { forEach, set } from 'lodash';
import { useHistory } from 'react-router-dom';
import { breakpointMap } from '../Components/Shared/Styles/Media';

export const imitateClickOnKeyPressForDiv = (
    onClick: VoidFunction
): ((e: React.KeyboardEvent<HTMLDivElement>) => void) => {
    const onKeyPress = (e: any): void => {
        const enterOrSpace =
            e.key === 'Enter' || e.key === ' ' || e.key === 'Spacebar' || e.which === 13 || e.which === 32;
        const isCheckBoxClicked =
            e.target?.getAttribute('class')?.indexOf('Checkbox') > -1 ||
            e.target?.getAttribute('type') == 'checkbox' ||
            e.target?.getAttribute('data-icon-name') == 'CheckMark'
                ? true
                : false;
        if (!isCheckBoxClicked && enterOrSpace) {
            e.preventDefault();
            onClick();
        }
    };
    return onKeyPress;
};

export const imitateClickOnKeyPressForAnchor = (
    onClick: VoidFunction
): ((e: React.KeyboardEvent<HTMLAnchorElement>) => void) => {
    const onKeyPress = (e: React.KeyboardEvent<HTMLAnchorElement>): void => {
        const enterOrSpace =
            e.key === 'Enter' || e.key === ' ' || e.key === 'Spacebar' || e.which === 13 || e.which === 32;
        if (enterOrSpace) {
            e.preventDefault();
            onClick();
        }
    };
    return onKeyPress;
};

export const isMobileResolution = (width: number): boolean => {
    return width <= breakpointMap.l;
};

export const toCamelCase = (text: string): string => {
    return text.replace(/^([A-Z])|[\s-_]+(\w)/g, function (match, p1, p2) {
        if (p2) return p2.toUpperCase();
        return p1.toLowerCase();
    });
};

export const flattenObject = (ob: any): any => {
    let toReturn = {} as any;
    let flatObject;
    for (const i in ob) {
        if (!ob.hasOwnProperty(i)) {
            continue;
        }
        if (typeof ob[i] === 'object') {
            flatObject = flattenObject(ob[i]);
            for (const x in flatObject) {
                if (!flatObject.hasOwnProperty(x)) {
                    continue;
                }
                toReturn[i + '.' + x] = flatObject[x];
            }
        } else {
            toReturn[i] = ob[i];
        }
    }
    return toReturn;
};

export const validateConditionClient = (propertyObject: any, condition: string): boolean => {
    const conditionPhrases = condition.match(/(\w+[^\w\s]\w+)|(\w+\s)/g);
    if (conditionPhrases) {
        conditionPhrases.forEach((conditionPhrase) => {
            const specialChar: any = conditionPhrase.match(/[^\w\s]/g);
            const conditionPhraseSplit = conditionPhrase.split(specialChar);
            let propertyName = conditionPhrase;
            if (conditionPhraseSplit.length > 1) {
                propertyName = conditionPhraseSplit[1];
            }
            //properties inside additionaldata shouldn't be in camel case
            if (!propertyName.includes('AdditionalData')) {
                propertyName = toCamelCase(propertyName);
            }
            propertyName = propertyName.trim();
            condition = condition.replace(conditionPhrase.trim(), '"' + propertyObject[propertyName] + '"');
        });
    }
    const result = new Function('return ' + condition)();
    return result;
};

export const validateCondition = (propertyObject: any, condition: string): boolean => {
    const conditionParts = condition.split('^');
    if (conditionParts.length > 1) {
        const key = conditionParts[0];
        condition = conditionParts[1];
        if (key === '_client') {
            const conditionPhrases = condition.match(/(\w+[^\w\s]\w+)|(\w+\s)/g);
            if (conditionPhrases) {
                conditionPhrases.forEach((conditionPhrase) => {
                    let propertyName = conditionPhrase;
                    //properties inside additionaldata shouldn't be in camel case
                    if (!propertyName.includes('AdditionalData')) {
                        propertyName = toCamelCase(propertyName);
                    }
                    propertyName = propertyName.trim();
                    condition = condition.replace(conditionPhrase.trim(), '"' + propertyObject[propertyName] + '"');
                });
            }
            const result = new Function('return ' + condition)();
            return result;
        }
    }
    return true;
};

export const testRegexIgnoreCase = (regex: string, val: string): boolean => {
    return new RegExp(regex, 'i').test(val);
};

const getNestedValue = (obj: any, key: string): any => {
    if (key && typeof key === 'string') {
        if (key.includes(' ')) {
            return key;
        }
        const curValue = key.includes('.')
            ? key.split('.').reduce(function (p, prop) {
                  return p?.[prop];
              }, obj)
            : obj[key];
        return curValue;
    } else {
        return null;
    }
};

export const flattenObj = (ob: any, isArrayFlattened?: boolean): object => {
    let result: any = {};
    for (const i in ob) {
        const isNonArrayObject = typeof ob[i] === 'object' && !Array?.isArray(ob[i]);
        const isArrayRequiresFlatten = Array?.isArray(ob[i]) && isArrayFlattened;
        if (isNonArrayObject || isArrayRequiresFlatten) {
            const temp: any = flattenObj(ob[i], isArrayFlattened);
            for (const j in temp) {
                result[i + '.' + j] = temp[j];
            }
        } else {
            result[i] = ob[i];
        }
    }
    return result;
};

const populateCustomProperties = (summaryObj: any, headerJSON: any, mappingObj: any): void => {
    const approvers = summaryObj.ApprovalHierarchy;
    const curApprover = {
        Alias: getNestedValue(headerJSON, mappingObj.ApproverAlias),
        Name: getNestedValue(headerJSON, mappingObj.ApproverName),
        _future: false,
        Action: '',
    };
    if (Array.isArray(approvers)) {
        for (let i = 0; i < approvers.length; i++) {
            const item = approvers[i];
            const newItem = {
                Alias: item.alias,
                Name: item.name,
                ApproverType: item.approverType,
                Action: item.action ? item.action : item.actionType,
                ReasonName: item.reasonName,
                Comments: item.comments,
                _future: false,
                customProperties: [] as any,
            };
            const customProps = [];
            for (const key in item) {
                const newProp = { name: key, value: item[key] };
                customProps.push(newProp);
            }
            newItem.customProperties = customProps;
            summaryObj.Approvers.push(newItem);
        }
    }
    summaryObj.Approvers.push(curApprover);
    if (summaryObj?.Submitter?.Name === mappingObj?.['Submitter.Name']) {
        summaryObj.Submitter.Name = '';
    }
};

export const generatePullModelSummary = (
    summaryDataMapping: string,
    headerJSON: any,
    needCustomProperties?: boolean
): object => {
    const mappingObj = JSON.parse(summaryDataMapping);
    const newVal = {};
    forEach(mappingObj, function (value: any, key: any) {
        set(newVal, key, getNestedValue(headerJSON, value) ?? (needCustomProperties ? value : null));
    });
    if (needCustomProperties) {
        populateCustomProperties(newVal, headerJSON, mappingObj);
    }
    return newVal;
};

export const generateSummaryObjForPullTenant = (dataItem: any, userAlias: string, summaryItem: any): any => {
    const summaryObj: any = {};
    summaryObj.unitValue = dataItem.unitValue;
    summaryObj.unitOfMeasure = dataItem.unitOfMeasure;
    summaryObj.unitValueFormat = dataItem.unitValueFormat
        ? dataItem.unitValueFormat
        : { filterName: '', filterValue: '' };
    summaryObj.documentNumber = dataItem.documentNumber;
    summaryObj.title = dataItem.title;
    summaryObj.description = dataItem.description;
    summaryObj.customAttributeName = dataItem.customAttributeName;
    summaryObj.customAttributeValue = dataItem.customAttributeValue;
    summaryObj.displayDate = dataItem.displayDate;
    summaryObj.submittedDate = dataItem.submittedDate;
    summaryObj.submitterAlias = dataItem.submitterAlias;
    summaryObj.submitterName = dataItem.submitterName;
    summaryObj.approverNotes = dataItem.approverNotes;
    summaryObj.approverAlias = userAlias;
    summaryObj.approverName = dataItem.approverName;
    summaryObj.isCommentsApplicable = dataItem.isCommentsApplicable;
    summaryObj.isJustificationApplicable = dataItem.isJustificationApplicable;
    summaryObj.approvalHierarchy = dataItem.approvalHierarchy;
    summaryObj.attachments = dataItem.attachments;
    summaryObj.companyCode = dataItem.companyCode;
    summaryObj.fiscalYear = dataItem.fiscalYear;
    summaryObj.submittedFor = dataItem.submittedFor ? dataItem.submittedFor : summaryItem.submittedFor ?? null;
    return summaryObj;
};

export const generatePullTenantAdditionalData = (summaryJSON: any, summaryJSONObj: any): any => {
    const additionalData = {
        etag: summaryJSON?.eTag,
        submittedFor: summaryJSON?.submittedFor,
        partner: summaryJSON?.partner,
        isLateApproval: summaryJSON?.isLateApproval,
        summaryJSON: summaryJSON ? JSON.stringify(summaryJSONObj) : '',
    };
    return additionalData;
};

export const generatePullTenantApprovalRequest = (): any => {
    return {};
};

export const lowerCaseFirstLetter = (s: string): string => {
    return s[0].toLowerCase() + s.slice(1);
};

export const isJustificationRequiredForBulk = (selectedRecords: any[]): boolean => {
    let isApplicable = false;
    for (let i = 0; i < selectedRecords.length; i++) {
        if (selectedRecords[i].isLateApproval) {
            isApplicable = true;
        }
    }
    return isApplicable;
};

export const booleanToReadableValue = (val: boolean): string => {
    if (typeof val === 'boolean') {
        return val ? 'Yes' : 'No';
    } else {
        return null;
    }
};

export const safeJSONParse = (str: string): any => {
    let res = null;
    try {
        res = JSON.parse(str);
    } catch (e) {
        return null;
    }
    return res;
};

export const convertKeysToLowercase = (obj: any): any => {
    let key;
    const keys = Object.keys(obj);
    const numKeys = keys.length;
    const newobj: any = {};
    for (let i = 0; i < numKeys; i++) {
        key = keys[i];
        newobj[lowerCaseFirstLetter(key)] = obj[key];
    }
    return newobj;
};

export const formatBusinessProcessName = (name: string, values: string[]): string => {
    let res = name;
    if (res) {
        res = res.replace('{0}', values[0]).replace('{1}', values[1]);
    }
    return res;
};
export const UrlWithQueryParams = (): void => {
    const history = useHistory();
    const queryParams = getUrlParams(window.location.hash);
    const tenantId = queryParams['tenantId'];
    const documentNumber = queryParams['documentNumber'];

    if (tenantId && documentNumber) {
        history.push(`/${tenantId}/${documentNumber}`);
    }
};

const getUrlParams = (hashQuery: any): any => {
    const hashes = hashQuery.slice(hashQuery.indexOf('?') + 1).split('&');
    return hashes.reduce((params: any, hash: any) => {
        const [key, val] = hash.split('=');
        return Object.assign(params, { [key]: decodeURIComponent(val) });
    }, {});
};

export const removeHTMLFromString = (str: string): string => {
    return str?.replace(/(<([^>]+)>)/gi, '') ?? '';
};
