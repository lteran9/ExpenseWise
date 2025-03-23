using System;
using AutoMapper;
using Core.Entities;

namespace Infrastructure.SqlDatabase
{
   public class GroupProfile : Profile
   {
      public GroupProfile()
      {
         // Database to Entity
         CreateMap<GroupEntity, Group>();
         // Entity to Database
         CreateMap<Group, GroupEntity>();
      }
   }
}