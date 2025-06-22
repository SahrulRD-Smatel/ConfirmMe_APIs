using AutoMapper;
using ConfirmMe.Dto;
using ConfirmMe.Models;

namespace ConfirmMe.Extensions
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApprovalFlow, InboxItemDto>()
                .ForMember(dest => dest.ApprovalRequestId, opt => opt.MapFrom(src => src.ApprovalRequestId))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.ApprovalRequest.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.ApprovalRequest.Description))
                .ForMember(dest => dest.RequestedById, opt => opt.MapFrom(src => src.ApprovalRequest.RequestedByUser.FullName))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.ApprovalRequest.CreatedAt))
                .ForMember(dest => dest.CurrentStep, opt => opt.MapFrom(src => src.OrderIndex))
                .ForMember(dest => dest.TotalSteps, opt => opt.MapFrom(src => src.ApprovalRequest.ApprovalFlows.Count))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.ApprovalTypeName, opt => opt.MapFrom(src => src.ApprovalRequest.ApprovalType.Name))
                .ForMember(dest => dest.ApprovalRequestId, opt => opt.MapFrom(src => src.Id));


            CreateMap<ApplicationUser, UserDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PositionId, opt => opt.MapFrom(src => src.PositionId))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

            CreateMap<ApprovalRequest, ApprovalRequestDto>();
            CreateMap<ApprovalType, ApprovalTypeStatDto>();
            CreateMap<Attachment, AttachmentDto>();

            CreateMap<ApprovalFlow, ApprovalFlowDto>()
                .ForMember(dest => dest.ApproverName, opt => opt.MapFrom(src => src.Approver.FullName))
                .ForMember(dest => dest.PositionTitle, opt => opt.MapFrom(src => src.Position.Title))
                .ForMember(dest => dest.Remark, opt => opt.MapFrom(src => src.Remark));
        }
    }
}
