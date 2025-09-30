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
            CreateMap<TaskCreateDto, TaskItem>();
            CreateMap<TaskUpdateDto, TaskItem>();
            CreateMap<TaskItem, TaskResponseDto>();
        }
    }
}
