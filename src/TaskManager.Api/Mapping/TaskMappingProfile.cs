using AutoMapper;
using TaskManager.Api.DTOs;
using TaskManager.Core.Entities;

namespace TaskManager.Api.Mapping
{
    public class TaskMappingProfile : Profile
    {
        public TaskMappingProfile()
        {
            // CreateMap<Source, Destination>();
            CreateMap<TaskCreateDto, TaskItem>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<TaskUpdateDto, TaskItem>();
            CreateMap<TaskItem, TaskResponseDto>();
        }
    }
}
