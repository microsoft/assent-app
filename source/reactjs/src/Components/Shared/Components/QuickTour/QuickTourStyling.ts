import {
    IButtonStyles,
    IDialogContentStyles,
    IDocumentCardStyles,
    IDocumentCardTitleStyles,
    IFocusTrapZoneProps,
    IModalStyles,
    IStackStyles,
    IStackTokens,
    IDocumentCardPreviewStyles
} from "@fluentui/react";

export const summaryPreviewImageHeight = window.innerWidth < 705 ? 25 : 35; 
export const summaryPreviewImageWidth = window.innerWidth < 705 ? 25 : 35;  

export const summaryCardPreviewStyles: IDocumentCardPreviewStyles = {
    root: {
        selectors: {
            ['@media (min-width: 706px)']: {
                padding: '10px',
                display: 'flex',
                justifyContent: 'center',
                alignItems: 'center'
            },
            ['@media (min-width: 319px) and (max-width: 705px)']: {
                padding: '8px',
                display: 'flex',
                justifyContent: 'center',
                alignItems: 'center'
            },
        }
    },
    icon: {
        selectors: {
            ['@media (min-width: 706px)']: {
                margin: '5px',
                ['@media (min-width: 319px) and (max-width: 705px)']: {
                    margin: '3px', 
                },
            }
        }
    },
    previewIcon: "",
    fileList: "",
    fileListIcon: "",
    fileListLink: "",
    fileListOverflowText: ""
};

export const containerStackTokens: IStackTokens = { childrenGap: 5 };
export const horizontalGapStackTokens: IStackTokens = {
    childrenGap: 10,
    padding: 10,
};

export const focusTrapZoneProps: IFocusTrapZoneProps = {
    isClickableOutsideFocusTrap: true,
    forceFocusInsideTrap: false,
    disableRestoreFocus: true,
    focusPreviouslyFocusedInnerElement: false,
    firstFocusableSelector: "firstFocusableTarget",
    ignoreExternalFocusing: true,
};

export const modalPropsStyles: IModalStyles = {
    main: {
        selectors: {
            ['@media (min-width: 706px)']: {
                width: '700px',
                height: '600px',
                maxWidth: '700px',
                maxHeight: '600px',
                borderRadius: '15px',
                overflow: 'hidden',
                display: 'flex',
            },
            ['@media (min-width: 374px) and (max-width: 705px)']: {
                width: '350px',
                height: '480px',
                maxWidth: '700',
                maxHeight: '600px',
                borderRadius: '15px',
                overflow: 'hidden',
                display: 'flex',
            },
             ['@media (min-width: 319px) and (max-width: 373px)']: {
                width: '350px',
                height: '500px',
                maxWidth: '700',
                maxHeight: '600px',
                borderRadius: '15px',
                overflow: 'hidden',
                display: 'flex',
            },
        }
    },
    scrollableContent: {
        selectors: {
            ['@media (min-width: 706px)']: {
                display: 'flex',
                overflow: 'hidden',
                width: '700px',
                height: '600px',

            },
            ['@media (min-width: 374px) and (max-width: 705px)']: {
                display: 'flex',
                overflow: 'hidden',
                width: '350px',
                height: '480px',
            },
            ['@media (min-width: 319px) and (max-width: 373px)']: {
                display: 'flex',
                overflow: 'hidden',
                width: '350px',
                height: '500px',
            },
        }
    },
    root: "",
    layer: "",
    keyboardMoveIconContainer: "",
    keyboardMoveIcon: ""
};


export const stackStyles: IStackStyles = {
    root: {
        height: 'inherit',
    },
};

export const summaryStackStyles: IStackStyles = {
    root: {
        selectors: {
            ['@media (min-width: 706px)']: {
                height: 400,
            },
            ['@media (min-width: 374px) and (max-width: 705px)']: {
                height: 245,
            },
            ['@media (min-width: 319px) and (max-width: 373px)']: {
                height: 265,
            },
        },
    },
    inner: {
        justifyContent: 'center',
        flexDirection: 'row',
        overflow: 'scroll',
        alignContent: 'flex-start',

    }
};


