using AutoMapper;
using Messenger.Models;
using Messenger.ViewModels;

namespace Messenger.Mappings;
public class UserProfile: Profile
{
    public UserProfile()
    {
        CreateMap<User, UserViewModel>()
        .ForMember(dst => dst.UserName, opt => opt.MapFrom(x=> x.UserName))
        .ForMember(dst => dst.Id, opt => opt.MapFrom(x => x.IntId))
        .ForMember(dst => dst.Email, opt => opt.MapFrom(x => x.Email))
        .ForMember(dst => dst.Avatar, opt => opt.MapFrom(x => x.Avatar));
        CreateMap<UserViewModel, User>();
    }
}