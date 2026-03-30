using AutoMapper;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.CFS.Approvals.Model.MapperProfiles
{
    public class AdobeSignToARXProfile : Profile
    {
        public AdobeSignToARXProfile()
        {
            // Top-level mapping
            CreateMap<AdobeSignEvent, ApprovalRequestExpressionExt>()
                .ForMember(dest => dest.Operation, opt => opt.MapFrom(src => MapAdobeEventTypeToOperation(src.Event, src.Agreement.Status, src)))
                .ForMember(dest => dest.OperationDateTime, opt => opt.MapFrom(src => src.EventDate))
                .ForMember(dest => dest.RefreshDetails, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.ApprovalIdentifier, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.Approvers, opt => opt.MapFrom(src => MapApprovers(src)))
                .ForMember(dest => dest.DeleteFor, opt => opt.MapFrom(src => MapDeleteFor(src)))
                .ForMember(dest => dest.ActionDetail, opt => opt.MapFrom(src => MapActionDetails(src)))
                .ForMember(dest => dest.NotificationDetail, opt => opt.MapFrom(src => MapNotification(src)))
                .ForMember(dest => dest.AdditionalData, opt => opt.MapFrom(src => new Dictionary<string, string>
                {
                { "RoutingId", src.WebhookNotificationId }
                }))
                .ForMember(dest => dest.SummaryData, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.DetailsData, opt => opt.Ignore())
                .ForMember(dest => dest.Telemetry, opt => opt.MapFrom(src => new Microsoft.CFS.Approvals.Contracts.DataContracts.ApprovalsTelemetry
                {
                    Tcv = src.WebhookNotificationId,
                    Xcv = src.Agreement.Id
                }));

            // ApprovalIdentifier mapping
            CreateMap<AdobeSignEvent, ApprovalIdentifier>()
                .ForMember(dest => dest.DisplayDocumentNumber, opt => opt.MapFrom(src => src.Agreement.Id))
                .ForMember(dest => dest.DocumentNumber, opt => opt.MapFrom(src => src.Agreement.Id))
                .ForMember(dest => dest.FiscalYear, opt => opt.Ignore());

            // SummaryData mapping
            CreateMap<AdobeSignEvent, SummaryJson>()
                .ForMember(dest => dest.UnitValue, opt => opt.MapFrom(src => src.Agreement.Name))
                .ForMember(dest => dest.UnitOfMeasure, opt => opt.MapFrom(src => "-"))
                .ForMember(dest => dest.Title, opt => opt.Ignore())
                .ForMember(dest => dest.SubmittedDate, opt => opt.MapFrom(src => src.Agreement.CreatedDate))
                .ForMember(dest => dest.ApprovalIdentifier, opt => opt.MapFrom(src => new ApprovalIdentifier
                {
                    DisplayDocumentNumber = src.Agreement.Id,
                    DocumentNumber = src.Agreement.Id,
                    FiscalYear = string.Empty
                }))
                .ForMember(dest => dest.Submitter, opt => opt.MapFrom(src => new User
                {
                    Alias = src.Agreement.SenderEmail.GetAliasFromUPN(),
                    Name = src.Agreement.SenderEmail
                }))
                .ForMember(dest => dest.ApprovalHierarchy, opt => opt.MapFrom(src => MapApprovalHierarchy(src)))
                .ForMember(dest => dest.ApprovalActionsApplicable, opt => opt.MapFrom(src => new List<string> { src.ActionType }))
                .ForMember(dest => dest.AdditionalData, opt => opt.MapFrom(src => new Dictionary<string, string>
                {
                    { "MS_IT_Comments", src.Agreement.Message },
                    { "ApproveByDate", src.Agreement.ExpirationTime.ToString("yyyy-MM-ddThh:mm:ss.ffZ") }
                }))
                .ForMember(dest => dest.ApproverNotes, opt => opt.MapFrom(src => src.Agreement.Message))
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => MapAttachments(src.Agreement.DocumentsInfo)));
        }

        private NotificationDetail MapNotification(AdobeSignEvent adobeSignEvent)
        {
            NotificationDetail notificationDetail = null;
            var eventType = adobeSignEvent.Event.ParseEnum<AdobeEventType>();
            var participantsInfo = adobeSignEvent.Agreement.ParticipantSetsInfo?.ParticipantSets?.Where(p => p.Status.Equals("WAITING_FOR_MY_SIGNATURE"))?.ToList();
            var memberInfos = participantsInfo != null && participantsInfo.Count > 0 ? participantsInfo.SelectMany(p => p.MemberInfos).Where(m => m.Status == "ACTIVE").ToList() : new List<MemberInfo>();
            switch (eventType)
            {
                case AdobeEventType.AGREEMENT_EXPIRATION_UPDATED:
                case AdobeEventType.AGREEMENT_MODIFIED:
                case AdobeEventType.AGREEMENT_DOCUMENTS_DELETED:
                case AdobeEventType.AGREEMENT_ACTION_DELEGATED:
                case AdobeEventType.AGREEMENT_ACTION_REPLACED_SIGNER:
                case AdobeEventType.AGREEMENT_PARTICIPANT_REPLACED:
                case AdobeEventType.AGREEMENT_ACTION_REQUESTED:
                    if (memberInfos != null && memberInfos.Count() > 0)
                        notificationDetail = new NotificationDetail
                        {
                            To = string.Join(";", memberInfos.Select(m => m.Email)),
                            Cc = adobeSignEvent.ActingUserEmail,
                            Bcc = string.Empty,
                            TemplateKey = "PendingApproval",
                            SendNotification = true,
                            Reminder = null
                        };
                    else
                        notificationDetail = new NotificationDetail
                        {
                            To = adobeSignEvent.ParticipantUserEmail,
                            Cc = adobeSignEvent.ActingUserEmail,
                            Bcc = string.Empty,
                            TemplateKey = "PendingApproval",
                            SendNotification = true,
                            Reminder = null
                        };
                    return notificationDetail;
                default:
                    return null;
            }
        }

        private List<Attachment> MapAttachments(DocumentsInfo documentsInfo)
        {
            List<Attachment> attachments = new List<Attachment>();
            if (documentsInfo != null && documentsInfo.Documents != null)
            {
                foreach (var document in documentsInfo.Documents)
                {
                    attachments.Add(new Attachment
                    {
                        ID = document.Id,
                        Name = document.Name,
                        Url = "" // URL mapping logic if available
                    });
                }
            }
            return attachments;
        }

        private int MapAdobeEventTypeToOperation(string eventString, string agreementStatus, AdobeSignEvent adobeSignEvent)
        {
            var eventType = eventString.ParseEnum<AdobeEventType>();

            switch (eventType)
            {
                case AdobeEventType.AGREEMENT_ACTION_REQUESTED:
                    ParticipantSet participantsInfo = adobeSignEvent.Agreement.ParticipantSetsInfo?.ParticipantSets?.Where(p => p.Status.Equals("WAITING_FOR_MY_SIGNATURE"))?.FirstOrDefault();
                    if (adobeSignEvent.Agreement.ParticipantSetsInfo == null || participantsInfo.Order == 1)
                        return (int)ApprovalRequestOperation.Create;
                    else
                        return (int)ApprovalRequestOperation.Update;
                case AdobeEventType.AGREEMENT_MODIFIED:
                case AdobeEventType.AGREEMENT_ACTION_REPLACED_SIGNER:
                case AdobeEventType.AGREEMENT_PARTICIPANT_REPLACED:
                case AdobeEventType.AGREEMENT_ACTION_DELEGATED:
                case AdobeEventType.AGREEMENT_EXPIRATION_UPDATED:
                case AdobeEventType.AGREEMENT_DOCUMENTS_DELETED:
                    return (int)ApprovalRequestOperation.Update;
                case AdobeEventType.AGREEMENT_DELETED:
                case AdobeEventType.AGREEMENT_RECALLED:
                case AdobeEventType.AGREEMENT_REJECTED:
                case AdobeEventType.AGREEMENT_EXPIRED:
                case AdobeEventType.AGREEMENT_WORKFLOW_COMPLETED:
                    return (int)ApprovalRequestOperation.Delete;
                case AdobeEventType.AGREEMENT_ACTION_COMPLETED:
                    if (agreementStatus.Equals("OUT_FOR_SIGNATURE", StringComparison.InvariantCultureIgnoreCase))
                    {
                        List<ParticipantSet> currentParticipantsInfo = adobeSignEvent.Agreement.ParticipantSetsInfo?.ParticipantSets?.Where(p => p.Status.Equals("WAITING_FOR_MY_SIGNATURE")).ToList();
                        if (currentParticipantsInfo != null && currentParticipantsInfo.Count > 0)
                            return (int)ApprovalRequestOperation.Delete;
                        else
                            return (int)ApprovalRequestOperation.Skip;
                    }
                    else
                        return (int)ApprovalRequestOperation.Skip;
                default:
                    return (int)ApprovalRequestOperation.Skip;
            }
        }

        private List<Approver> MapApprovers(AdobeSignEvent adobeSignEvent)
        {
            List<Approver> approvers = new List<Approver>();
            var eventType = adobeSignEvent.Event.ParseEnum<AdobeEventType>();
            var participantsInfo = adobeSignEvent.Agreement.ParticipantSetsInfo?.ParticipantSets?.Where(p => p.Status.Equals("WAITING_FOR_MY_SIGNATURE")).ToList();
            List<MemberInfo> memberInfos = participantsInfo != null && participantsInfo.Count > 0 ? participantsInfo.SelectMany(p => p.MemberInfos).ToList() : new List<MemberInfo>();

            switch (eventType)
            {

                case AdobeEventType.AGREEMENT_ACTION_REPLACED_SIGNER:
                case AdobeEventType.AGREEMENT_PARTICIPANT_REPLACED:
                case AdobeEventType.AGREEMENT_ACTION_DELEGATED:
                    approvers.Add(new Approver
                    {
                        Alias = adobeSignEvent.ParticipantUserEmail.GetAliasFromUPN(),
                        Name = adobeSignEvent.ParticipantUserEmail,
                        UserPrincipalName = adobeSignEvent.ParticipantUserEmail,
                        MailNickname = adobeSignEvent.ParticipantUserEmail.GetAliasFromUPN(),
                        CanEdit = false
                    });
                    return approvers;
                case AdobeEventType.AGREEMENT_ACTION_REQUESTED:
                    foreach (var member in memberInfos)
                    {
                        approvers.Add(new Approver
                        {
                            Alias = member.Email.GetAliasFromUPN(),
                            Name = member.Name,
                            UserPrincipalName = member.Email,
                            MailNickname = member.Email.GetAliasFromUPN(),
                            CanEdit = false
                        });
                    }
                    return approvers;
                case AdobeEventType.AGREEMENT_ACTION_COMPLETED:
                    return null;

                case AdobeEventType.AGREEMENT_EXPIRATION_UPDATED:
                case AdobeEventType.AGREEMENT_MODIFIED:
                case AdobeEventType.AGREEMENT_DOCUMENTS_DELETED:
                    foreach (var member in memberInfos)
                    {
                        approvers.Add(new Approver
                        {
                            Alias = member.Email.GetAliasFromUPN(),
                            Name = member.Name,
                            UserPrincipalName = member.Email,
                            MailNickname = member.Email.GetAliasFromUPN(),
                            CanEdit = false
                        });
                    }
                    return approvers;

                case AdobeEventType.AGREEMENT_DELETED:
                case AdobeEventType.AGREEMENT_RECALLED:
                case AdobeEventType.AGREEMENT_REJECTED:
                case AdobeEventType.AGREEMENT_EXPIRED:
                case AdobeEventType.AGREEMENT_WORKFLOW_COMPLETED:
                    return null;
                default:
                    foreach (var member in memberInfos)
                    {
                        approvers.Add(new Approver
                        {
                            Alias = member.Email.GetAliasFromUPN(),
                            Name = member.Name,
                            UserPrincipalName = member.Email,
                            MailNickname = member.Email.GetAliasFromUPN(),
                            CanEdit = false
                        });
                    }
                    return approvers;
            }
        }

        private List<string> MapDeleteFor(AdobeSignEvent adobeSignEvent)
        {
            List<string> deleteFor = new List<string>();
            var eventType = adobeSignEvent.Event.ParseEnum<AdobeEventType>();
            var participantsInfo = adobeSignEvent.Agreement.ParticipantSetsInfo?.ParticipantSets?.Where(p => p.Status.Equals("WAITING_FOR_MY_SIGNATURE")).ToList();
            List<MemberInfo> memberInfos = participantsInfo != null && participantsInfo.Count > 0 ? participantsInfo.SelectMany(p => p.MemberInfos).ToList() : new List<MemberInfo>();

            switch (eventType)
            {
                case AdobeEventType.AGREEMENT_ACTION_REQUESTED:
                    if (adobeSignEvent.Agreement.ParticipantSetsInfo == null || participantsInfo.FirstOrDefault().Order == 1)
                        return null;
                    else
                    {
                        var oldParticipantInfo = adobeSignEvent.Agreement.ParticipantSetsInfo.ParticipantSets.Where(p => p.Status.Equals("WAITING_FOR_OTHERS") && p.Order == participantsInfo.FirstOrDefault().Order - 1);
                        deleteFor = oldParticipantInfo != null && oldParticipantInfo.Any() ? oldParticipantInfo.SelectMany(p => p.MemberInfos).ToList().Select(m => m.Email).ToList() : new List<string>();
                        return deleteFor;
                    }

                ////TODO :: check the data and correct the logic
                case AdobeEventType.AGREEMENT_ACTION_REPLACED_SIGNER:
                case AdobeEventType.AGREEMENT_PARTICIPANT_REPLACED:
                case AdobeEventType.AGREEMENT_ACTION_DELEGATED:

                case AdobeEventType.AGREEMENT_ACTION_COMPLETED:
                    deleteFor.Add(adobeSignEvent.ActingUserEmail);
                    return deleteFor;

                case AdobeEventType.AGREEMENT_MODIFIED:
                case AdobeEventType.AGREEMENT_EXPIRATION_UPDATED:
                case AdobeEventType.AGREEMENT_DOCUMENTS_DELETED:
                    deleteFor.AddRange(memberInfos.Select(m => m.Email));
                    return deleteFor;

                case AdobeEventType.AGREEMENT_DELETED:
                case AdobeEventType.AGREEMENT_RECALLED:
                    var participantsInfoRecalled = adobeSignEvent.Agreement.ParticipantSetsInfo.ParticipantSets.Where(p => p.Status.Equals("CANCELLED")).ToList();
                    deleteFor.AddRange(participantsInfoRecalled.SelectMany(p => p.MemberInfos).Select(m => m.Email));
                    return deleteFor;
                case AdobeEventType.AGREEMENT_REJECTED:
                    deleteFor.Add(adobeSignEvent.ActingUserEmail);
                    return deleteFor;
                case AdobeEventType.AGREEMENT_EXPIRED:
                    var participantsInfoExpired = adobeSignEvent.Agreement.ParticipantSetsInfo.ParticipantSets.Where(p => p.Status.Equals("EXPIRED")).ToList();
                    deleteFor.AddRange(participantsInfoExpired.SelectMany(p => p.MemberInfos).Select(m => m.Email));
                    return deleteFor;
                case AdobeEventType.AGREEMENT_WORKFLOW_COMPLETED:
                    //deleteFor.Add(adobeSignEvent.ActingUserEmail);
                    var participantsInfoCompleted = adobeSignEvent.Agreement.ParticipantSetsInfo.ParticipantSets.Where(p => p.Status.Equals("COMPLETED")).ToList();
                    deleteFor.AddRange(participantsInfoCompleted.SelectMany(p => p.MemberInfos).Select(m => m.Email));
                    return deleteFor;
                default:
                    return null;
            }
        }

        private List<ApprovalHierarchy> MapApprovalHierarchy(AdobeSignEvent adobeSignEvent)
        {
            List<ApprovalHierarchy> approvalHierarchies = new List<ApprovalHierarchy>();
            if (adobeSignEvent.Agreement.ParticipantSetsInfo != null && adobeSignEvent.Agreement.ParticipantSetsInfo.ParticipantSets != null)
            {
                foreach (var participantSet in adobeSignEvent.Agreement.ParticipantSetsInfo.ParticipantSets)
                {
                    var approvers = participantSet.MemberInfos.Select(member => new Contracts.DataContracts.User
                    {
                        Alias = member.Email.GetAliasFromUPN(),
                        Name = member.Name,
                        UserPrincipalName = member.Email,
                        MailNickname = member.Email.GetAliasFromUPN()
                    }).ToList();
                    approvalHierarchies.Add(new ApprovalHierarchy
                    {
                        ApproverType = participantSet.Role,
                        Approvers = approvers
                    });
                }
            }
            else
            {
                var approvers = new List<User>();
                approvers.Add(new User
                {
                    Alias = adobeSignEvent.ParticipantUserEmail.GetAliasFromUPN(),
                    Name = adobeSignEvent.ParticipantUserEmail,
                    UserPrincipalName = adobeSignEvent.ParticipantUserEmail,
                    MailNickname = adobeSignEvent.ParticipantUserEmail.GetAliasFromUPN()
                });
                approvalHierarchies.Add(new ApprovalHierarchy
                {
                    ApproverType = adobeSignEvent.ParticipantRole,
                    Approvers = approvers
                });
            }
            return approvalHierarchies;
        }

        private ActionDetail MapActionDetails(AdobeSignEvent adobeSignEvent)
        {
            var operation = MapAdobeEventTypeToOperation(adobeSignEvent.Event, adobeSignEvent.Agreement.Status, adobeSignEvent);
            if (operation == (int)ApprovalRequestOperation.Delete || operation == (int)ApprovalRequestOperation.Update)
            {
                ActionDetail actionDetails = new ActionDetail()
                {
                    Comment = adobeSignEvent.AgreementCancellationInfo?.Comment,
                    Date = adobeSignEvent.EventDate
                };
                if (adobeSignEvent.ActionType != null)
                    actionDetails.Name = adobeSignEvent.ActionType;
                else if (adobeSignEvent.Event.Equals(AdobeEventType.AGREEMENT_WORKFLOW_COMPLETED.ToString()))
                    actionDetails.Name = "E-signed";
                else if (operation == (int)ApprovalRequestOperation.Update && adobeSignEvent.Event.Equals(AdobeEventType.AGREEMENT_ACTION_REQUESTED.ToString()))
                    actionDetails.Name = "E-signed";
                else
                    actionDetails.Name = adobeSignEvent.Event.ToLower();

                if (adobeSignEvent.Event.Equals(AdobeEventType.AGREEMENT_ACTION_REPLACED_SIGNER.ToString()))
                {
                    var participantsInfo = adobeSignEvent.Agreement.ParticipantSetsInfo.ParticipantSets.Where(p => p.Status.Equals("WAITING_FOR_MY_SIGNATURE")).FirstOrDefault();
                    var oldParticipantInfo = adobeSignEvent.Agreement.ParticipantSetsInfo.ParticipantSets.Where(p => p.Status.Equals("WAITING_FOR_OTHERS") && p.Order == participantsInfo.Order - 1).FirstOrDefault();
                    actionDetails.NewApprover = new User()
                    {
                        Alias = oldParticipantInfo.MemberInfos.FirstOrDefault().Email.GetAliasFromUPN(),
                        Name = oldParticipantInfo.MemberInfos.FirstOrDefault().Name,
                        UserPrincipalName = oldParticipantInfo.MemberInfos.FirstOrDefault().Email,
                        MailNickname = oldParticipantInfo.MemberInfos.FirstOrDefault().Email.GetAliasFromUPN()
                    };
                }
                else
                {
                    actionDetails.ActionBy = new User()
                    {
                        Alias = adobeSignEvent.ActingUserEmail.GetAliasFromUPN(),
                        Name = adobeSignEvent.ActingUserEmail,
                        UserPrincipalName = adobeSignEvent.ActingUserEmail,
                        MailNickname = adobeSignEvent.ActingUserEmail.GetAliasFromUPN()
                    };
                }
                return actionDetails;
            }

            return null;
        }
    }
}