export const summaryCardTitleStyles: IDocumentCardTitleStyles = {
    root: {
        selectors: {
            ['@media (min-width: 706px)']: {
                fontSize: "14px !important",
                height: 100,
                lineHeight: "20px !important",
            },
            ['@media (min-width: 374px) and (max-width: 705px)']: {
                fontSize: "10px !important",
                height: 60,
                lineHeight: "10px !important",
            },
            ['@media (min-width: 319px) and (max-width: 373px)']: {
                fontSize: "10px !important",
                height: 60,
                lineHeight: "10px !important",
            },
        }
    },
}
export const summaryCardTextStyles: IDocumentCardTitleStyles = {
    root: {
        selectors: {
            ['@media (min-width: 706px)']: {
                fontSize: "14px !important",
                height: 100,
                lineHeight: "25px !important",
            },
            ['@media (min-width: 374px) and (max-width: 705px)']: {
                fontSize: "10px !important",
                height: 60,
                lineHeight: "10px !important",
            },
            ['@media (min-width: 319px) and (max-width: 373px)']: {
                fontSize: "10px !important",
                height: 60,
                lineHeight: "10px !important",
            },
        }
    },
}


export const summaryCardStyles: IDocumentCardStyles = {
    root: {
        selectors: {
            ['@media (min-width: 706px)']: {
                maxWidth: 800,
                width: 550,
                height: 55,
                marginBottom: 10
            },
            ['@media (min-width: 374px) and (max-width: 705px)']: {
                maxWidth: 800,
                width: 300,
                marginBottom: 5,
                height: 50,
            },
             ['@media (min-width: 319px) and (max-width: 373px)']: {
                maxWidth: 800,
                width: 230,
                marginBottom: 5,
                height: 50,
            },
        }
    },
};


export const footerStackStyles: IStackStyles = {
    root: {
        selectors: {
            ['@media (min-width: 706px)']: {
                width: 700,
                height: 'inherit'
            },
            ['@media (min-width: 374px) and (max-width: 705px)']: {
                width: 345,
                padding: "0px 5px 0px 5px",
                height: 'inherit'
            },
             ['@media (min-width: 319px) and (max-width: 373px)']: {
                width: 280,
                padding: "0px 5px 0px 0px",
                height: 'inherit'
            },
        },
    }
};

export const footerItemStyles: IStackStyles = {
    root: {
        selectors: {
            ['@media (min-width: 706px)']: {
                width: 680 / 3,
            },
            ['@media (min-width: 319px) and (max-width: 705px)']: {
                width: 380 / 3,
            },
        }
    }
}


export const stackHeaderStyles: IStackStyles = {
    root: {
        padding: '15px 15px 0px 15px',
    }
}


export const dialogContentStyles: IDialogContentStyles = {
    content: {
        selectors: {
            ['@media (min-width: 319px)']: {
                padding: '0px 0px 0px 0px',
                height: '100%',
                overflow: 'hidden'
            },
            ['@media (min-width: 706px)']: {
                width: 700,
            },
            ['@media (min-width: 319px) and (max-width: 705px)']: {
                width: 350,
                top: 100,
            },
        }
    },
    header: {
        selectors: {

            ['@media (min-width: 319px)']: {
                display: 'none'
            }
        }
    },
    inner: {
        selectors: {
            ['@media (min-width: 319px)']: {
                padding: '0px 0px 0px 0px',
                height: '100%'
            },
            ['@media (min-width: 706px)']: {
                width: 700,
            },
            ['@media (min-width: 374px) and (max-width: 705px)']: {
                width: 350,
            },
            ['@media (min-width: 319px) and (max-width: 374px)']: {
                width: 288,
            },
        }
    },
    innerContent: {
        selectors: {
            ['@media (min-width: 319px)']: {
                height: '100%',
                overflow: 'hidden'
            }
        }
    },
    subText: "",
    button: "",
    title: "",
    topButton: ""
};

export const buttonStyles: IButtonStyles = {
    root: {
        selectors: {
            ['@media (min-width: 706px)']: {
                width: 90,
                paddingRight: 10,
                paddingLeft: 10,
            },
            ['@media (min-width: 374px) and (max-width: 705px)']: {
                width: 70,
                minWidth: "30px !important",
                fontSize: "10px !important",
            },
            ['@media (min-width: 319px) and (max-width: 374px)']: {
                width: 50,
                minWidth: "30px !important",
                fontSize: "10px !important",
            },
        }

    }
}

export const titleStyles = {
    fontSize: '2em',
    margin: '0',
    padding: '0'
};

export const subTextStyles = {
    fontSize: '14px',
    margin: '0',
    padding: '0'
};

export const pageCounterStyles = {
    textAlign: 'center' as const,
    margin: '0',
    padding: '0'
};

export const imgStyles = {
    desktop: {
        width: '700px',
        height: '350px'
    },
    mobile: {
        width: '350px',
        height: '175px'
    }
};






