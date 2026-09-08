import * as React from 'react';
import {
    DefaultButton,
    Dialog,
    DialogFooter,
    DirectionalHint,
    DocumentCard,
    DocumentCardActivity,
    DocumentCardDetails,
    DocumentCardPreview,
    DocumentCardTitle,
    DocumentCardType,
    ImageFit,
    PrimaryButton,
    Stack,
    TeachingBubble
} from '@fluentui/react';
import { useBoolean } from '@fluentui/react-hooks';
import * as QuickTourStyled from './QuickTourStyling';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { getIsQuickTourOpen, getItemsRead, getQuickTourData, getUpdatedQuickToursList } from '../../SharedComponents.selectors';
import { clearUnreadQuickTours, postQuickTourInfo, toggleQuickTour } from '../../SharedComponents.actions';
import { IQuickTourSlide } from '../../SharedComponents.types';

export const QuickTour = (props: any): JSX.Element => {
    const {
        hidden,
        unreadOnly
    } = props;

    const { useSelector, dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const [dialogTitle, setDialogTitle] = React.useState('');
    const [dialogSubText, setDialogSubText] = React.useState('');
    const [currentpage, setCurrentPage] = React.useState(0);
    const [teachingBubbleVisible, { toggle: toggleTeachingBubbleVisible }] = useBoolean(false);
    const isQuickTourOpen = useSelector(getIsQuickTourOpen);
    const quickTourData = useSelector(getQuickTourData)
    const readQuickTours = useSelector(getItemsRead)
    const updatedQuickTourList = useSelector(getUpdatedQuickToursList)
    const buttonId = "HelpTopHeaderButton";
    const summaryPresent = quickTourData?.length > 1 ? true : false;

    const [quickTourSlides, setQuickTourSlides] = React.useState<IQuickTourSlide[]>([]);
    const [summaries, setSummaries] = React.useState<any[]>([]);
    const [dimensions, setDimensions] = React.useState({
        height: window.innerHeight,
        width: window.innerWidth,
    });
    const dialogContentProps = {
        styles: QuickTourStyled.dialogContentStyles,
    };
    const modalProps = {
        isBlocking: false,
        topOffsetFixed: true,
        styles: QuickTourStyled.modalPropsStyles,
        scrollableContent: false
    };

    React.useEffect(() => {
        handleResize();
        window.addEventListener('resize', handleResize);

        return (): void => {
            window.removeEventListener('resize', handleResize);
        };
    }, []);

    React.useEffect(() => {
        if (hidden === false) {
            dispatch(toggleQuickTour());
        }
    }, []);

    React.useEffect(() => {
        if (isQuickTourOpen === true) {
            let tempQuickTourSlides: IQuickTourSlide[] = summaryPresent ?
                [
                    {
                        title: 'Summary of New Features',
                        subtext: null,
                        image: null
                    }
                ] : [];
            let tempSummaries: any[] = []
            let previous = 1;


            for (var i = 0; i < quickTourData?.length; i++) {
                if (!quickTourData[i]?.slides) {
                    continue;
                }
                if (unreadOnly) {
                    if (quickTourData[i]?.isViewed == false) {
                        tempQuickTourSlides = tempQuickTourSlides.concat(quickTourData[i]?.slides);
                        tempSummaries = tempSummaries.concat(
                            {
                                summary: quickTourData[i]?.summary,
                                image: quickTourData[i]?.summaryImage,
                                title: quickTourData[i]?.name,
                                target: previous
                            });
                    }
                }
                else {
                    tempQuickTourSlides = tempQuickTourSlides.concat(quickTourData[i]?.slides);
                    tempSummaries = tempSummaries.concat(
                        {
                            summary: quickTourData[i]?.summary,
                            image: quickTourData[i]?.summaryImage,
                            title: quickTourData[i]?.name,
                            target: previous
                        });
                }
                previous += quickTourData[i]?.slides.length;
            }
            setQuickTourSlides(tempQuickTourSlides);
            setSummaries(tempSummaries);
            setCurrentPage(0);
            setDialogTitle(tempQuickTourSlides[0]?.title);
            setDialogSubText(tempQuickTourSlides[0]?.subtext);
            if (tempQuickTourSlides?.length === 0) {
                dispatch(toggleQuickTour());
            }
        }
    }, [isQuickTourOpen]);


    function handleResize() {
        setDimensions({
            height: window.innerHeight,
            width: window.innerWidth,
        });
    }

    function goToPage(page: number): void {
        setCurrentPage(page);
        setDialogTitle(quickTourSlides[page]?.title);
        setDialogSubText(quickTourSlides[page]?.subtext);
    }

    function goToNextPage(): void {
        if (currentpage < quickTourSlides.length - 1) {
            goToPage(currentpage + 1);
        }
    }

    function goToPreviousPage(): void {
        if (currentpage >= 1) {
            goToPage(currentpage - 1);
        }
    }

    function handleDismiss(): void {
        dispatch(toggleQuickTour());
        toggleTeachingBubbleVisible();
        if (unreadOnly) {
            dispatch(postQuickTourInfo(updatedQuickTourList));
            dispatch(clearUnreadQuickTours());
        }
    }

    function renderSummaries(): JSX.Element {
        return (
            <>
                <p>If you want to see a specific feature, you can jump to it by clicking on its tile</p>
                <br />
                <Stack wrap horizontalAlign="center" styles={QuickTourStyled.summaryStackStyles}>
                    {summaries.map((summaryObj: any) =>
                    (
                        <>
                            <Stack.Item title={summaryObj.name}>
                                <DocumentCard
                                    aria-label={summaryObj?.name}
                                    onClick={() => goToPage(summaryObj.target)}
                                    type={DocumentCardType.compact}
                                    styles={QuickTourStyled.summaryCardStyles}
                                    title={summaryObj?.name}
                                >
                                    <DocumentCardPreview
                                        styles={QuickTourStyled.summaryCardPreviewStyles}
                                        previewImages={[{
                                            previewImageSrc: summaryObj?.image,
                                            imageFit: ImageFit.contain,
                                            height: QuickTourStyled.summaryPreviewImageHeight,
                                            width: QuickTourStyled.summaryPreviewImageWidth
                                        }]} />
                                    <DocumentCardDetails>
                                        <DocumentCardTitle
                                            title={summaryObj?.summary}
                                            aria-label={summaryObj?.title}
                                            shouldTruncate
                                            styles={QuickTourStyled.summaryCardTitleStyles}
                                        />
                                    </DocumentCardDetails>
                                </DocumentCard>
                            </Stack.Item>
                        </>
                    ))}
                </Stack>
            </>
        )
    }

    return (
        <>
            {(teachingBubbleVisible && readQuickTours.length === 0) && (
                <TeachingBubble
                    calloutProps={{ directionalHint: DirectionalHint.bottomCenter }}
                    target={`#${buttonId}`}
                    isWide={true}
                    hasCloseButton={true}
                    closeButtonAriaLabel="Close"
                    onDismiss={toggleTeachingBubbleVisible}
                    headline="Want to see quick tours again?"
                    focusTrapZoneProps={QuickTourStyled.focusTrapZoneProps}
                >
                    Navigate to the FAQ page where you can find all current and previous quicktours available for viewing at anytime!
                </TeachingBubble>
            )}
            <div>
                <Dialog
                    hidden={!isQuickTourOpen}
                    onDismiss={handleDismiss}
                    dialogContentProps={dialogContentProps}
                    modalProps={modalProps}
                >
                    <Stack horizontalAlign='start' tokens={QuickTourStyled.containerStackTokens} styles={QuickTourStyled.stackStyles}>
                        <Stack.Item>
                            {(quickTourSlides[currentpage]?.image) &&
                                <img
                                    src={quickTourSlides[currentpage]?.image}
                                    alt={quickTourSlides[currentpage]?.title}
                                    style={dimensions.width >= 706 ? QuickTourStyled.imgStyles.desktop : QuickTourStyled.imgStyles.mobile}
                                />}
                        </Stack.Item>

                        <Stack verticalAlign='space-between' styles={QuickTourStyled.stackStyles}>
                            <Stack.Item styles={QuickTourStyled.stackHeaderStyles}>
                                <h1 style={QuickTourStyled.titleStyles}>{dialogTitle}</h1>
                                <br />
                                {(
                                    (currentpage === 0 && summaryPresent) ?
                                        renderSummaries() :
                                        <p style={QuickTourStyled.subTextStyles}>{dialogSubText}</p>
                                )}
                            </Stack.Item>
                            <Stack.Item>
                                <DialogFooter>
                                    <Stack horizontal styles={QuickTourStyled.footerStackStyles} tokens={QuickTourStyled.horizontalGapStackTokens}>
                                        <Stack.Item align='center' styles={QuickTourStyled.footerItemStyles}>
                                            <Stack
                                                horizontal
                                                horizontalAlign="start"
                                                tokens={QuickTourStyled.horizontalGapStackTokens}>
                                                <DefaultButton
                                                    onClick={handleDismiss}
                                                    text="Not Now"
                                                    styles={QuickTourStyled.buttonStyles} />
                                            </Stack>
                                        </Stack.Item>
                                        <Stack.Item align='center' styles={QuickTourStyled.footerItemStyles}>
                                            <p style={QuickTourStyled.pageCounterStyles}>{currentpage + 1} of {quickTourSlides?.length}</p>
                                        </Stack.Item>
                                        <Stack.Item align='center' styles={QuickTourStyled.footerItemStyles}>
                                            <Stack
                                                horizontal
                                                horizontalAlign="end"
                                                tokens={QuickTourStyled.horizontalGapStackTokens}>
                                                {(currentpage !== 0 &&
                                                    <DefaultButton
                                                        onClick={goToPreviousPage}
                                                        text="Back"
                                                        styles={QuickTourStyled.buttonStyles} />)}
                                                {(currentpage !== quickTourSlides.length - 1 &&
                                                    <PrimaryButton
                                                        onClick={goToNextPage}
                                                        text="Next"
                                                        styles={QuickTourStyled.buttonStyles} />)}
                                            </Stack>
                                        </Stack.Item>
                                    </Stack>
                                </DialogFooter>
                            </Stack.Item>
                        </Stack>
                    </Stack>
                </Dialog>
            </div>
        </>
    )

}
